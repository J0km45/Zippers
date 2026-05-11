using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;

/// <summary>
/// LobbyScene 안의 호스트 권위 NetworkBehaviour.
/// LobbyScene 의 SceneObject 로 배치되어 NGO 가 자동 spawn — 호스트만 권위 로직 실행.
///
/// 두 가지 책임:
/// 1) 클래스 중복 검증 (ServerRpc → ClientRpc 라운드트립, race-safe 한 reservation 포함)
/// 2) 클래스에 맞는 캐릭터 모델 NetworkObject 의 spawn / 변경 / 취소 시 despawn
///
/// 클라이언트는 LobbyHostAuthority.Instance.RequestClassChangeAsync() 만 호출하면 됨.
/// LobbyManager.SetClassAsync 가 내부적으로 호출함. UI 는 직접 만지지 않음.
/// </summary>
public class LobbyHostAuthority : NetworkBehaviour
{
    public static LobbyHostAuthority Instance { get; private set; }

    // ── Inspector: 클래스별 prefab + 슬롯 위치 ─────────────────────
    [Header("Class Prefabs (index = (int)PlayerClass)")]
    [Tooltip("배열 길이 = 4. [0]=Melee, [1]=Rifle, [2]=Shotgun, [3]=Pistol 순. None(-1)은 별도 자리 없음 — 음수 인덱스라 spawn 단계에서 자연스럽게 차단됨.")]
    [SerializeField] private NetworkObject[] _classPrefabs;

    [Header("Spawn Points (index = SlotIndex)")]
    [Tooltip("배열 길이 = MaxPlayers (보통 4). 슬롯 인덱스에 해당하는 Transform 을 순서대로 드래그.")]
    [SerializeField] private Transform[] _spawnPoints;

    // ── Server-side state ────────────────────────────────────────
    // playerId(UGS) → spawn 된 모델 추적
    private readonly Dictionary<string, AvatarRecord> _spawnedAvatars = new Dictionary<string, AvatarRecord>();
    // 승인했지만 PlayerProperty 갱신이 아직 안 들어온 reservation. race 방지용.
    private readonly Dictionary<string, PlayerClass> _pendingReservations = new Dictionary<string, PlayerClass>();

    // ── Client-side state ────────────────────────────────────────
    // 본인의 진행 중 클래스 변경 요청 (한 번에 하나만 허용).
    private TaskCompletionSource<bool> _pendingLocalRequest;

    private struct AvatarRecord
    {
        public NetworkObject NetworkObject;
        public PlayerClass Class;
        public int Slot;
    }

    // ── Singleton ────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void OnDestroy()
    {
        if (Instance == this) Instance = null;
        base.OnDestroy();
    }

    // ── NetworkBehaviour lifecycle ───────────────────────────────
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            if (LobbyManager.Instance == null)
            {
                DebugTool.Error("LobbyManager.Instance 가 null - LobbyHostAuthority 가 자기 역할 못함", DebugType.Network, this);
                return;
            }
            LobbyManager.Instance.OnSessionUpdated += HostHandleSessionUpdate;
            // 초기 상태 1회 처리 (이미 있을 수 있는 PlayerProperty 반영)
            HostHandleSessionUpdate(LobbyManager.Instance.CurrentSession);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            if (LobbyManager.Instance != null)
            {
                LobbyManager.Instance.OnSessionUpdated -= HostHandleSessionUpdate;
            }
            // NGO 씬 전환 시 NetworkObject 들이 자동 despawn 되긴 하지만 명시적으로 정리.
            foreach (KeyValuePair<string, AvatarRecord> kv in _spawnedAvatars)
            {
                if (kv.Value.NetworkObject != null && kv.Value.NetworkObject.IsSpawned)
                {
                    kv.Value.NetworkObject.Despawn();
                }
            }
            _spawnedAvatars.Clear();
            _pendingReservations.Clear();
        }
        // 클라 측: 진행 중 요청이 있다면 실패 처리해서 await 가 영원히 안 멈춤
        if (_pendingLocalRequest != null)
        {
            TaskCompletionSource<bool> tcs = _pendingLocalRequest;
            _pendingLocalRequest = null;
            tcs.TrySetResult(false);
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // Public API (LobbyManager 가 호출)
    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// 클라이언트에서 호스트에게 클래스 변경 요청 → 응답 await.
    /// 동시에 한 요청만 허용 (이전 요청 진행 중이면 false 즉시 반환).
    /// PlayerClass.None 호출은 LobbyManager 단계에서 분기되므로 여기엔 들어오지 않음.
    /// </summary>
    public Task<bool> RequestClassChangeAsync(PlayerClass requested)
    {
        if (requested == PlayerClass.None)
        {
            // 안전망: 호출자가 None 을 보냈다면 검증 없이 바로 통과
            return Task.FromResult(true);
        }
        if (!IsSpawned)
        {
            DebugTool.Warning("LobbyHostAuthority 아직 spawn 안 됨", DebugType.Network, this);
            return Task.FromResult(false);
        }
        if (_pendingLocalRequest != null)
        {
            DebugTool.Warning("이전 클래스 요청 진행 중 - 새 요청 거부", DebugType.Network, this);
            return Task.FromResult(false);
        }

        string ownId = LobbyManager.Instance?.CurrentSession?.CurrentPlayer?.Id;
        if (string.IsNullOrEmpty(ownId))
        {
            DebugTool.Error("CurrentPlayer.Id 가 비어있음 - 세션 미진입", DebugType.Network, this);
            return Task.FromResult(false);
        }

        _pendingLocalRequest = new TaskCompletionSource<bool>();
        RequestClassChangeServerRpc((int)requested, new FixedString64Bytes(ownId));
        return _pendingLocalRequest.Task;
    }

    // ─────────────────────────────────────────────────────────────────
    // ServerRpc: 호스트 검증
    // ─────────────────────────────────────────────────────────────────

    [ServerRpc(RequireOwnership = false)]
    private void RequestClassChangeServerRpc(int classInt, FixedString64Bytes requesterPlayerId, ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;
        string requesterId = requesterPlayerId.ToString();
        PlayerClass requested = (PlayerClass)classInt;

        bool approved = ValidateAndReserve(requesterId, requested);

        ResponseClassChangeClientRpc(approved, classInt, new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { senderClientId } }
        });
    }

    // 호스트 측 검증: PlayerProperty + 진행 중 reservation 둘 다 고려해 중복 차단.
    private bool ValidateAndReserve(string requesterUgsId, PlayerClass requested)
    {
        ISession session = LobbyManager.Instance?.CurrentSession;
        if (session == null) return false;

        // 다른 플레이어의 현재 PlayerProperty 검사
        for (int i = 0; i < session.Players.Count; i++)
        {
            IReadOnlyPlayer p = session.Players[i];
            if (p.Id == requesterUgsId) continue;
            PlayerClass theirClass = LobbyManager.Instance.GetPlayerInfo(p).PlayerClass;
            if (theirClass == requested) return false;
        }

        // 다른 플레이어의 진행 중 reservation 검사 (race 방지)
        foreach (KeyValuePair<string, PlayerClass> kv in _pendingReservations)
        {
            if (kv.Key == requesterUgsId) continue;
            if (kv.Value == requested) return false;
        }

        // 본인 reservation 갱신
        _pendingReservations[requesterUgsId] = requested;
        return true;
    }

    // ─────────────────────────────────────────────────────────────────
    // ClientRpc: 응답 (요청자에게만 targeted)
    // ─────────────────────────────────────────────────────────────────

    [ClientRpc]
    private void ResponseClassChangeClientRpc(bool approved, int classInt, ClientRpcParams rpcParams = default)
    {
        if (_pendingLocalRequest == null) return;
        TaskCompletionSource<bool> tcs = _pendingLocalRequest;
        _pendingLocalRequest = null;
        tcs.TrySetResult(approved);
    }

    // ─────────────────────────────────────────────────────────────────
    // Server-side: PlayerProperty 변경에 따라 모델 spawn/despawn 재조정
    // ─────────────────────────────────────────────────────────────────

    private void HostHandleSessionUpdate(ISession session)
    {
        if (!IsServer || session == null) return;

        // 현재 세션에 있는 playerId 모음 (퇴장자 정리용)
        HashSet<string> activeIds = new HashSet<string>();

        for (int i = 0; i < session.Players.Count; i++)
        {
            IReadOnlyPlayer player = session.Players[i];
            string playerId = player.Id;
            activeIds.Add(playerId);

            PlayerInfo info = LobbyManager.Instance.GetPlayerInfo(player);

            // 본인 PlayerProperty 가 reservation 과 일치하면 reservation 해제
            if (_pendingReservations.TryGetValue(playerId, out PlayerClass reserved) && reserved == info.PlayerClass)
            {
                _pendingReservations.Remove(playerId);
            }

            // 미선택 또는 슬롯 미배정 → 기존 모델 있으면 despawn
            if (info.PlayerClass == PlayerClass.None || info.SlotIndex < 0)
            {
                DespawnAvatar(playerId);
                continue;
            }

            // 이미 같은 클래스/슬롯이면 스킵
            if (_spawnedAvatars.TryGetValue(playerId, out AvatarRecord existing))
            {
                if (existing.Class == info.PlayerClass && existing.Slot == info.SlotIndex)
                {
                    continue;
                }
                // 클래스 또는 슬롯이 바뀜 → 기존 despawn 후 재spawn
                DespawnAvatar(playerId);
            }

            SpawnAvatar(playerId, info.PlayerClass, info.SlotIndex);
        }

        // 세션에서 빠진 플레이어의 모델 정리
        List<string> toRemove = null;
        foreach (string spawnedId in _spawnedAvatars.Keys)
        {
            if (activeIds.Contains(spawnedId)) continue;
            if (toRemove == null) toRemove = new List<string>();
            toRemove.Add(spawnedId);
        }
        if (toRemove != null)
        {
            for (int i = 0; i < toRemove.Count; i++) DespawnAvatar(toRemove[i]);
        }

        // 떠난 플레이어의 reservation 도 정리
        List<string> toRemoveReservations = null;
        foreach (string reservedId in _pendingReservations.Keys)
        {
            if (activeIds.Contains(reservedId)) continue;
            if (toRemoveReservations == null) toRemoveReservations = new List<string>();
            toRemoveReservations.Add(reservedId);
        }
        if (toRemoveReservations != null)
        {
            for (int i = 0; i < toRemoveReservations.Count; i++) _pendingReservations.Remove(toRemoveReservations[i]);
        }
    }

    private void SpawnAvatar(string playerId, PlayerClass cls, int slot)
    {
        int prefabIndex = (int)cls;
        if (_classPrefabs == null || prefabIndex < 0 || prefabIndex >= _classPrefabs.Length)
        {
            DebugTool.Warning($"클래스 prefab 인덱스 {prefabIndex} 범위 초과 (배열 길이 {_classPrefabs?.Length ?? 0})", DebugType.Network, this);
            return;
        }
        NetworkObject prefab = _classPrefabs[prefabIndex];
        if (prefab == null)
        {
            DebugTool.Warning($"클래스 {cls} 의 prefab 미할당 ([{prefabIndex}])", DebugType.Network, this);
            return;
        }

        if (_spawnPoints == null || slot < 0 || slot >= _spawnPoints.Length)
        {
            DebugTool.Warning($"슬롯 {slot} 의 spawn point 범위 초과 (배열 길이 {_spawnPoints?.Length ?? 0})", DebugType.Network, this);
            return;
        }
        Transform spawnPoint = _spawnPoints[slot];
        if (spawnPoint == null)
        {
            DebugTool.Warning($"슬롯 {slot} 의 spawn point 미할당", DebugType.Network, this);
            return;
        }

        try
        {
            NetworkObject instance = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            instance.Spawn();   // server-owned. 조작 없는 디스플레이 모델이라 ownership 불필요
            _spawnedAvatars[playerId] = new AvatarRecord
            {
                NetworkObject = instance,
                Class = cls,
                Slot = slot
            };
            DebugTool.Log($"avatar spawn: player={playerId}, class={cls}, slot={slot}", DebugType.Network, this);
        }
        catch (Exception e)
        {
            DebugTool.Error($"avatar spawn 실패: {e.Message}", DebugType.Network, this);
        }
    }

    private void DespawnAvatar(string playerId)
    {
        if (!_spawnedAvatars.TryGetValue(playerId, out AvatarRecord record)) return;
        if (record.NetworkObject != null && record.NetworkObject.IsSpawned)
        {
            try
            {
                record.NetworkObject.Despawn();
                DebugTool.Log($"avatar despawn: player={playerId}, class={record.Class}", DebugType.Network, this);
            }
            catch (Exception e)
            {
                DebugTool.Warning($"avatar despawn 예외: {e.Message}", DebugType.Network, this);
            }
        }
        _spawnedAvatars.Remove(playerId);
    }
}
