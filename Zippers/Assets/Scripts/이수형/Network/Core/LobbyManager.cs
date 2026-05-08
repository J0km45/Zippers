using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Multiplayer;

/// <summary>
/// [PRODUCTION] 세션(Lobby + Relay + NGO 통합) 진입/퇴장과 게임 시작을 총괄하는 싱글톤.
/// Unity Services Multiplayer Sessions API 위에서 동작.
///
/// 샘플(LobbyManagerSample) 대비 변경점:
/// - 닉네임(PlayerName) 제거. 대신 PlayerInfo 기반 PlayerProperty 계약 (Class / Ready).
/// - OnError 이벤트 — 사용자 노출용 채널 (Debug 로그와 분리).
/// - DebugTool(DebugType.Network) 로 로그 통일.
/// - 슬롯 관리: SessionProperty["Slots"] JSON, 호스트 권위.
/// - 클래스 중복 검증: LobbyHostAuthority(NetworkBehaviour) 의 ServerRpc 통과 시에만 PlayerProperty 기록.
/// - Ready ↔ Class 양방향 게이트: PlayerClass==None 면 Ready 불가, IsReady=true 면 Class 변경 불가.
/// - 게임 종료 복귀 시 PlayerClass 도 None 으로 리셋.
/// </summary>
public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    [SerializeField] private LobbySettings _settings;

    // SDK 내부 transient 실패(첫 NetworkManager 시작 task canceled 등) 자동 재시도용
    private const int JOIN_MAX_RETRY = 1;
    private const int JOIN_RETRY_DELAY_MS = 500;

    // 세션 진입 직후 NGO Network.State == Started 검증 폴링 시간
    private const float NGO_START_WAIT_SEC = 6f;

    private ISession _session;
    private bool _isStartingGame;
    private bool _isQuitting;
    private float _lastGameEndRealtime = float.NegativeInfinity;
    private Coroutine _restartCooldownRoutine;

    // 로컬 플레이어가 세션 진입 시 처음 들고 들어가는 정보. 세션 진입 후엔 PlayerProperty 가 권위.
    private PlayerInfo _pendingLocalInfo = PlayerInfo.Unassigned;

    // 슬롯 매핑(SessionProperty["Slots"] JSON) 의 로컬 캐시. 키=슬롯 인덱스, 값=ISession.Players[i].Id.
    // 호스트는 자기 권위 영역으로 갱신하고, 모든 클라(호스트 포함)는 SessionPropertiesChanged / Changed 이벤트로 재구성.
    private readonly Dictionary<int, string> _slotCache = new Dictionary<int, string>();

    // 호스트의 슬롯 갱신 작업을 직렬화하기 위한 in-flight Task. 동시에 두 개의 PlayerJoined 가 와도
    // 같은 슬롯을 두 번 배정하지 않도록 ChainSlotUpdate 로 이전 Task await 후 다음 작업 시작.
    private Task _slotUpdateInFlight = Task.CompletedTask;

    /// <summary>로비/게임 시작 흐름의 설정값 ScriptableObject.</summary>
    public LobbySettings Settings => _settings;

    /// <summary>현재 참가 중인 세션 (없으면 null).</summary>
    public ISession CurrentSession => _session;

    /// <summary>현재 로컬 플레이어가 호스트인지.</summary>
    public bool IsHost => _session != null && _session.IsHost;

    /// <summary>게임 시작 시점에 확정된 세션 인원수. 게임 씬 합류 판정용.</summary>
    public int ExpectedPlayerCount { get; private set; }

    /// <summary>호스트가 현재 세션 기준으로 게임을 시작할 수 있는지.</summary>
    public bool CanHostStartGame
    {
        get
        {
            if (!IsHost || _session == null || _isStartingGame) return false;
            if (Time.realtimeSinceStartup - _lastGameEndRealtime < _settings.GameRestartCooldownSec) return false;
            if (_session.PlayerCount < _settings.MinPlayersToStart) return false;
            return AreNonHostPlayersReady();
        }
    }

    public event Action<ISession> OnSessionUpdated;
    public event Action OnSessionLeft;
    public event Action OnGameStarting;

    /// <summary>게임 재시작 쿨다운이 끝난 시점에 1회. 시간 기반 조건 변화 전파.</summary>
    public event Action OnRestartCooldownEnded;

    /// <summary>사용자에게 노출할 수 있는 에러 메시지 채널. UI 가 구독해서 토스트/상태 텍스트 등으로 처리.</summary>
    public event Action<string> OnError;

    // ─────────────────────────────────────────────────────────────────
    // Lifecycle
    // ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        SetSingleton();
        Application.wantsToQuit += OnWantsToQuit;
    }

    private void OnDestroy()
    {
        Application.wantsToQuit -= OnWantsToQuit;
        if (Instance == this) Instance = null;
    }

    // leave 완료까지 종료 보류, 완료 후 Application.Quit() 재호출
    private bool OnWantsToQuit()
    {
        if (_session == null || _isQuitting) return true;
        _isQuitting = true;
        _ = LeaveAndQuitAsync();
        return false;
    }

    private async Task LeaveAndQuitAsync()
    {
        try
        {
            await _session.LeaveAsync();
        }
        catch (Exception e)
        {
            DebugTool.Warning($"quit-leave 실패: {e.Message}", DebugType.Network);
        }
        Application.Quit();
    }

    // ─────────────────────────────────────────────────────────────────
    // Local PlayerInfo
    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// 세션 진입 전에 호출되는 로컬 플레이어 초기 정보 설정.
    /// 세션 진입 시 BuildLocalPlayerProperties() 가 이 값을 직렬화해 PlayerProperty 로 보냄.
    /// </summary>
    public void SetLocalPlayerInfo(PlayerInfo info)
    {
        _pendingLocalInfo = info;
    }

    // ─────────────────────────────────────────────────────────────────
    // Session: Query / Create / Join
    // ─────────────────────────────────────────────────────────────────

    /// <summary>공개 세션 목록 조회. 실패 시 빈 리스트.</summary>
    public async Task<IList<ISessionInfo>> QuerySessionsAsync()
    {
        try
        {
            QuerySessionsOptions options = new QuerySessionsOptions
            {
                Count = 25,
                FilterOptions = new List<FilterOption>
                {
                    new FilterOption(FilterField.AvailableSlots, "0", FilterOperation.Greater),
                    new FilterOption(FilterField.IsLocked, "true", FilterOperation.NotEqual)
                }
            };
            QuerySessionsResults results = await MultiplayerService.Instance.QuerySessionsAsync(options);
            return results.Sessions;
        }
        catch (Exception e)
        {
            DebugTool.Error($"목록 조회 실패: {e.Message}", DebugType.Network);
            RaiseError("방 목록을 불러오지 못했습니다.");
            return new List<ISessionInfo>();
        }
    }

    /// <summary>세션 생성 후 자동 진입. Relay + NGO Host 동시 시작.</summary>
    public async Task<bool> CreateSessionAsync(string sessionName)
    {
        for (int attempt = 0; attempt <= JOIN_MAX_RETRY; attempt++)
        {
            await EnsureCleanNetworkStateAsync();
            try
            {
                string region = string.IsNullOrWhiteSpace(_settings.RelayRegion) ? null : _settings.RelayRegion;
                SessionOptions options = new SessionOptions
                {
                    Name = sessionName,
                    MaxPlayers = _settings.MaxPlayers,
                    IsPrivate = false,
                    PlayerProperties = BuildLocalPlayerProperties()
                }.WithRelayNetwork(region);
                _session = await MultiplayerService.Instance.CreateSessionAsync(options);
                if (!await VerifyNgoStartedOrCleanupAsync())
                {
                    if (attempt < JOIN_MAX_RETRY) continue;
                    return false;
                }
                BindSessionEvents(_session);
                // 진입 직후 SessionProperty 초기 상태로 캐시 1회 채움 (이후엔 이벤트로 갱신).
                RebuildSlotCacheFromSession();
                // 호스트 자신을 슬롯 0 으로 초기 등재. PlayerJoined 가 호스트 본인에 대해 발화하지 않을 수 있어 명시적으로 1회 기록.
                _slotUpdateInFlight = ChainSlotUpdate(_slotUpdateInFlight, InitializeSlotsAsHostAsync);
                OnSessionUpdated?.Invoke(_session);
                return true;
            }
            catch (Exception e) when (attempt < JOIN_MAX_RETRY && IsTransientNgoError(e))
            {
                DebugTool.Warning($"생성 일시 실패 - 자동 재시도: {e.Message}", DebugType.Network);
                await Task.Delay(JOIN_RETRY_DELAY_MS);
            }
            catch (Exception e)
            {
                DebugTool.Error($"생성 실패: {e.Message}", DebugType.Network);
                RaiseError("방을 만들지 못했습니다.");
                return false;
            }
        }
        return false;
    }

    /// <summary>세션 ID 기반 참여.</summary>
    public async Task<bool> JoinSessionByIdAsync(string sessionId)
    {
        return await JoinInternalAsync(opts => MultiplayerService.Instance.JoinSessionByIdAsync(sessionId, opts), "참여");
    }

    /// <summary>조인 코드 기반 참여.</summary>
    public async Task<bool> JoinSessionByCodeAsync(string sessionCode)
    {
        return await JoinInternalAsync(opts => MultiplayerService.Instance.JoinSessionByCodeAsync(sessionCode, opts), "코드 참여");
    }

    /// <summary>빈 자리가 있는 임의 세션 빠른 참여. (Query + ID 우회 — MatchmakeSessionAsync 의 transient 이슈 회피)</summary>
    public async Task<bool> QuickJoinAsync()
    {
        try
        {
            IList<ISessionInfo> sessions = await QuerySessionsAsync();
            if (sessions == null || sessions.Count == 0)
            {
                DebugTool.Warning("빠른 참여: 참여 가능한 방 없음", DebugType.Network);
                RaiseError("참여할 수 있는 방이 없습니다.");
                return false;
            }
            return await JoinSessionByIdAsync(sessions[0].Id);
        }
        catch (Exception e)
        {
            DebugTool.Error($"빠른 참여 실패: {e.Message}", DebugType.Network);
            RaiseError("빠른 참여에 실패했습니다.");
            return false;
        }
    }

    // 공통 join 본문. 호출 SDK 메서드만 람다로 받음
    private async Task<bool> JoinInternalAsync(Func<JoinSessionOptions, Task<ISession>> joinCall, string opLabel)
    {
        for (int attempt = 0; attempt <= JOIN_MAX_RETRY; attempt++)
        {
            await EnsureCleanNetworkStateAsync();
            try
            {
                JoinSessionOptions options = new JoinSessionOptions
                {
                    PlayerProperties = BuildLocalPlayerProperties()
                };
                _session = await joinCall(options);
                if (!await VerifyNgoStartedOrCleanupAsync())
                {
                    if (attempt < JOIN_MAX_RETRY) continue;
                    return false;
                }
                BindSessionEvents(_session);
                // 진입 직후 SessionProperty 초기 상태로 캐시 1회 채움 (이후엔 이벤트로 갱신).
                RebuildSlotCacheFromSession();
                OnSessionUpdated?.Invoke(_session);
                return true;
            }
            catch (Exception e) when (attempt < JOIN_MAX_RETRY && IsTransientNgoError(e))
            {
                DebugTool.Warning($"{opLabel} 일시 실패 - 자동 재시도: {e.Message}", DebugType.Network);
                await Task.Delay(JOIN_RETRY_DELAY_MS);
            }
            catch (Exception e)
            {
                DebugTool.Error($"{opLabel} 실패: {e.Message}", DebugType.Network);
                RaiseError($"{opLabel}에 실패했습니다.");
                return false;
            }
        }
        return false;
    }

    // ─────────────────────────────────────────────────────────────────
    // Player property updates (local)
    // ─────────────────────────────────────────────────────────────────

    /// <summary>로컬 플레이어 Ready 상태 갱신. PlayerClass == None 이면 isReady=true 거부.</summary>
    public async Task SetReadyAsync(bool isReady)
    {
        if (_session == null) return;

        // Phase 2-D 게이트: Ready 켜려면 클래스 선택돼있어야 함
        if (isReady)
        {
            PlayerInfo myInfo = GetPlayerInfo(_session.CurrentPlayer);
            if (myInfo.PlayerClass == PlayerClass.None)
            {
                DebugTool.Warning("Ready 거부 - 클래스 미선택 상태", DebugType.Network);
                RaiseError("클래스를 먼저 선택하세요.");
                return;
            }
        }

        try
        {
            await SetLocalPlayerPropertyAsync(LobbyConstants.KEY_PLAYER_READY, isReady ? LobbyConstants.VALUE_TRUE : LobbyConstants.VALUE_FALSE);
            OnSessionUpdated?.Invoke(_session);
        }
        catch (Exception e)
        {
            DebugTool.Error($"레디 갱신 실패: {e.Message}", DebugType.Network);
            RaiseError("준비 상태를 변경하지 못했습니다.");
        }
    }

    /// <summary>
    /// 로컬 플레이어가 클래스를 선택/변경/취소.
    ///
    /// 동작:
    /// - playerClass == None : 취소. 검증 없이 즉시 PlayerProperty 기록.
    /// - playerClass != None : 호스트 LobbyHostAuthority 의 ServerRpc 로 중복 검증.
    ///                          승인 시에만 PlayerProperty 기록.
    ///
    /// 게이트:
    /// - 본인 IsReady == true : 거부 (Ready 해제 후 변경 가능).
    ///
    /// 모델 spawn/despawn 은 LobbyHostAuthority 가 PlayerPropertiesChanged 로 자동 처리.
    /// </summary>
    public async Task<bool> SetClassAsync(PlayerClass playerClass)
    {
        if (_session == null) return false;

        // Phase 2-E 게이트: Ready 중엔 클래스 변경 불가
        PlayerInfo myInfo = GetPlayerInfo(_session.CurrentPlayer);
        if (myInfo.IsReady)
        {
            DebugTool.Warning("클래스 변경 거부 - Ready 상태", DebugType.Network);
            RaiseError("Ready 해제 후 클래스를 변경할 수 있습니다.");
            return false;
        }

        // Phase 2-F: 취소(None) 는 검증 없이 즉시 기록
        if (playerClass == PlayerClass.None)
        {
            return await WriteLocalClassAsync(PlayerClass.None);
        }

        // Phase 2-C: 호스트 ServerRpc 검증
        if (LobbyHostAuthority.Instance == null)
        {
            DebugTool.Error("LobbyHostAuthority.Instance 가 null - LobbyScene 안의 NetworkObject 미배치 또는 NGO 미동기화", DebugType.Network);
            RaiseError("로비 권위자가 없습니다.");
            return false;
        }

        bool approved = await LobbyHostAuthority.Instance.RequestClassChangeAsync(playerClass);
        if (!approved)
        {
            DebugTool.Warning($"클래스 {playerClass} 거부됨 (점유 중 또는 동시 요청 race)", DebugType.Network);
            RaiseError("이미 선택된 클래스입니다.");
            return false;
        }

        return await WriteLocalClassAsync(playerClass);
    }

    // 내부: 본인 PlayerProperty 의 Class 만 쓰기 (검증/게이트 통과 후 호출).
    private async Task<bool> WriteLocalClassAsync(PlayerClass playerClass)
    {
        try
        {
            _session.CurrentPlayer.SetProperty(
                LobbyConstants.KEY_PLAYER_CLASS,
                new PlayerProperty(((int)playerClass).ToString(), VisibilityPropertyOptions.Member));
            await _session.SaveCurrentPlayerDataAsync();
            OnSessionUpdated?.Invoke(_session);
            return true;
        }
        catch (Exception e)
        {
            DebugTool.Error($"클래스 갱신 실패: {e.Message}", DebugType.Network);
            RaiseError("클래스 선택에 실패했습니다.");
            return false;
        }
    }

    private async Task SetLocalPlayerPropertyAsync(string key, string value)
    {
        if (_session == null) return;
        _session.CurrentPlayer.SetProperty(key, new PlayerProperty(value, VisibilityPropertyOptions.Member));
        await _session.SaveCurrentPlayerDataAsync();
    }

    // ─────────────────────────────────────────────────────────────────
    // Game start / return / leave
    // ─────────────────────────────────────────────────────────────────

    /// <summary>호스트만 호출. 세션 잠금 후 NGO 게임 씬 동기화 로드.</summary>
    public async Task<bool> TryStartGameAsHostAsync()
    {
        if (!IsHost || _session == null || _isStartingGame) return false;
        if (Time.realtimeSinceStartup - _lastGameEndRealtime < _settings.GameRestartCooldownSec) return false;
        if (_session.PlayerCount < _settings.MinPlayersToStart || !AreNonHostPlayersReady()) return false;

        _isStartingGame = true;
        ExpectedPlayerCount = _session.PlayerCount;

        try
        {
            IHostSession host = _session.AsHost();
            host.IsLocked = true;
            await host.SavePropertiesAsync();
            OnGameStarting?.Invoke();

            if (!SceneLoader.LoadNetworked(SceneId.Game))
            {
                _isStartingGame = false;
                RaiseError("게임 씬 로드에 실패했습니다.");
                return false;
            }
            return true;
        }
        catch (Exception e)
        {
            DebugTool.Error($"호스트 게임 시작 실패: {e.Message}", DebugType.Network);
            _isStartingGame = false;
            RaiseError("게임을 시작하지 못했습니다.");
            return false;
        }
    }

    /// <summary>게임 종료 후 현재 세션을 유지한 채 로비 씬으로 복귀.</summary>
    public async Task ReturnToRoomAsync()
    {
        _isStartingGame = false;
        _lastGameEndRealtime = Time.realtimeSinceStartup;
        StartRestartCooldownWatch();

        if (IsHost && _session != null)
        {
            try
            {
                IHostSession host = _session.AsHost();
                host.IsLocked = false;
                await host.SavePropertiesAsync();
            }
            catch (Exception e)
            {
                DebugTool.Warning($"게임 종료 후 잠금 해제 실패: {e.Message}", DebugType.Network);
            }
        }

        // 모든 멤버: 자기 ready / class 리셋. 다른 플레이어 PlayerProperty 는 호스트도 직접 못 바꾸므로 각자 리셋.
        try
        {
            await SetLocalPlayerPropertyAsync(LobbyConstants.KEY_PLAYER_READY, LobbyConstants.VALUE_FALSE);
        }
        catch (Exception e)
        {
            DebugTool.Warning($"레디 해제 실패: {e.Message}", DebugType.Network);
        }
        try
        {
            // Phase 9-A: 게임 종료 후 모든 플레이어의 클래스를 None 으로 리셋
            await SetLocalPlayerPropertyAsync(LobbyConstants.KEY_PLAYER_CLASS, ((int)PlayerClass.None).ToString());
        }
        catch (Exception e)
        {
            DebugTool.Warning($"클래스 리셋 실패: {e.Message}", DebugType.Network);
        }

        OnSessionUpdated?.Invoke(_session);

        if (IsHost)
        {
            SceneLoader.LoadNetworked(SceneId.Lobby);
        }
    }

    /// <summary>현재 세션에서 퇴장.</summary>
    public async Task LeaveSessionAsync()
    {
        if (_session == null) return;
        ISession session = _session;
        UnbindSessionEvents(session);
        _session = null;
        _slotCache.Clear();
        try
        {
            await session.LeaveAsync();
        }
        catch (Exception e)
        {
            DebugTool.Warning($"퇴장 중 예외: {e.Message}", DebugType.Network);
        }
        OnSessionLeft?.Invoke();
    }

    // ─────────────────────────────────────────────────────────────────
    // Internals: NGO clean state / NGO Started polling / transient 판별
    // ─────────────────────────────────────────────────────────────────

    // 진입 전 NGO 잔재 정리 (이전 시도 흔적이 남으면 다음 StartHost/StartClient 가 깨질 수 있음)
    private async Task EnsureCleanNetworkStateAsync()
    {
        NetworkManager networkManager = NetworkManager.Singleton;
        if (networkManager == null) return;
        if (!networkManager.IsListening && !networkManager.IsClient && !networkManager.IsServer) return;

        DebugTool.Warning("이전 NGO 잔재 감지 - Shutdown 후 진행", DebugType.Network);
        networkManager.Shutdown();

        for (int i = 0; i < 30; i++)
        {
            await Task.Yield();
            if (!networkManager.IsListening && !networkManager.IsClient && !networkManager.IsServer) break;
        }
    }

    private static bool IsTransientNgoError(Exception e)
    {
        if (e == null || e.Message == null) return false;
        return e.Message.Contains("Failed to start NetworkManager")
            || e.Message.Contains("task was canceled")
            || e.Message.Contains("A task was canceled");
    }

    private async Task<bool> VerifyNgoStartedOrCleanupAsync()
    {
        if (_session == null) return false;

        float deadline = Time.realtimeSinceStartup + NGO_START_WAIT_SEC;
        while (Time.realtimeSinceStartup < deadline)
        {
            if (_session == null) return false;
            if (_session.Network.State == NetworkState.Started) return true;
            await Task.Yield();
        }

        DebugTool.Error($"세션 진입 후 NGO 비정상 상태: {_session?.Network.State} - 강제 leave", DebugType.Network);
        ISession failed = _session;
        _session = null;
        if (failed != null)
        {
            try { await failed.LeaveAsync(); }
            catch (Exception e) { DebugTool.Warning($"비정상 정리 중 예외: {e.Message}", DebugType.Network); }
        }
        return false;
    }

    // ─────────────────────────────────────────────────────────────────
    // PlayerProperty / SessionProperty helpers
    // ─────────────────────────────────────────────────────────────────

    /// <summary>임의 플레이어의 PlayerProperty 값 헬퍼.</summary>
    public static string GetPlayerProperty(IReadOnlyPlayer player, string key)
    {
        if (player == null || player.Properties == null) return null;
        return player.Properties.TryGetValue(key, out PlayerProperty prop) ? prop.Value : null;
    }

    /// <summary>
    /// 임의 플레이어의 현재 PlayerInfo 스냅샷.
    /// PlayerProperty(Class/Ready) + SessionProperty["Slots"] 캐시에서 SlotIndex 합성.
    /// 호스트가 아직 슬롯을 배정하지 않은 짧은 윈도우에선 SlotIndex == -1 가 반환될 수 있다.
    /// (UI 가 본인 슬롯이 필요하면 WaitForOwnSlotAsync 사용)
    /// </summary>
    public PlayerInfo GetPlayerInfo(IReadOnlyPlayer player)
    {
        if (player == null) return PlayerInfo.Unassigned;

        string classStr = GetPlayerProperty(player, LobbyConstants.KEY_PLAYER_CLASS);
        string readyStr = GetPlayerProperty(player, LobbyConstants.KEY_PLAYER_READY);

        PlayerClass playerClass = ParsePlayerClass(classStr);
        bool isReady = readyStr == LobbyConstants.VALUE_TRUE;

        int slotIndex = FindSlotByPlayerId(player.Id);

        return new PlayerInfo(slotIndex, playerClass, isReady);
    }

    // 슬롯 캐시에서 playerId 의 슬롯 인덱스 역검색. 미등재면 -1.
    private int FindSlotByPlayerId(string playerId)
    {
        if (string.IsNullOrEmpty(playerId)) return -1;
        foreach (KeyValuePair<int, string> kv in _slotCache)
        {
            if (kv.Value == playerId) return kv.Key;
        }
        return -1;
    }

    /// <summary>
    /// 본인(CurrentPlayer) 의 슬롯이 SessionProperty 에 등재될 때까지 대기.
    /// 세션 진입 직후 호스트가 슬롯을 막 쓰는 사이의 짧은 윈도우(보통 1초 미만) 처리용.
    /// 타임아웃 도달 시 -1 반환, 정상 부여 시 슬롯 인덱스 반환.
    /// </summary>
    public async Task<int> WaitForOwnSlotAsync(float timeoutSec = 5f)
    {
        if (_session == null) return -1;
        string ownId = _session.CurrentPlayer?.Id;
        if (string.IsNullOrEmpty(ownId)) return -1;

        float deadline = Time.realtimeSinceStartup + Mathf.Max(0f, timeoutSec);
        while (Time.realtimeSinceStartup < deadline)
        {
            if (_session == null) return -1;
            int slot = FindSlotByPlayerId(ownId);
            if (slot >= 0) return slot;
            await Task.Yield();
        }
        DebugTool.Warning($"WaitForOwnSlotAsync 타임아웃 ({timeoutSec}s): 본인({ownId}) 슬롯 미부여", DebugType.Network);
        return -1;
    }

    private static PlayerClass ParsePlayerClass(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return PlayerClass.None;
        return int.TryParse(raw, out int v) && Enum.IsDefined(typeof(PlayerClass), v)
            ? (PlayerClass)v
            : PlayerClass.None;
    }

    private bool AreNonHostPlayersReady()
    {
        if (_session == null || _session.Players.Count == 0) return false;
        bool hasNonHost = false;
        for (int i = 0; i < _session.Players.Count; i++)
        {
            IReadOnlyPlayer player = _session.Players[i];
            if (player.Id == _session.Host) continue;
            hasNonHost = true;
            string ready = GetPlayerProperty(player, LobbyConstants.KEY_PLAYER_READY);
            if (ready != LobbyConstants.VALUE_TRUE) return false;
        }
        return hasNonHost;
    }

    private Dictionary<string, PlayerProperty> BuildLocalPlayerProperties()
    {
        // _pendingLocalInfo 가 SetLocalPlayerInfo 로 채워져 있으면 그 값을, 아니면 기본값을 직렬화.
        PlayerInfo info = _pendingLocalInfo;

        return new Dictionary<string, PlayerProperty>
        {
            {
                LobbyConstants.KEY_PLAYER_CLASS,
                new PlayerProperty(((int)info.PlayerClass).ToString(), VisibilityPropertyOptions.Member)
            },
            {
                LobbyConstants.KEY_PLAYER_READY,
                new PlayerProperty(info.IsReady ? LobbyConstants.VALUE_TRUE : LobbyConstants.VALUE_FALSE, VisibilityPropertyOptions.Member)
            }
        };
    }

    // ─────────────────────────────────────────────────────────────────
    // Session events (멤버 입퇴장, 속성 변경)
    // ─────────────────────────────────────────────────────────────────

    private void BindSessionEvents(ISession session)
    {
        if (session == null) return;
        session.Changed += OnSessionChanged;
        session.PlayerJoined += OnPlayerJoined;
        session.PlayerHasLeft += OnPlayerHasLeft;
        session.PlayerPropertiesChanged += RaiseSessionUpdated;
        session.SessionPropertiesChanged += OnSessionPropertiesChanged;
        session.RemovedFromSession += HandleSessionGone;
        session.Deleted += HandleSessionGone;
    }

    private void UnbindSessionEvents(ISession session)
    {
        if (session == null) return;
        session.Changed -= OnSessionChanged;
        session.PlayerJoined -= OnPlayerJoined;
        session.PlayerHasLeft -= OnPlayerHasLeft;
        session.PlayerPropertiesChanged -= RaiseSessionUpdated;
        session.SessionPropertiesChanged -= OnSessionPropertiesChanged;
        session.RemovedFromSession -= HandleSessionGone;
        session.Deleted -= HandleSessionGone;
    }

    // session.Changed 가 어떤 이유든 발화되면 슬롯 캐시도 같이 재구성 (안전망).
    private void OnSessionChanged()
    {
        RebuildSlotCacheFromSession();
        RaiseSessionUpdated();
    }

    // SessionProperty 변경 전용 이벤트. 슬롯 매핑 변경의 주된 트리거.
    private void OnSessionPropertiesChanged()
    {
        RebuildSlotCacheFromSession();
        RaiseSessionUpdated();
    }

    private void OnPlayerJoined(string playerId)
    {
        // 호스트만 슬롯 갱신. 클라이언트는 SessionPropertiesChanged 이벤트로 캐시 재구성만 함.
        if (IsHost)
        {
            _slotUpdateInFlight = ChainSlotUpdate(_slotUpdateInFlight, () => AssignSlotForPlayerAsync(playerId));
        }
        RaiseSessionUpdated(playerId);
    }

    private void OnPlayerHasLeft(string playerId)
    {
        if (IsHost)
        {
            _slotUpdateInFlight = ChainSlotUpdate(_slotUpdateInFlight, () => RemoveSlotForPlayerAsync(playerId));
        }
        RaiseSessionUpdated(playerId);
    }

    private void RaiseSessionUpdated()
    {
        OnSessionUpdated?.Invoke(_session);
    }

    private void RaiseSessionUpdated(string _)
    {
        OnSessionUpdated?.Invoke(_session);
    }

    private void HandleSessionGone()
    {
        UnbindSessionEvents(_session);
        _session = null;
        _slotCache.Clear();
        OnSessionLeft?.Invoke();
    }

    // ─────────────────────────────────────────────────────────────────
    // Cooldown / singleton / error
    // ─────────────────────────────────────────────────────────────────

    private void StartRestartCooldownWatch()
    {
        if (_restartCooldownRoutine != null) StopCoroutine(_restartCooldownRoutine);
        _restartCooldownRoutine = StartCoroutine(WaitForRestartCooldownThenNotify());
    }

    private IEnumerator WaitForRestartCooldownThenNotify()
    {
        yield return new WaitForSeconds(_settings.GameRestartCooldownSec);
        _restartCooldownRoutine = null;
        OnRestartCooldownEnded?.Invoke();
    }

    private void RaiseError(string message)
    {
        OnError?.Invoke(message);
    }

    // ─────────────────────────────────────────────────────────────────
    // Slot management (SessionProperty["Slots"] JSON 매핑)
    // ─────────────────────────────────────────────────────────────────

    // 호스트 전용: 세션 생성 직후 슬롯 0 = 호스트 자신 등재.
    private async Task InitializeSlotsAsHostAsync()
    {
        if (_session == null || !_session.IsHost) return;
        string hostId = _session.CurrentPlayer?.Id;
        if (string.IsNullOrEmpty(hostId)) return;

        Dictionary<int, string> next = new Dictionary<int, string>();
        // SessionProperty 에 이미 무언가 적혀있을 가능성은 새 세션이라 거의 없지만, 안전하게 병합.
        foreach (KeyValuePair<int, string> kv in _slotCache) next[kv.Key] = kv.Value;
        next[0] = hostId;

        await WriteSlotMapAsync(next);
    }

    // 호스트 전용: 새 플레이어에게 가장 작은 빈 슬롯 배정. 이미 등재된 playerId 면 no-op (멱등).
    private async Task AssignSlotForPlayerAsync(string playerId)
    {
        if (_session == null || !_session.IsHost) return;
        if (string.IsNullOrEmpty(playerId)) return;
        // 이미 캐시에 있으면 멱등 처리.
        if (FindSlotByPlayerId(playerId) >= 0) return;

        int maxPlayers = _settings != null ? _settings.MaxPlayers : 4;
        int slot = FindLowestEmptySlot(maxPlayers);
        if (slot < 0)
        {
            DebugTool.Warning($"빈 슬롯 없음 - playerId={playerId} 배정 실패", DebugType.Network);
            return;
        }

        Dictionary<int, string> next = new Dictionary<int, string>(_slotCache);
        next[slot] = playerId;
        await WriteSlotMapAsync(next);
    }

    // 호스트 전용: 떠난 플레이어의 슬롯 회수.
    private async Task RemoveSlotForPlayerAsync(string playerId)
    {
        if (_session == null || !_session.IsHost) return;
        if (string.IsNullOrEmpty(playerId)) return;

        int slot = FindSlotByPlayerId(playerId);
        if (slot < 0) return;

        Dictionary<int, string> next = new Dictionary<int, string>(_slotCache);
        next.Remove(slot);
        await WriteSlotMapAsync(next);
    }

    // 호스트 측 SessionProperty["Slots"] 갱신. 캐시는 SessionPropertiesChanged 이벤트가 다시 채움(권위는 SessionProperty).
    private async Task WriteSlotMapAsync(Dictionary<int, string> slots)
    {
        if (_session == null) return;
        try
        {
            string json = JsonConvert.SerializeObject(slots);
            IHostSession host = _session.AsHost();
            host.SetProperty(LobbyConstants.KEY_SESSION_SLOTS, new SessionProperty(json, VisibilityPropertyOptions.Member));
            await host.SavePropertiesAsync();
        }
        catch (Exception e)
        {
            DebugTool.Error($"슬롯 맵 쓰기 실패: {e.Message}", DebugType.Network);
            RaiseError("슬롯 정보를 저장하지 못했습니다.");
        }
    }

    private int FindLowestEmptySlot(int maxPlayers)
    {
        for (int i = 0; i < maxPlayers; i++)
        {
            if (!_slotCache.ContainsKey(i)) return i;
        }
        return -1;
    }

    // 모든 클라(호스트 포함) 가 SessionProperty["Slots"] 변경 이벤트에서 호출.
    // SessionProperty 가 권위, _slotCache 는 lookup 편의용 사본.
    private void RebuildSlotCacheFromSession()
    {
        _slotCache.Clear();
        if (_session == null) return;
        if (_session.Properties == null) return;
        if (!_session.Properties.TryGetValue(LobbyConstants.KEY_SESSION_SLOTS, out SessionProperty prop)) return;
        if (prop == null || string.IsNullOrEmpty(prop.Value)) return;

        try
        {
            Dictionary<int, string> parsed = JsonConvert.DeserializeObject<Dictionary<int, string>>(prop.Value);
            if (parsed == null) return;
            foreach (KeyValuePair<int, string> kv in parsed)
            {
                _slotCache[kv.Key] = kv.Value;
            }
        }
        catch (Exception e)
        {
            DebugTool.Warning($"슬롯 맵 역직렬화 실패: {e.Message}", DebugType.Network);
        }
    }

    // 호스트의 슬롯 갱신 작업을 직렬화. 이전 작업이 실패해도 체인은 계속 진행되도록 try/catch 로 감쌈.
    private static async Task ChainSlotUpdate(Task previous, Func<Task> next)
    {
        try { await previous; }
        catch (Exception e) { DebugTool.Warning($"이전 슬롯 작업 실패 무시: {e.Message}", DebugType.Network); }
        await next();
    }

    private void SetSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
