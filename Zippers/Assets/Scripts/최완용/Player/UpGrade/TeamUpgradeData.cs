using System;
using Unity.Netcode;
using UnityEngine;
using Zippers.Network;

/// <summary>
/// [MIGRATION · Step 4] 호환 래퍼.
///
/// 원래 이 컴포넌트가 자체적으로 _upgradeLevel Dictionary, _purchasedUpgrades HashSet 을
/// 들고 팀 업그레이드를 관리했지만, 멀티 전환을 위해 데이터를 TeamEconomyNetState 의
/// NetworkList&lt;UpgradeLevelEntry&gt; / NetworkList&lt;int&gt; 로 옮겼다.
///
/// 외부 시그니처는 모두 보존 (호출 코드 변경 불필요):
///   - GetLevel(TeamUpgradeEntry) / GetLevel(int)
///   - UpgradeCost(TeamUpgradeEntry)
///   - GetValue(TeamUpgradeEntry)            — 일회성/영구 분기 누적
///   - CanUpgrade(TeamUpgradeEntry)          — UI 표시용 검증
///   - Upgrade(TeamUpgradeEntry)             — 호스트 검증 요청 (낙관 반환)
///   - ResetCurrentShopPurchases()           — 다음 상점 진입 시
///   - ClearOneTimeBattleUpgrades()          — 전투 종료 시
///   - TeamUpgradeChanged 이벤트
///
/// 내부 동작:
///   - 모든 레벨/구매기록 조회·변경은 TeamEconomyNetState 로 위임
///   - Supplies 차감도 TeamEconomyNetState.ServerTryUpgradeTeam 내부에서 처리
///   - TeamEconomyNetState.OnTeamUpgradeChanged 를 받아 TeamUpgradeChanged 이벤트로 재방출
/// </summary>
public class TeamUpgradeData : MonoBehaviour
{
    /// <summary>팀 업그레이드 적용 시 호출. (entry, newLevel)</summary>
    public event Action<TeamUpgradeEntry, int> TeamUpgradeChanged;

    private bool _eventBound;

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
        TryBindEvents();   // NetworkBehaviour spawn 시점을 기다려 lazy bind
    }

    private void TryBindEvents()
    {
        if (_eventBound) return;
        TeamEconomyNetState net = TeamEconomyNetState.Instance;
        if (net == null) return;

        net.OnTeamUpgradeChanged += HandleTeamUpgradeChanged;
        _eventBound = true;
    }

    private void UnbindEvents()
    {
        if (!_eventBound) return;
        TeamEconomyNetState net = TeamEconomyNetState.Instance;
        if (net != null)
        {
            net.OnTeamUpgradeChanged -= HandleTeamUpgradeChanged;
        }
        _eventBound = false;
    }

    // ── 조회 ────────────────────────────────────────────────────────

    public int GetLevel(TeamUpgradeEntry entry)
    {
        if (entry == null) return 0;
        return GetLevel(entry.UpgradeId);
    }

    public int GetLevel(int upgradeId)
    {
        return TeamEconomyNetState.Instance != null
            ? TeamEconomyNetState.Instance.GetTeamLevel(upgradeId)
            : 0;
    }

    public int UpgradeCost(TeamUpgradeEntry entry)
    {
        if (entry == null) return 0;
        int currentLevel = GetLevel(entry);
        return entry.BaseCost + entry.CostIncrease * currentLevel;
    }

    /// <summary>
    /// 누적 보너스 값.
    /// - 일회성(ClearHeal/Battle*): level &gt; 0 ? ValuePerLevel : 0
    /// - 영구: level * ValuePerLevel
    /// </summary>
    public float GetValue(TeamUpgradeEntry entry)
    {
        if (entry == null) return 0f;
        int currentLevel = GetLevel(entry);

        if (IsOneShotBattleStat(entry.StatKey))
        {
            return currentLevel > 0 ? entry.ValuePerLevel : 0f;
        }

        return currentLevel * entry.ValuePerLevel;
    }

    // ── 검증 (UI 표시용 — 호스트도 같은 로직으로 다시 검증) ──────────

    public bool CanUpgrade(TeamUpgradeEntry entry)
    {
        if (entry == null) return false;

        TeamEconomyNetState net = TeamEconomyNetState.Instance;
        if (net == null) return false;

        // 같은 상점 재구매 방지
        if (net.IsPurchasedInCurrentShop(entry.UpgradeId))
        {
            DebugTool.Log($"[TeamUpgradeData] 같은 상점에서 이미 구매한 업그레이드: {entry.UpgradeId}", DebugType.Data, this);
            return false;
        }

        int currentLevel = GetLevel(entry);

        // 영구만 MaxLevel 검사
        if (!IsOneShotBattleStat(entry.StatKey) && currentLevel >= entry.MaxLevel)
        {
            return false;
        }

        int cost = UpgradeCost(entry);
        if (!net.HasEnoughSupplies(cost))
        {
            DebugTool.Log($"[TeamUpgradeData] 팀 업그레이드 비용 부족 / 필요 Supplies: {cost}", DebugType.Data, this);
            return false;
        }

        return true;
    }

    // ── 업그레이드 요청 (서버 권한) ─────────────────────────────────

    /// <summary>
    /// 호스트에 팀 업그레이드 구매를 요청한다.
    /// 반환값은 "요청 송신 성공" 의미 — 실제 결과는 TeamUpgradeChanged 이벤트로 통보.
    /// </summary>
    public bool Upgrade(TeamUpgradeEntry entry)
    {
        if (entry == null) return false;

        TeamEconomyNetState net = TeamEconomyNetState.Instance;
        if (net == null)
        {
            DebugTool.Log("[TeamUpgradeData] TeamEconomyNetState 미준비", DebugType.Data, this);
            return false;
        }

        if (!CanUpgrade(entry)) return false;

        if (IsServer())
        {
            return net.ServerTryUpgradeTeam(entry.UpgradeId);
        }

        net.RequestUpgradeTeamServerRpc(entry.UpgradeId);
        return true;   // 요청 송신
    }

    // ── 상점 / 전투 종료 처리 ─────────────────────────────────────

    public void ResetCurrentShopPurchases()
    {
        TeamEconomyNetState net = TeamEconomyNetState.Instance;
        if (net == null) return;

        if (IsServer())
        {
            net.ServerResetCurrentShopPurchases();
        }
        else
        {
            net.RequestResetCurrentShopPurchasesServerRpc();
        }
    }

    public void ClearOneTimeBattleUpgrades()
    {
        TeamEconomyNetState net = TeamEconomyNetState.Instance;
        if (net == null) return;

        if (IsServer())
        {
            net.ServerClearOneTimeBattleUpgrades();
        }
        else
        {
            net.RequestClearOneTimeBattleUpgradesServerRpc();
        }
    }

    // ── NetworkList 변경 → TeamUpgradeChanged 이벤트 재방출 ─────────

    private void HandleTeamUpgradeChanged(int upgradeId, int newLevel)
    {
        if (LocalDataAccess.Instance == null || LocalDataAccess.Instance.Game == null) return;

        TeamUpgradeEntry entry = LocalDataAccess.Instance.Game.GetTeamUpgrade(upgradeId);
        if (entry == null)
        {
            DebugTool.Log($"[TeamUpgradeData] TeamUpgradeChanged: id={upgradeId} entry 조회 실패", DebugType.Data, this);
            return;
        }

        TeamUpgradeChanged?.Invoke(entry, newLevel);
    }

    // ── Helper ────────────────────────────────────────────────────

    private static bool IsOneShotBattleStat(TeamUpgradeStatKey key)
    {
        return key == TeamUpgradeStatKey.ClearHeal
            || key == TeamUpgradeStatKey.BattleDamage
            || key == TeamUpgradeStatKey.BattleMoveSpeed
            || key == TeamUpgradeStatKey.BattleAttackSpeed;
    }

    private static bool IsServer()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;
    }
}
