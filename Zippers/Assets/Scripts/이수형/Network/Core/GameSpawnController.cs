using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameScene 진입 시 모든 연결 클라이언트를 그들의 PlayerClass / SlotIndex 에 맞게
/// Start Map 의 스폰 좌표에 SpawnAsPlayerObject() 로 생성하는 호스트 권위 컴포넌트.
///
/// 배치:
/// - GameScene.unity 안의 빈 GameObject 에 NetworkObject + 이 컴포넌트 부착.
/// - 씬 NetworkObject 라 NGO 가 자동으로 spawn → OnNetworkSpawn 에서 콜백 구독.
///
/// 흐름 (Node System Multiplayer Migration PDF Phase 1 그대로):
/// 1) 호스트가 SceneLoader.LoadNetworked(SceneId.Game) 호출 → NGO 가 모든 멤버 씬 로드 동기화
/// 2) 모든 클라이언트 로드 완료 → NetworkManager.SceneManager.OnLoadEventCompleted 호스트에 발화
/// 3) NodeManager.GetStartSpawnPoints() 로 좌표 배열 획득
/// 4) ConnectedClientsIds 순회 → PlayerSessionBridge 로 PlayerInfo 조회
/// 5) class prefab 을 Instantiate 후 SpawnAsPlayerObject(clientId) 로 owner 지정
///
/// 옵션 A 결정 (NetworkManager.PlayerPrefab 비움) 과 결합:
/// NGO 자동 스폰이 없으므로 이 컨트롤러가 호출하는 SpawnAsPlayerObject 가 각 클라이언트의 첫 PlayerObject.
///
/// 실패 케이스 정책:
/// - 매핑 미존재 / SlotIndex 범위 외 / PlayerClass.None / prefab null → 에러 로그 + 그 클라만 skip.
/// - Phase 1 디버깅 단계라 자동 fallback (예: Melee/slot 0) 안 함. 문제가 묻히는 게 더 위험.
/// </summary>
public class GameSpawnController : NetworkBehaviour
{
    public static GameSpawnController Instance { get; private set; }

    [Header("Class Prefabs (index = (int)PlayerClass)")]
    [Tooltip("배열 길이 = 4. [0]=Melee, [1]=Rifle, [2]=Shotgun, [3]=Pistol(현재 Util.prefab).\n" +
             "LobbyHostAuthority 의 _classPrefabs 와 동일 순서 유지 필수.")]
    [SerializeField] private NetworkObject[] _classPrefabs;

    // 스폰된 PlayerObject 추적. 디버깅/사후 조회용.
    private readonly Dictionary<ulong, NetworkObject> _spawnedByClient = new Dictionary<ulong, NetworkObject>();

    // OnLoadEventCompleted 가 동일 씬에 대해 두 번 발화하는 비정상 케이스 방어.
    // Phase 1 의 초기 스폰은 1회만 수행.
    private bool _spawned;

    // 구독 상태 플래그. OnNetworkDespawn 에서 정확히 한 번만 해제하기 위함.
    private bool _subscribed;

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
    // NetworkBehaviour lifecycle
    // ─────────────────────────────────────────────────────────────────

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        NetworkManager nm = NetworkManager.Singleton;
        if (nm == null || nm.SceneManager == null)
        {
            DebugTool.Error("NetworkManager/SceneManager 없음 - OnLoadEventCompleted 구독 실패", DebugType.Network, this);
            return;
        }

        nm.SceneManager.OnLoadEventCompleted += OnAllClientsSceneLoaded;
        // Phase D: disconnect → 사망 처리 정리. NGO 가 PlayerObject 자동 destroy →
        // MapObjectCounter.OnTriggerExit 가 AlivePlayerCount 자동 감소.
        // 여기서는 _spawnedByClient 추적 dictionary 만 정리.
        nm.OnClientDisconnectCallback += OnClientDisconnect;
        _subscribed = true;
        DebugTool.Log("OnLoadEventCompleted / OnClientDisconnectCallback 구독 완료 (호스트)", DebugType.Network, this);
    }

    public override void OnNetworkDespawn()
    {
        if (!_subscribed) return;

        NetworkManager nm = NetworkManager.Singleton;
        if (nm != null)
        {
            if (nm.SceneManager != null)
            {
                nm.SceneManager.OnLoadEventCompleted -= OnAllClientsSceneLoaded;
            }
            nm.OnClientDisconnectCallback -= OnClientDisconnect;
        }
        _subscribed = false;
    }

    /// <summary>
    /// 클라이언트 disconnect 시 호스트 콜백.
    /// PlayerObject 자체는 NGO 가 자동 despawn/destroy (NetworkObject.DontDestroyWithOwner == false 기본).
    /// destroy 가 발생하면 Unity 가 OnTriggerExit 를 발화 → MapObjectCounter 가 AlivePlayerCount 감소.
    /// 여기서는 추적용 dictionary 정리 + SessionPlayerStateController 에 disconnect 통보.
    /// </summary>
    private void OnClientDisconnect(ulong clientId)
    {
        if (_spawnedByClient.TryGetValue(clientId, out NetworkObject obj))
        {
            string objName = obj != null ? obj.name : "(이미 destroy 됨)";
            _spawnedByClient.Remove(clientId);
            DebugTool.Log($"disconnect 정리: clientId={clientId}, obj={objName}", DebugType.Network, this);
        }
        else
        {
            DebugTool.Log($"disconnect 무시: clientId={clientId} 미추적 (스폰 전 또는 이미 정리됨)", DebugType.Network, this);
        }

        // 세션 카운트 갱신. SessionPlayerStateController 는 미추적 clientId 도 자체적으로 무시 처리.
        if (SessionPlayerStateController.Instance != null)
        {
            SessionPlayerStateController.Instance.NotifyClientDisconnected(clientId);
        }
        else
        {
            DebugTool.Warning(
                $"SessionPlayerStateController.Instance 가 null - disconnect 통보 누락 (clientId={clientId}). GameScene 에 컴포넌트 배치 확인.",
                DebugType.Network, this);
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // Scene load callback (server only)
    // ─────────────────────────────────────────────────────────────────

    private void OnAllClientsSceneLoaded(
        string sceneName,
        LoadSceneMode mode,
        List<ulong> clientsCompleted,
        List<ulong> clientsTimedOut)
    {
        // GameScene 이외 무시 (안전망 - 우리가 GameScene 안에서만 살지만 SceneManager 는 전역 이벤트라)
        if (sceneName != SceneId.Game.GetName()) return;

        if (_spawned)
        {
            DebugTool.Warning($"OnLoadEventCompleted 재발화 무시 (sceneName={sceneName})", DebugType.Network, this);
            return;
        }

        if (clientsTimedOut != null && clientsTimedOut.Count > 0)
        {
            // 타임아웃 클라가 있어도 일단 스폰은 진행 (그 클라는 어차피 매핑 조회 실패로 skip 됨).
            // Phase 1 은 게임 종료/복귀 로직 없으므로 여기서 강제 종료까진 안 함.
            DebugTool.Warning($"씬 로드 타임아웃 {clientsTimedOut.Count}명 - 그대로 진행", DebugType.Network, this);
        }

        // Phase B: NodeManager 1회 조회 - 노드 초기화와 플레이어 스폰 양쪽에서 공유.
        NodeManager nodeManager = FindFirstObjectByType<NodeManager>();
        if (nodeManager == null)
        {
            DebugTool.Error("NodeManager 를 찾을 수 없음 - 노드 초기화/스폰 중단 (GameScene 에 배치되어야 함)", DebugType.Network, this);
            return;
        }

        // 1단계: 노드 시스템 초기화 (호스트가 LobbyManager 의 난이도 읽음 → NodeManager 가 시드 결정 → ClientRpc 로 전파).
        InitializeNodeSystem(nodeManager);

        // 2단계: 플레이어 PlayerObject 스폰 (Start 맵 좌표 기준).
        SpawnAllConnectedClients(nodeManager);
        _spawned = true;
    }

    /// <summary>
    /// LobbyManager.GetCurrentDifficulty() 로 난이도 읽고 NodeManager.InitializeAsHost 호출.
    /// LobbyManager 미존재(Test 빌드 / 단독 호스트 부트스트랩 등) 시 Level3=Normal 로 폴백.
    /// </summary>
    private void InitializeNodeSystem(NodeManager nodeManager)
    {
        NodeDifficulty difficulty;
        if (LobbyManager.Instance != null)
        {
            difficulty = LobbyManager.Instance.GetCurrentDifficulty();
        }
        else
        {
            difficulty = NodeDifficulty.Level3;
            DebugTool.Warning("LobbyManager.Instance 가 null - 노드 시스템 기본 난이도(Level3=Normal) 로 폴백", DebugType.Network, this);
        }

        DebugTool.Log($"노드 시스템 초기화 요청: difficulty={difficulty}", DebugType.Network, this);
        nodeManager.InitializeAsHost(difficulty);
    }

    // ─────────────────────────────────────────────────────────────────
    // Spawning
    // ─────────────────────────────────────────────────────────────────

    private void SpawnAllConnectedClients(NodeManager nodeManager)
    {
        // NodeManager 는 OnAllClientsSceneLoaded / ForceRespawnAll 에서 null 가드 후 전달됨.
        // 호출 시점엔 StartTypeMapController.Data.PlayerSpawnPoint_Down 이 채워져 있어야 함.
        Transform[] spawnPoints = nodeManager.GetStartSpawnPoints();
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            DebugTool.Error("GetStartSpawnPoints() 결과가 비어있음 - 스폰 중단 (StartTypeMapController 의 PlayerSpawnPoint_Down 확인)", DebugType.Network, this);
            return;
        }

        NetworkManager nm = NetworkManager.Singleton;
        if (nm == null)
        {
            DebugTool.Error("NetworkManager.Singleton 없음 - 스폰 중단", DebugType.Network, this);
            return;
        }

        int successCount = 0;
        int totalClients = nm.ConnectedClientsIds.Count;
        DebugTool.Log($"스폰 시작: 연결 클라이언트 {totalClients}명, 스폰 좌표 {spawnPoints.Length}개", DebugType.Network, this);

        // ConnectedClientsIds 는 호스트 자신(0) 포함. 호스트도 동일 경로로 스폰됨.
        // 순회 중 컬렉션 변경 방지를 위해 복사 (IReadOnlyList<ulong> → List<ulong>).
        List<ulong> clientIds = new List<ulong>(nm.ConnectedClientsIds);

        for (int i = 0; i < clientIds.Count; i++)
        {
            if (SpawnOne(clientIds[i], spawnPoints))
            {
                successCount++;
            }
        }

        DebugTool.Log($"스폰 완료: {successCount}/{totalClients}명", DebugType.Network, this);
    }

    /// <summary>
    /// 한 명의 클라이언트에 대해 spawn 시도. 실패 사유가 있으면 로그 + false 반환 (skip).
    /// </summary>
    private bool SpawnOne(ulong clientId, Transform[] spawnPoints)
    {
        // 1) clientId → PlayerInfo 조회
        if (PlayerSessionBridge.Instance == null)
        {
            DebugTool.Error($"PlayerSessionBridge.Instance 없음 - clientId={clientId} skip", DebugType.Network, this);
            return false;
        }

        if (!PlayerSessionBridge.Instance.TryGetPlayerInfo(clientId, out PlayerInfo info))
        {
            DebugTool.Warning($"clientId={clientId} 의 PlayerInfo 조회 실패 - skip", DebugType.Network, this);
            return false;
        }

        // 2) PlayerClass 가드
        if (info.PlayerClass == PlayerClass.None)
        {
            DebugTool.Warning($"clientId={clientId} PlayerClass=None - skip (로비에서 클래스 미선택 상태)", DebugType.Network, this);
            return false;
        }

        int classIndex = (int)info.PlayerClass;
        if (_classPrefabs == null || classIndex < 0 || classIndex >= _classPrefabs.Length)
        {
            DebugTool.Error(
                $"clientId={clientId} class={info.PlayerClass} 의 prefab 인덱스 {classIndex} 범위 외 (배열 길이 {_classPrefabs?.Length ?? 0}) - skip",
                DebugType.Network, this);
            return false;
        }

        NetworkObject prefab = _classPrefabs[classIndex];
        if (prefab == null)
        {
            DebugTool.Error($"clientId={clientId} class={info.PlayerClass} 의 prefab 미할당 (인스펙터 확인) - skip", DebugType.Network, this);
            return false;
        }

        // 3) SlotIndex 가드
        if (info.SlotIndex < 0 || info.SlotIndex >= spawnPoints.Length)
        {
            DebugTool.Error(
                $"clientId={clientId} SlotIndex={info.SlotIndex} 가 spawnPoints 범위 외 (배열 길이 {spawnPoints.Length}) - skip",
                DebugType.Network, this);

            // 진단 컨텍스트 덤프 - SlotIndex=-1 같은 이상치가 왜 발생했는지 한 번에 파악하기 위해
            // 호스트의 슬롯/매핑/세션 상태를 같이 찍는다. 부작용 없음.
            DumpDiagnosticsForBadSlot(clientId, info);
            return false;
        }

        Transform spawnPoint = spawnPoints[info.SlotIndex];
        if (spawnPoint == null)
        {
            DebugTool.Error($"clientId={clientId} SlotIndex={info.SlotIndex} 의 spawn point Transform 이 null - skip", DebugType.Network, this);
            return false;
        }

        // 4) 실제 스폰
        try
        {
            NetworkObject instance = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            instance.SpawnAsPlayerObject(clientId);
            _spawnedByClient[clientId] = instance;

            DebugTool.Log(
                $"spawn 성공: clientId={clientId}, class={info.PlayerClass}, slot={info.SlotIndex}, pos={spawnPoint.position}",
                DebugType.Network, this);

            // 세션 카운트 갱신 (Connected/Alive 양쪽 +1, idempotent).
            // ForceRespawnAll 처럼 같은 clientId 로 재호출되어도 SessionPlayerStateController 가
            // HashSet 으로 중복 처리하므로 안전.
            if (SessionPlayerStateController.Instance != null)
            {
                SessionPlayerStateController.Instance.NotifyPlayerSpawned(clientId);
            }
            else
            {
                DebugTool.Warning(
                    $"SessionPlayerStateController.Instance 가 null - spawn 통보 누락 (clientId={clientId}). GameScene 에 컴포넌트 배치 확인.",
                    DebugType.Network, this);
            }
            return true;
        }
        catch (Exception e)
        {
            DebugTool.Error($"clientId={clientId} spawn 예외: {e.Message}", DebugType.Network, this);
            return false;
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // Public lookup (Phase 후반에 사용 가능 - 예: 특정 clientId 의 PlayerObject 조회)
    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// 이 컨트롤러가 spawn 시킨 clientId 의 NetworkObject 조회. 미스폰이면 false.
    /// 주의: NGO 의 NetworkManager.ConnectedClients[clientId].PlayerObject 와 본질적으로 동일하지만,
    /// 이 컨트롤러가 직접 추적한 사본이라 디버깅 일관성용.
    /// </summary>
    public bool TryGetSpawnedPlayer(ulong clientId, out NetworkObject playerObject)
    {
        return _spawnedByClient.TryGetValue(clientId, out playerObject);
    }

    // ─────────────────────────────────────────────────────────────────
    // Diagnostics (호스트 측 SlotIndex 이상 감지 시 호출)
    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// SlotIndex 가 범위 외(특히 -1) 로 감지됐을 때 호스트 측 컨텍스트를 한 번에 덤프.
    /// - PlayerSessionBridge 의 clientId↔playerId 매핑
    /// - LobbyManager 의 _slotCache + SessionProperty["Slots"] 원본 JSON + session.Players 요약
    /// - NetworkManager.ConnectedClientsIds 전체
    /// Phase 1 진단용. 호출 자체에 부작용 없음.
    /// </summary>
    private void DumpDiagnosticsForBadSlot(ulong clientId, PlayerInfo info)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"[GameSpawnController] === SlotIndex 진단 (clientId={clientId}, class={info.PlayerClass}, slot={info.SlotIndex}) ===");

        // 1) Bridge 매핑
        if (PlayerSessionBridge.Instance != null)
        {
            sb.Append(PlayerSessionBridge.Instance.DumpClientIdMapping(clientId));
        }
        else
        {
            sb.AppendLine("  PlayerSessionBridge.Instance == null");
        }

        // 2) LobbyManager 슬롯 상태
        if (LobbyManager.Instance != null)
        {
            sb.Append(LobbyManager.Instance.DumpSlotState());
        }
        else
        {
            sb.AppendLine("  LobbyManager.Instance == null");
        }

        // 3) NGO ConnectedClientsIds
        NetworkManager nm = NetworkManager.Singleton;
        if (nm != null)
        {
            sb.Append($"  NetworkManager.ConnectedClientsIds = [");
            bool first = true;
            foreach (ulong cid in nm.ConnectedClientsIds)
            {
                if (!first) sb.Append(", ");
                sb.Append(cid);
                first = false;
            }
            sb.AppendLine("]");
        }
        else
        {
            sb.AppendLine("  NetworkManager.Singleton == null");
        }

        DebugTool.Error(sb.ToString(), DebugType.Network, this);
    }

    // ─────────────────────────────────────────────────────────────────
    // Debug helpers
    // ─────────────────────────────────────────────────────────────────

    [ContextMenu("Dump Spawned Players")]
    private void DumpSpawnedPlayers()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"GameSpawnController: 스폰된 플레이어 {_spawnedByClient.Count}명 (_spawned={_spawned})");
        foreach (KeyValuePair<ulong, NetworkObject> kv in _spawnedByClient)
        {
            string netState = kv.Value == null ? "null" : (kv.Value.IsSpawned ? "spawned" : "despawned");
            sb.AppendLine($"  clientId={kv.Key} → {(kv.Value != null ? kv.Value.name : "null")} ({netState})");
        }
        DebugTool.Log(sb.ToString(), DebugType.Network, this);
    }

    [ContextMenu("Force Respawn All (Debug Only)")]
    private void ForceRespawnAll()
    {
        if (!IsServer)
        {
            DebugTool.Warning("호스트가 아니라 강제 리스폰 불가", DebugType.Network, this);
            return;
        }

        // 기존 스폰 정리
        foreach (KeyValuePair<ulong, NetworkObject> kv in _spawnedByClient)
        {
            if (kv.Value != null && kv.Value.IsSpawned)
            {
                try { kv.Value.Despawn(); }
                catch (Exception e) { DebugTool.Warning($"despawn 예외: {e.Message}", DebugType.Network, this); }
            }
        }
        _spawnedByClient.Clear();
        _spawned = false;

        // 노드 시스템은 재초기화 안 함 (이미 IsInitialized=true). 스폰만 다시.
        NodeManager nodeManager = FindFirstObjectByType<NodeManager>();
        if (nodeManager == null)
        {
            DebugTool.Error("NodeManager 를 찾을 수 없음 - 강제 리스폰 중단", DebugType.Network, this);
            return;
        }
        SpawnAllConnectedClients(nodeManager);
        _spawned = true;
    }
}
