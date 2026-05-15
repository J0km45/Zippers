using System;
using Unity.Netcode;
using UnityEngine;
using Zippers.Network;

/// <summary>
/// [MIGRATION · Step 7] ⚠️ 슬림 호환 래퍼 — 곧 삭제 예정.
///
/// 실제 데이터는 TeamEconomyNetState 가 NetworkVariable&lt;float&gt; Supplies 로 관리.
/// 이 클래스는 다음 디버그/임시 코드의 호환을 위해서만 잠시 살려둔다:
///   - TeamUpgradeDebugTest.cs (B 영역, 인스펙터 SerializeField 참조)
///
/// 이 클래스가 완전히 삭제될 시점:
///   B 가 TeamUpgradeDebugTest 를 TeamEconomyNetState 직접 호출로 정리한 직후.
///
/// 신규 코드는 본 클래스를 호출하지 말고 TeamEconomyNetState.Instance 를 직접 쓸 것.
/// </summary>
[Obsolete("Use TeamEconomyNetState.Instance directly. This wrapper exists only for TeamUpgradeDebugTest compatibility.", false)]
public class TeamResourceManager : MonoBehaviour
{
    public static TeamResourceManager Instance { get; private set; }

    /// <summary>호환용 이벤트. TeamEconomyNetState.OnTeamResourceChanged 를 그대로 재방출.</summary>
    public event Action<ResourcesType, float, float> TeamResourceChanged;

    /// <summary>현재 Supplies — TeamEconomyNetState 의 NetworkVariable 값을 그대로 노출.</summary>
    public float Supplies =>
        TeamEconomyNetState.Instance != null ? TeamEconomyNetState.Instance.Supplies.Value : 0f;

    private bool _eventBound;
    private TeamEconomyNetState _boundTo;

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

    private void Update()
    {
        TryBindEvents();
    }

    private void TryBindEvents()
    {
        if (_eventBound) return;
        TeamEconomyNetState net = TeamEconomyNetState.Instance;
        if (net == null) return;

        net.OnTeamResourceChanged += RelayTeamResourceChanged;
        _boundTo = net;
        _eventBound = true;
    }

    private void UnbindEvents()
    {
        if (!_eventBound) return;
        if (_boundTo != null) _boundTo.OnTeamResourceChanged -= RelayTeamResourceChanged;
        _boundTo = null;
        _eventBound = false;
    }

    public bool AddResource(ResourcesType type, float amount)
    {
        if (amount <= 0f) return false;
        if (type != ResourcesType.Supplies) return false;

        TeamEconomyNetState net = TeamEconomyNetState.Instance;
        if (net == null) return false;

        if (IsServer())
        {
            return net.ServerGrantResource(0, type, amount, "TeamResourceManager.AddResource");
        }

        net.RequestGrantSuppliesServerRpc(amount);
        return true;
    }

    public bool HasEnoughResource(ResourcesType type, float amount)
    {
        if (amount <= 0f) return false;
        if (type != ResourcesType.Supplies) return false;
        return TeamEconomyNetState.Instance != null && TeamEconomyNetState.Instance.HasEnoughSupplies(amount);
    }

    public bool UseResource(ResourcesType type, float amount)
    {
        if (amount <= 0f) return false;
        if (type != ResourcesType.Supplies) return false;

        TeamEconomyNetState net = TeamEconomyNetState.Instance;
        if (net == null) return false;

        if (IsServer())
        {
            return net.ServerSpendResource(0, type, amount, "TeamResourceManager.UseResource");
        }

        net.RequestSpendSuppliesServerRpc(amount);
        return true;
    }

    public float GetResourceAmount(ResourcesType type)
    {
        if (type != ResourcesType.Supplies) return 0f;
        return Supplies;
    }

    private void RelayTeamResourceChanged(ResourcesType type, float current, float delta)
    {
        TeamResourceChanged?.Invoke(type, current, delta);
    }

    private static bool IsServer()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;
    }
}
