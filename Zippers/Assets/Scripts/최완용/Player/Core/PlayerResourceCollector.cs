using System;
using Unity.Netcode;
using UnityEngine;
using Zippers.Network;

/// <summary>
/// [MIGRATION · Step 7] 호환 래퍼.
///
/// 자원 데이터는 다음 NetworkBehaviour 에서 관리:
///   - Scrap / InfectionSample → PlayerEconomyNetState (같은 GameObject)
///   - Supplies → TeamEconomyNetState (씬 singleton)
///
/// 외부 시그니처(IResourceCollectable 등)는 모두 보존 — 호출 코드 변경 불필요:
///   - Scrap / Supplies / InfectionSample 프로퍼티
///   - CollectResource(type, amount)
///   - HasEnoughResource(type, amount)
///   - UseResource(type, amount)
///   - OnResourceCollected / OnResourceUsed 이벤트
///
/// Step 7 변경: 옛 TeamResourceManager 의존 제거. Supplies 도 TeamEconomyNetState 로 직접.
/// </summary>
public class PlayerResourceCollector : MonoBehaviour, IResourceCollectable
{
    public Action<ResourcesType, float> OnResourceCollected;
    public Action<ResourcesType, float> OnResourceUsed;

    private PlayerEconomyNetState _economyNetState;

    private bool _economyEventBound;
    private bool _teamEventBound;
    private TeamEconomyNetState _boundTeamEconomy;

    // ── 프로퍼티 ─────────────────────────────────────────────────────
    public float Scrap =>
        _economyNetState != null ? _economyNetState.CurrentScrap : 0f;

    public float InfectionSample =>
        _economyNetState != null ? _economyNetState.CurrentInfectionSample : 0f;

    public float Supplies =>
        TeamEconomyNetState.Instance != null ? TeamEconomyNetState.Instance.CurrentSupplies : 0f;

    // ── Lifecycle ───────────────────────────────────────────────────

    private void Awake()
    {
        _economyNetState = GetComponent<PlayerEconomyNetState>();
        if (_economyNetState == null)
        {
            DebugTool.Log("[PlayerResourceCollector] PlayerEconomyNetState 가 같은 GameObject 에 없습니다.", DebugType.Data, this);
        }
    }

    private void OnDisable()
    {
        UnbindEvents();
    }

    private void OnDestroy()
    {
        UnbindEvents();
    }

    private void Update()
    {
        TryBindEvents();
    }

    private void TryBindEvents()
    {
        // PlayerEconomyNetState 이벤트
        if (!_economyEventBound && _economyNetState != null)
        {
            _economyNetState.OnResourceChanged += HandleEconomyResourceChanged;
            _economyEventBound = true;
        }

        // TeamEconomyNetState 이벤트
        if (!_teamEventBound && TeamEconomyNetState.Instance != null)
        {
            _boundTeamEconomy = TeamEconomyNetState.Instance;
            _boundTeamEconomy.OnTeamResourceChanged += HandleTeamResourceChanged;
            _teamEventBound = true;
        }
    }

    private void UnbindEvents()
    {
        if (_economyEventBound && _economyNetState != null)
        {
            _economyNetState.OnResourceChanged -= HandleEconomyResourceChanged;
            _economyEventBound = false;
        }
        if (_teamEventBound && _boundTeamEconomy != null)
        {
            _boundTeamEconomy.OnTeamResourceChanged -= HandleTeamResourceChanged;
            _boundTeamEconomy = null;
            _teamEventBound = false;
        }
    }

    // ── IResourceCollectable 구현 ───────────────────────────────────

    public void CollectResource(ResourcesType type, float amount)
    {
        // Supplies — 팀 공용 → TeamEconomyNetState 직접 호출
        if (type == ResourcesType.Supplies)
        {
            AddTeamSupplies(amount);
            return;
        }

        // Scrap / InfectionSample — 본인 PlayerEconomyNetState
        if (_economyNetState == null)
        {
            DebugTool.Log("[PlayerResourceCollector] PlayerEconomyNetState 미준비 - 무시", DebugType.Data, this);
            return;
        }

        ulong ownerId = _economyNetState.OwnerClientId;

        if (IsServer())
        {
            _economyNetState.ServerGrantResource(ownerId, type, amount, "PlayerResourceCollector.CollectResource");
        }
        else if (_economyNetState.IsOwner)
        {
            _economyNetState.RequestGrantResourceServerRpc((int)type, amount);
        }
        else
        {
            DebugTool.Log(
                $"[PlayerResourceCollector] CollectResource 클라 호출 무시 (IsOwner=false, type={type})",
                DebugType.Data, this);
        }
    }

    public bool HasEnoughResource(ResourcesType type, float amount)
    {
        if (amount <= 0f) return false;

        if (type == ResourcesType.Supplies)
        {
            return TeamEconomyNetState.Instance != null
                && TeamEconomyNetState.Instance.HasEnoughSupplies(amount);
        }

        if (_economyNetState == null) return false;
        return _economyNetState.HasEnoughResource(type, amount);
    }

    public bool UseResource(ResourcesType type, float amount)
    {
        if (amount <= 0f) return false;

        if (type == ResourcesType.Supplies)
        {
            TeamEconomyNetState team = TeamEconomyNetState.Instance;
            if (team == null)
            {
                DebugTool.Log("[PlayerResourceCollector] TeamEconomyNetState 미준비 - Supplies 사용 불가", DebugType.Data, this);
                return false;
            }

            if (IsServer())
            {
                return team.ServerSpendResource(0, type, amount, "PlayerResourceCollector.UseResource");
            }

            team.RequestSpendSuppliesServerRpc(amount);
            return true;   // 요청 송신
        }

        if (_economyNetState == null) return false;

        ulong ownerId = _economyNetState.OwnerClientId;

        if (IsServer())
        {
            return _economyNetState.ServerSpendResource(ownerId, type, amount, "PlayerResourceCollector.UseResource");
        }

        if (_economyNetState.IsOwner)
        {
            _economyNetState.RequestSpendResourceServerRpc((int)type, amount);
            return true;
        }

        DebugTool.Log(
            $"[PlayerResourceCollector] UseResource 클라 호출 무시 (IsOwner=false, type={type})",
            DebugType.Data, this);
        return false;
    }

    // ── 내부 헬퍼 ──────────────────────────────────────────────────

    private void AddTeamSupplies(float amount)
    {
        TeamEconomyNetState team = TeamEconomyNetState.Instance;
        if (team == null)
        {
            DebugTool.Log("[PlayerResourceCollector] TeamEconomyNetState 미준비 - Supplies 획득 불가", DebugType.Data, this);
            return;
        }

        if (IsServer())
        {
            team.ServerGrantResource(0, ResourcesType.Supplies, amount, "PlayerResourceCollector.CollectResource");
        }
        else
        {
            team.RequestGrantSuppliesServerRpc(amount);
        }
    }

    private void HandleEconomyResourceChanged(ResourcesType type, float current, float delta)
    {
        if (delta > 0f) OnResourceCollected?.Invoke(type, delta);
        else if (delta < 0f) OnResourceUsed?.Invoke(type, Mathf.Abs(delta));
    }

    private void HandleTeamResourceChanged(ResourcesType type, float current, float delta)
    {
        if (type != ResourcesType.Supplies) return;

        if (delta > 0f) OnResourceCollected?.Invoke(type, delta);
        else if (delta < 0f) OnResourceUsed?.Invoke(type, Mathf.Abs(delta));
    }

    private static bool IsServer()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;
    }
}
