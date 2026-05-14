using UnityEngine;
using System.Collections.Generic;
using System;

public class TeamUpgradeData : MonoBehaviour
{
    public event Action<TeamUpgradeEntry, int> TeamUpgradeChanged;

    private readonly Dictionary<int, int> _upgradeLevel = new();

    //현재상점에서 구매한 업그레이드 목록
    private readonly HashSet<int> _purchasedUpgrades = new();

    [Header("재화 설정")]
    [SerializeField] private TeamResourceManager _teamResourceManager;
    [SerializeField] private ResourcesType _upgradeCostType = ResourcesType.Supplies;

    private void Awake()
    {
        if (_teamResourceManager == null)
        {
            _teamResourceManager = GetComponent<TeamResourceManager>();
        }

        if (_teamResourceManager == null)
        {
            _teamResourceManager = TeamResourceManager.Instance;
        }

        if (_teamResourceManager == null)
        {
            DebugTool.Log("[TeamUpgradeData] TeamResourceManager를 찾지 못했습니다.", DebugType.Data, this);
        }
    }

    public int GetLevel(TeamUpgradeEntry entry)
    {
        if (entry == null)
        {
            return 0;
        }

        return GetLevel(entry.UpgradeId);
    }
    //레벨반환
    public int GetLevel(int upgradeId)
    {
        return _upgradeLevel.TryGetValue(upgradeId, out int level) ? level : 0;
    }

    //업그레이드 비용
    public int UpgradeCost(TeamUpgradeEntry entry)
    {
        if (entry == null)
        {
            return 0;
        }
        int CurrentLevel = GetLevel(entry);
        return entry.BaseCost + entry.CostIncrease * CurrentLevel;
    }

    //누적값 반환
    public float GetValue(TeamUpgradeEntry entry)
    {
        if (entry == null)
        {
            return 0f;
        }
        int CurrentLevel = GetLevel(entry);

        if (entry.StatKey == TeamUpgradeStatKey.ClearHeal ||
            entry.StatKey == TeamUpgradeStatKey.BattleDamage ||
            entry.StatKey == TeamUpgradeStatKey.BattleMoveSpeed ||
            entry.StatKey == TeamUpgradeStatKey.BattleAttackSpeed)
        {
            return CurrentLevel > 0 ? entry.ValuePerLevel : 0f;
        }

        return CurrentLevel * entry.ValuePerLevel;
    }

    //가능한지 판단
    //UI 버튼에 연결하기
    public bool CanUpgrade(TeamUpgradeEntry entry)
    {
        //TODO : 팀 업그레이드 가능 여부는 서버가 Supplies, MaxLevel 기준으로 검증해야됨
        if (entry == null)
        {
            return false;
        }

        // 같은 상점에서는 같은 팀 업그레이드를 1번만 구매 가능
        if (_purchasedUpgrades.Contains(entry.UpgradeId))
        {
            DebugTool.Log($"[TeamUpgradeData] 현재 상점에서 이미 구매한 업그레이드입니다. ID: {entry.UpgradeId}", DebugType.Data, this);
            return false;
        }

        int CurrentLevel = GetLevel(entry);

        // 51005~51009 같은 영구 업그레이드만 MaxLevel 검사
        // 51001~51004는 다음 상점에서 다시 구매 가능해야 해서 MaxLevel로 막지 않음
        if (entry.StatKey != TeamUpgradeStatKey.ClearHeal &&
            entry.StatKey != TeamUpgradeStatKey.BattleDamage &&
            entry.StatKey != TeamUpgradeStatKey.BattleMoveSpeed &&
            entry.StatKey != TeamUpgradeStatKey.BattleAttackSpeed)
        {
            if (CurrentLevel >= entry.MaxLevel)
            {
                return false;
            }
        }

        if (_teamResourceManager == null)
        {
            DebugTool.Log("[TeamUpgradeData] 팀 재화 관리자가 없습니다.", DebugType.Data, this);
            return false;
        }

        int cost = UpgradeCost(entry);

        if (!_teamResourceManager.HasEnoughResource(_upgradeCostType, cost))
        {
            DebugTool.Log($"[TeamUpgradeData] 팀 업그레이드 비용 부족 / 필요 Supplies: {cost}", DebugType.Data, this);
            return false;
        }

        return true;
    }

    //팀 업그레이드 1레벨 증가
    public bool Upgrade(TeamUpgradeEntry entry)
    {
        //TODO : 팀 업그레이드는 클라이언트가 직접 적용하지않고 ID만 서버에 요청해야됨
        if (!CanUpgrade(entry))
        {
            return false;
        }

        int cost = UpgradeCost(entry);

        //TODO : 팀 업그레이드 비용 차감을 서버 기준으로 처리해야됨       
        if (!_teamResourceManager.UseResource(_upgradeCostType, cost))
        {
            return false;
        }

        // 같은 상점에서 다시 못 사도록 구매 기록 저장
        _purchasedUpgrades.Add(entry.UpgradeId);

        //TODO : 팀 업그레이드 레벨 Dictionary는 서버 기준으로 관리하고 모든 클라이언트에 동기화
        int beforeLevel = GetLevel(entry);
        int nextLevel = beforeLevel + 1;

        // 51001~51004는 일회성이므로 누적 레벨 증가가 아니라 1로만 저장
        if (entry.StatKey == TeamUpgradeStatKey.ClearHeal ||
            entry.StatKey == TeamUpgradeStatKey.BattleDamage ||
            entry.StatKey == TeamUpgradeStatKey.BattleMoveSpeed ||
            entry.StatKey == TeamUpgradeStatKey.BattleAttackSpeed)
        {
            nextLevel = 1;
        }

        _upgradeLevel[entry.UpgradeId] = nextLevel;

        //TODO : 이벤트는 서버 승인후 변경돈 팀 업그레이드 레벨을 받은 뒤 호출해야됨
        TeamUpgradeChanged?.Invoke(entry, nextLevel);
        return true;
    }
    // 다음 상점에 들어갈 때 호출한다.
    public void ResetCurrentShopPurchases()
    {
        _purchasedUpgrades.Clear();
        DebugTool.Log("[TeamUpgradeData] 현재 상점 구매 기록 초기화", DebugType.Data, this);
    }

    // 전투 종료 시 다음 전투 일회성 업그레이드를 제거한다.
    public void ClearOneTimeBattleUpgrades()
    {
        foreach (int upgradeId in LocalDataAccess.Instance.Game.GetAllTeamUpgradeIds())
        {
            TeamUpgradeEntry entry = LocalDataAccess.Instance.Game.GetTeamUpgrade(upgradeId);

            if (entry == null)
            {
                continue;
            }

            if (entry.StatKey == TeamUpgradeStatKey.ClearHeal ||
                entry.StatKey == TeamUpgradeStatKey.BattleDamage ||
                entry.StatKey == TeamUpgradeStatKey.BattleMoveSpeed ||
                entry.StatKey == TeamUpgradeStatKey.BattleAttackSpeed)
            {
                _upgradeLevel[entry.UpgradeId] = 0;
            }
        }

        DebugTool.Log("[TeamUpgradeData] 다음 전투 일회성 팀 업그레이드 초기화", DebugType.Data, this);
    }
    // 빈 오브젝트에 붙여서 사용할 수 있는 간단한 스크립트입니다.
}
