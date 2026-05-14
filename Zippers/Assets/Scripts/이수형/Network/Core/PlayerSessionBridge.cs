using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;

/// <summary>
/// NGO clientId(ulong) ↔ UGS playerId(string) 매핑을 호스트가 관리하는 싱글톤.
///
/// 왜 필요한가:
/// - NGO API(SpawnAsPlayerObject, ServerRpc 등)는 clientId 만 받음.
/// - 로비/세션 정보(PlayerClass, SlotIndex 등)는 playerId(UGS PlayerProperty) 기반.
/// - 두 ID 체계를 잇는 다리가 없으면 "이 clientId 가 어느 클래스/슬롯인가" 조회가 불가.
///
/// 동작 흐름:
/// - 클라: LobbyManager 가 CreateSession/JoinSession 직전에
///   NetworkConfig.ConnectionData = playerId 바이트로 세팅.
/// - 호스트: ConnectionApprovalCallback 에서 payload 디코드 → 매핑 등록.
/// - 호스트: OnClientDisconnectCallback 에서 매핑 정리.
///
/// 호스트 자신도 clientId=0 인 클라이언트로 동일 경로를 거침. 특수 처리 없음.
/// DontDestroyOnLoad 라 Lobby ↔ Game 씬 전환에 영향 없음.
///
/// 옵션 A 결정과 결합: response.CreatePlayerObject = false 로 응답해 NGO 자동 스폰을 차단함.
/// 실제 PlayerObject 스폰은 GameSpawnController(Step 3)가 명시적으로 수행.
/// </summary>
public class PlayerSessionBridge : MonoBehaviour
{
    public static PlayerSessionBridge Instance { get; private set; }

    // 서버 권위 상태. 클라 측엔 비어있어도 무방.
    private readonly Dictionary<ulong, string> _clientToPlayer = new Dictionary<ulong, string>();
    private readonly Dictionary<string, ulong> _playerToClient = new Dictionary<string, ulong>();

    private bool _subscribed;

    // ─────────────────────────────────────────────────────────────────
    // Lifecycle
    // ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        SetSingleton();
    }

    private void OnDestroy()
    {
        Unsubscribe();
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        // NetworkManager.Singleton 이 Awake 시점에 null 일 수 있음 (script execution order 비결정성).
        // Start 시점엔 거의 보장되지만, 안전망으로 코루틴 폴링도 같이 둠.
        StartCoroutine(EnsureSubscribed());
    }

    private IEnumerator EnsureSubscribed()
    {
        while (NetworkManager.Singleton == null)
        {
            yield return null;
        }
        Subscribe();
    }

    private void Subscribe()
    {
        if (_subscribed) return;

        NetworkManager nm = NetworkManager.Singleton;
        if (nm == null) return;

        nm.ConnectionApprovalCallback += OnApprovalRequested;
        nm.OnClientDisconnectCallback += OnClientDisconnect;
        _subscribed = true;

        DebugTool.Log("ConnectionApproval / Disconnect 콜백 구독 완료", DebugType.Network, this);
    }

    private void Unsubscribe()
    {
        if (!_subscribed) return;

        NetworkManager nm = NetworkManager.Singleton;
        if (nm != null)
        {
            nm.ConnectionApprovalCallback -= OnApprovalRequested;
            nm.OnClientDisconnectCallback -= OnClientDisconnect;
        }
        _subscribed = false;
    }

    // ─────────────────────────────────────────────────────────────────
    // NGO callbacks (host-side authoritative)
    // ─────────────────────────────────────────────────────────────────

    private void OnApprovalRequested(
        NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response)
    {
        string playerId = DecodePayload(request.Payload);

        if (string.IsNullOrEmpty(playerId))
        {
            DebugTool.Warning(
                $"clientId={request.ClientNetworkId} payload 비어있음 - 매핑 없이 승인 (게임 진행 시 PlayerInfo 조회 실패 가능)",
                DebugType.Network, this);
        }
        else
        {
            RegisterMapping(request.ClientNetworkId, playerId);
        }

        // 옵션 A: NGO 자동 스폰 비활성. 실제 PlayerObject 는 GameSpawnController 가 SpawnAsPlayerObject 로 생성.
        response.Approved = true;
        response.CreatePlayerObject = false;
        response.Pending = false;
        // PlayerPrefabHash / Position / Rotation 은 default (어차피 CreatePlayerObject=false 라 무시됨)
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (_clientToPlayer.TryGetValue(clientId, out string playerId))
        {
            _clientToPlayer.Remove(clientId);
            _playerToClient.Remove(playerId);
            DebugTool.Log($"매핑 해제: clientId={clientId}, playerId={playerId}", DebugType.Network, this);
        }
        else
        {
            DebugTool.Log($"매핑 해제 스킵: clientId={clientId} 미등록", DebugType.Network, this);
        }
    }

    private static string DecodePayload(byte[] payload)
    {
        if (payload == null || payload.Length == 0) return null;
        try
        {
            return Encoding.UTF8.GetString(payload);
        }
        catch (Exception e)
        {
            DebugTool.Warning($"payload 디코드 실패: {e.Message}", DebugType.Network);
            return null;
        }
    }

    private void RegisterMapping(ulong clientId, string playerId)
    {
        // 동일 playerId 가 다른 clientId 로 이미 등록되어 있으면 (재진입/재연결) 이전 매핑 제거.
        if (_playerToClient.TryGetValue(playerId, out ulong existingClientId) && existingClientId != clientId)
        {
            _clientToPlayer.Remove(existingClientId);
            DebugTool.Warning(
                $"동일 playerId 재등록 감지 - 이전 clientId={existingClientId} 제거 후 새 clientId={clientId} 등록 (playerId={playerId})",
                DebugType.Network, this);
        }

        _clientToPlayer[clientId] = playerId;
        _playerToClient[playerId] = clientId;

        DebugTool.Log($"매핑 등록: clientId={clientId}, playerId={playerId}", DebugType.Network, this);
    }

    // ─────────────────────────────────────────────────────────────────
    // Public lookups
    // ─────────────────────────────────────────────────────────────────

    /// <summary>clientId → UGS playerId. 미등록이면 false.</summary>
    public bool TryGetPlayerId(ulong clientId, out string playerId)
    {
        return _clientToPlayer.TryGetValue(clientId, out playerId);
    }

    /// <summary>UGS playerId → clientId. 미등록이면 false.</summary>
    public bool TryGetClientId(string playerId, out ulong clientId)
    {
        if (string.IsNullOrEmpty(playerId))
        {
            clientId = default;
            return false;
        }
        return _playerToClient.TryGetValue(playerId, out clientId);
    }

    /// <summary>
    /// clientId 한 번에 PlayerInfo (Class / SlotIndex) 까지 조회.
    /// 내부: clientId → playerId → LobbyManager.GetPlayerInfo.
    /// 호출자(GameSpawnController 등)가 매핑 디테일 몰라도 되게 편의 메서드 제공.
    /// </summary>
    public bool TryGetPlayerInfo(ulong clientId, out PlayerInfo info)
    {
        info = PlayerInfo.Unassigned;

        if (!_clientToPlayer.TryGetValue(clientId, out string playerId))
        {
            return false;
        }

        if (LobbyManager.Instance == null)
        {
            DebugTool.Warning("LobbyManager.Instance 가 null - PlayerInfo 조회 불가", DebugType.Network, this);
            return false;
        }

        ISession session = LobbyManager.Instance.CurrentSession;
        if (session == null)
        {
            DebugTool.Warning("CurrentSession 이 null - PlayerInfo 조회 불가", DebugType.Network, this);
            return false;
        }

        for (int i = 0; i < session.Players.Count; i++)
        {
            IReadOnlyPlayer p = session.Players[i];
            if (p.Id == playerId)
            {
                info = LobbyManager.Instance.GetPlayerInfo(p);
                return true;
            }
        }

        DebugTool.Warning(
            $"clientId={clientId} 의 playerId={playerId} 가 현재 세션 Players 에 없음",
            DebugType.Network, this);
        return false;
    }

    /// <summary>현재 등록된 매핑 수. 디버깅용.</summary>
    public int MappingCount => _clientToPlayer.Count;

    /// <summary>
    /// 특정 clientId 의 매핑 상태를 사람이 읽을 수 있는 문자열로 반환.
    /// - 해당 clientId 의 정방향 (clientId → playerId) 매핑 존재 여부
    /// - 그 playerId 의 역방향 (playerId → clientId) 일치 여부
    /// - 전체 매핑 개수 / 전 항목 dump
    /// GameSpawnController 등이 SlotIndex=-1 같은 비정상 시점에 한 번에 컨텍스트를 찍을 때 사용.
    /// 부작용 없음 (read-only).
    /// </summary>
    public string DumpClientIdMapping(ulong clientId)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"[PlayerSessionBridge] DumpClientIdMapping(clientId={clientId}) count={_clientToPlayer.Count}");

        if (_clientToPlayer.TryGetValue(clientId, out string playerId))
        {
            bool reverseMatch = _playerToClient.TryGetValue(playerId, out ulong reverseClientId) && reverseClientId == clientId;
            sb.AppendLine($"  forward  : clientId={clientId} → playerId={playerId}");
            sb.AppendLine($"  reverse  : playerId={playerId} → clientId={(reverseMatch ? reverseClientId.ToString() : "(missing or mismatch)")}");
            if (!reverseMatch)
            {
                sb.AppendLine($"  ⚠ 역방향 매핑 불일치 - _playerToClient 동기화 깨짐 의심");
            }
        }
        else
        {
            sb.AppendLine($"  ⚠ 정방향 매핑 없음 - ConnectionApproval payload 누락 또는 디코드 실패 가능");
        }

        sb.AppendLine($"  --- 전체 매핑 ---");
        foreach (KeyValuePair<ulong, string> kv in _clientToPlayer)
        {
            sb.AppendLine($"    clientId={kv.Key} ↔ playerId={kv.Value}");
        }
        return sb.ToString();
    }

    // ─────────────────────────────────────────────────────────────────
    // Debug helpers
    // ─────────────────────────────────────────────────────────────────

    [ContextMenu("Dump Mapping")]
    private void DumpMapping()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"PlayerSessionBridge: 현재 매핑 {_clientToPlayer.Count}건");
        foreach (KeyValuePair<ulong, string> kv in _clientToPlayer)
        {
            sb.AppendLine($"  clientId={kv.Key} ↔ playerId={kv.Value}");
        }
        DebugTool.Log(sb.ToString(), DebugType.Network, this);
    }

    [ContextMenu("Clear All Mapping (Debug Only)")]
    private void ClearAllMapping()
    {
        _clientToPlayer.Clear();
        _playerToClient.Clear();
        DebugTool.Warning("모든 매핑 강제 삭제 (디버그용)", DebugType.Network, this);
    }

    // ─────────────────────────────────────────────────────────────────
    // Singleton
    // ─────────────────────────────────────────────────────────────────

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
