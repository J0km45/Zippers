using System;
using Unity.Netcode;
using UnityEngine;
using Zippers.Network;

/// <summary>
/// [MIGRATION · Step 1] 호환 래퍼.
///
/// 원래 이 클래스가 자체적으로 _supplies 를 들고 팀 재화를 관리했지만,
/// 멀티 전환을 위해 TeamEconomyNetState (NetworkBehaviour) 로 데이터를 옮겼다.
/// 이 파일은 기존 호출 코드가 그대로 동작하도록 잠시 살려두는 어댑터이다.
///
/// 외부 시그니처는 모두 보존 (호출 코드 변경 불필요):
///   - TeamResourceManager.Instance.Supplies
///   - TeamResourceManager.Instance.AddResource(type, amount)
///   - TeamResourceManager.Instance.UseResource(type, amount)
///   - TeamResourceManager.Instance.HasEnoughResource(type, amount)
///   - TeamResourceManager.Instance.GetResourceAmount(type)
///   - TeamResourceManager.Instance.TeamResourceChanged 이벤트
///
/// 내부 동작:
///   - 모든 데이터/검증/변경은 TeamEconomyNetState.Instance 로 위임
///   - TeamEconomyNetState 의 OnTeamResourceChanged 를 받아 자기 TeamResourceChanged 로 재방출
///
/// 진행 단계 (§6-2):
///   ✅ Step 1: 이 래퍼로 변환 — 기존 호출 깨지지 않음
///   ⏳ Step 2: PlayerResourceCollector / TeamUpgradeData 의 호출처를
///              TeamEconomyNetState.Instance 로 일괄 치환
///   ⏳ Step 2 말미: 본 파일 삭제
///
/// 호출 위치 (grep 기준):
///   - PlayerResourceCollector.cs
///   - TeamUpgradeData.cs
/// </summary>
public class TeamResourceManager : MonoBehaviour
{
    public static TeamResourceManager Instance { get; private set; }

    /// <summary>호환용 이벤트. TeamEconomyNetState.OnTeamResourceChanged 를 그대로 재방출.</summary>
    public event Action<ResourcesType, float, float> TeamResourceChanged;

    /// <summary>현재 Supplies — TeamEconomyNetState 의 NetworkVariable 값을 그대로 노출.</summary>
    public float Supplies =>
        TeamEconomyNetState.Instance != null
            ? TeamEconomyNetState.Instance.Supplies.Value
            : 0f;

    // TeamEconomyNetState 의 이벤트 구독 상태
    private bool _eventBound;
    private TeamEconomyNetState _boundTo;

    // ─────────────────────────────────────────────────────────────
    // Lifecycle
    // ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            return;
        }

        if (Instance != this)
        {
            DebugTool.Log("[TeamResourceManager] 중복 인스턴스 - 제거", DebugType.Data, this);
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        UnbindEvents();
    }

    private void OnDestroy()
    {
        UnbindEvents();
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// TeamEconomyNetState 는 NetworkBehaviour 라 NGO 의 OnNetworkSpawn 시점이 늦을 수 있다.
    /// 매 프레임 idempotent 하게 시도. 한 번 연결되면 _eventBound 가 true 라 더 안 함.
    /// 마이그레이션 끝나면 이 파일 자체가 삭제될 것이므로 임시 비용으로 허용.
    /// </summary>
    private void Update()
    {
        TryBindEvents();
    }

    private void TryBindEvents()
    {
        if (_eventBound) return;

        TeamEconomyNetState net = TeamEconomyNetState.Instance;
        if (net == null) return;

        net.OnTeamResourceChanged += HandleTeamResourceChanged;
        _boundTo = net;
        _eventBound = true;

        DebugTool.Log(
            "[TeamResourceManager] TeamEconomyNetState 이벤트 연결 완료",
            DebugType.Data, this);
    }

    private void UnbindEvents()
    {
        if (!_eventBound) return;
        if (_boundTo != null)
        {
            _boundTo.OnTeamResourceChanged -= HandleTeamResourceChanged;
        }
        _boundTo = null;
        _eventBound = false;
    }

    // ─────────────────────────────────────────────────────────────
    // 기존 시그니처 — TeamEconomyNetState 로 위임
    // ─────────────────────────────────────────────────────────────

    public bool AddResource(ResourcesType type, float amount)
    {
        if (amount <= 0f)
        {
            DebugTool.Log($"[TeamResourceManager] 증가량 부적절: {amount}", DebugType.Data, this);
            return false;
        }

        if (type != ResourcesType.Supplies)
        {
            DebugTool.Log($"[TeamResourceManager] 팀 재화로 관리하지 않는 타입: {type}", DebugType.Data, this);
            return false;
        }

        TeamEconomyNetState net = TeamEconomyNetState.Instance;
        if (net == null)
        {
            DebugTool.Log("[TeamResourceManager] TeamEconomyNetState 미준비 - 무시", DebugType.Data, this);
            return false;
        }

        // 호스트면 직접 호출, 클라이언트면 ServerRpc 발행
        if (IsServer())
        {
            return net.ServerGrantResource(LocalClientId(), type, amount, "TeamResourceManager.AddResource");
        }

        net.RequestGrantSuppliesServerRpc(amount);
        return true;   // 요청 송신 성공 의미. 실제 검증 결과는 NetworkVariable 동기화로 확인.
    }

    public bool HasEnoughResource(ResourcesType type, float amount)
    {
        if (amount <= 0f) return false;
        if (type != ResourcesType.Supplies) return false;

        TeamEconomyNetState net = TeamEconomyNetState.Instance;
        if (net == null) return false;

        return net.HasEnoughSupplies(amount);
    }

    public bool UseResource(ResourcesType type, float amount)
    {
        if (amount <= 0f) return false;
        if (type != ResourcesType.Supplies)
        {
            DebugTool.Log($"[TeamResourceManager] 팀 재화로 사용하지 않는 타입: {type}", DebugType.Data, this);
            return false;
        }

        TeamEconomyNetState net = TeamEconomyNetState.Instance;
        if (net == null)
        {
            DebugTool.Log("[TeamResourceManager] TeamEconomyNetState 미준비", DebugType.Data, this);
            return false;
        }

        if (IsServer())
        {
            return net.ServerSpendResource(LocalClientId(), type, amount, "TeamResourceManager.UseResource");
        }

        net.RequestSpendSuppliesServerRpc(amount);
        return true;   // 요청 송신 성공 의미.
    }

    public float GetResourceAmount(ResourcesType type)
    {
        if (type != ResourcesType.Supplies) return 0f;
        return Supplies;
    }

    // ─────────────────────────────────────────────────────────────
    // Internal helpers
    // ─────────────────────────────────────────────────────────────

    private void HandleTeamResourceChanged(ResourcesType type, float current, float delta)
    {
        TeamResourceChanged?.Invoke(type, current, delta);
    }

    private static bool IsServer()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;
    }

    private static ulong LocalClientId()
    {
        return NetworkManager.Singleton != null ? NetworkManager.Singleton.LocalClientId : 0UL;
    }
}
