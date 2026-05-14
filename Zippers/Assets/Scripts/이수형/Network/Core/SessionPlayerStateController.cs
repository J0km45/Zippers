using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 게임 세션 동안 살아있는/접속 중인 플레이어 수를 호스트 권위로 추적하는 NetworkBehaviour.
///
/// 두 가지 NetworkVariable 노출:
/// - ConnectedPlayerCount: 세션에 접속해 있는 플레이어 수. Spawn 시 +1, disconnect 시 -1 (단방향, 영구).
/// - SessionAlivePlayerCount: 현재 HP 살아있는 플레이어 수. ConnectedPlayerCount 의 부분집합.
///   HP=0 사망 시 -1, 부활 시 +1 (부활 시점에 connected 인 경우만).
///
/// 사용처:
/// - TeleportSupporter.MinVoteWin: SessionAlivePlayerCount / 2f → 살아있는 인원의 과반 투표 임계값.
/// - PlayerCheckEventSO: 다음 맵으로 텔레포트 후 부활 처리까지 끝나면 ConnectedPlayerCount 만큼이 도착.
///   curCount(맵 안 인원) >= ConnectedPlayerCount 일 때 Battle 진입.
///
/// 배치:
/// - GameScene 안의 빈 GameObject 에 NetworkObject + 이 컴포넌트 부착.
/// - 씬 NetworkObject 라 NGO 가 자동 spawn.
///
/// 사망/부활 hook (B 쪽 전투/부활 시스템에서 호출):
/// - NotifyPlayerHPDeath(clientId): HP=0 시. alive 에서만 제거.
/// - NotifyPlayerRevive(clientId): 부활 시. connected 체크 내장 — disconnect 영구 사망자 부활 거부.
/// 둘 다 현재는 호출자 없음. 1단계에서는 spawn/disconnect 만으로 양 카운트가 같이 움직임.
/// </summary>
public class SessionPlayerStateController : NetworkBehaviour
{
    public static SessionPlayerStateController Instance { get; private set; }

    // ─────────────────────────────────────────────────────────────────
    // Network state (호스트만 쓰기, 모두 읽기)
    // ─────────────────────────────────────────────────────────────────

    private readonly NetworkVariable<int> _connectedPlayerCount =
        new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private readonly NetworkVariable<int> _sessionAlivePlayerCount =
        new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    /// <summary>세션에 접속해 있는 플레이어 수. 모든 클라이언트 read.</summary>
    public NetworkVariable<int> ConnectedPlayerCount => _connectedPlayerCount;

    /// <summary>현재 HP 살아있는 플레이어 수. 모든 클라이언트 read.</summary>
    public NetworkVariable<int> SessionAlivePlayerCount => _sessionAlivePlayerCount;

    // ─────────────────────────────────────────────────────────────────
    // Server-only state (호스트에서만 채워짐)
    // ─────────────────────────────────────────────────────────────────

    private readonly HashSet<ulong> _connectedClientIds = new HashSet<ulong>();
    private readonly HashSet<ulong> _aliveClientIds = new HashSet<ulong>();

    // ─────────────────────────────────────────────────────────────────
    // Singleton
    // ─────────────────────────────────────────────────────────────────

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

    // ─────────────────────────────────────────────────────────────────
    // Notify API (호스트 권위 — 비호스트 호출은 즉시 return)
    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// GameSpawnController.SpawnOne 성공 직후 호스트에서 호출.
    /// connected/alive 양쪽에 추가. 이미 추적 중이면 idempotent (중복 +1 방지).
    /// </summary>
    public void NotifyPlayerSpawned(ulong clientId)
    {
        if (!IsServer) return;

        bool addedConnected = _connectedClientIds.Add(clientId);
        bool addedAlive = _aliveClientIds.Add(clientId);

        if (addedConnected || addedAlive)
        {
            SyncCounts();
            DebugTool.Log(
                $"NotifyPlayerSpawned: clientId={clientId} (Connected={_connectedPlayerCount.Value}, Alive={_sessionAlivePlayerCount.Value})",
                DebugType.Network, this);
        }
        else
        {
            DebugTool.Warning(
                $"NotifyPlayerSpawned: clientId={clientId} 이미 추적 중 - 무시 (재호출?)",
                DebugType.Network, this);
        }
    }

    /// <summary>
    /// 클라이언트 disconnect 시 호스트에서 호출 (GameSpawnController.OnClientDisconnect).
    /// connected/alive 양쪽에서 제거. 영구 — 다시 들어와도 자동 복구 안 됨 (재 join 은 NotifyPlayerSpawned 로).
    /// </summary>
    public void NotifyClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;

        bool removedConnected = _connectedClientIds.Remove(clientId);
        bool removedAlive = _aliveClientIds.Remove(clientId);

        if (removedConnected || removedAlive)
        {
            SyncCounts();
            DebugTool.Log(
                $"NotifyClientDisconnected: clientId={clientId} (Connected={_connectedPlayerCount.Value}, Alive={_sessionAlivePlayerCount.Value})",
                DebugType.Network, this);
        }
        else
        {
            DebugTool.Warning(
                $"NotifyClientDisconnected: clientId={clientId} 미추적 - 무시 (스폰 전 또는 이미 정리됨)",
                DebugType.Network, this);
        }
    }

    /// <summary>
    /// HP=0 사망 시 호스트에서 호출 (B 쪽 전투 시스템 hook).
    /// alive 에서만 제거 — connected 는 유지 (부활 가능 상태).
    /// </summary>
    public void NotifyPlayerHPDeath(ulong clientId)
    {
        if (!IsServer) return;

        if (!_connectedClientIds.Contains(clientId))
        {
            DebugTool.Warning(
                $"NotifyPlayerHPDeath: clientId={clientId} 가 connected 가 아님 - 무시 (이미 disconnect)",
                DebugType.Network, this);
            return;
        }

        if (_aliveClientIds.Remove(clientId))
        {
            SyncCounts();
            DebugTool.Log(
                $"NotifyPlayerHPDeath: clientId={clientId} (Alive={_sessionAlivePlayerCount.Value}/{_connectedPlayerCount.Value})",
                DebugType.Network, this);
        }
        else
        {
            DebugTool.Warning(
                $"NotifyPlayerHPDeath: clientId={clientId} 가 이미 사망 상태 - 무시",
                DebugType.Network, this);
        }
    }

    /// <summary>
    /// 부활 시 호스트에서 호출 (B 쪽 부활 시스템 hook, 텔레포트 후 처리 가정).
    /// connected 일 때만 alive 에 추가 — disconnect 된 영구 사망자는 부활 거부.
    /// </summary>
    public void NotifyPlayerRevive(ulong clientId)
    {
        if (!IsServer) return;

        if (!_connectedClientIds.Contains(clientId))
        {
            DebugTool.Warning(
                $"NotifyPlayerRevive: clientId={clientId} 가 connected 가 아님 - 부활 거부 (disconnect 된 영구 사망자)",
                DebugType.Network, this);
            return;
        }

        if (_aliveClientIds.Add(clientId))
        {
            SyncCounts();
            DebugTool.Log(
                $"NotifyPlayerRevive: clientId={clientId} (Alive={_sessionAlivePlayerCount.Value}/{_connectedPlayerCount.Value})",
                DebugType.Network, this);
        }
        else
        {
            DebugTool.Warning(
                $"NotifyPlayerRevive: clientId={clientId} 가 이미 살아있음 - 무시",
                DebugType.Network, this);
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // 호스트 권위 read helper (서버 측 즉시 조회용)
    // ─────────────────────────────────────────────────────────────────

    /// <summary>호스트 측에서 clientId 가 현재 connected 로 추적 중인지 조회. 부활 가능 여부 판단 등.</summary>
    public bool IsConnectedServer(ulong clientId)
    {
        return IsServer && _connectedClientIds.Contains(clientId);
    }

    /// <summary>호스트 측에서 clientId 가 현재 HP-alive 로 추적 중인지 조회.</summary>
    public bool IsAliveServer(ulong clientId)
    {
        return IsServer && _aliveClientIds.Contains(clientId);
    }

    // ─────────────────────────────────────────────────────────────────
    // Internal sync
    // ─────────────────────────────────────────────────────────────────

    private void SyncCounts()
    {
        // NetworkVariable 쓰기는 spawn 된 상태에서만 안전. despawn 직전/후 호출 방어.
        try
        {
            _connectedPlayerCount.Value = _connectedClientIds.Count;
            _sessionAlivePlayerCount.Value = _aliveClientIds.Count;
        }
        catch
        {
            // 무시 — 다음 호출에서 다시 시도됨.
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // Diagnostics (DebugTool 연동 — 호스트 측 컨텍스트 덤프)
    // ─────────────────────────────────────────────────────────────────

    [ContextMenu("Dump Session Player State")]
    private void DumpSessionPlayerState()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("[SessionPlayerStateController] ===");
        sb.AppendLine($"  ConnectedPlayerCount = {_connectedPlayerCount.Value} (set size {_connectedClientIds.Count})");
        sb.Append("    ids = [");
        bool first = true;
        foreach (ulong cid in _connectedClientIds)
        {
            if (!first) sb.Append(", ");
            sb.Append(cid);
            first = false;
        }
        sb.AppendLine("]");

        sb.AppendLine($"  SessionAlivePlayerCount = {_sessionAlivePlayerCount.Value} (set size {_aliveClientIds.Count})");
        sb.Append("    ids = [");
        first = true;
        foreach (ulong cid in _aliveClientIds)
        {
            if (!first) sb.Append(", ");
            sb.Append(cid);
            first = false;
        }
        sb.AppendLine("]");

        DebugTool.Log(sb.ToString(), DebugType.Network, this);
    }
}
