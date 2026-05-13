using UnityEngine;
using System.Collections.Generic;
using System;

public class TeamUpgradeData : MonoBehaviour
{
    public event Action<TeamUpgradeEntry, int> TeamUpgradeChanged;

    private readonly Dictionary<int, int> _upgradeLevel = new();

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
        return CurrentLevel * entry.ValuePerLevel;
    }

    //가능한지 판단
    public bool CanUpgrade(TeamUpgradeEntry entry)
    {
        if (entry == null)
        {
            return false;
        }
        int CurrentLevel = GetLevel(entry);

        if(CurrentLevel >=entry.MaxLevel)
        {
            return false;
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
        if(!CanUpgrade(entry))
        {
            return false;
        }

        int cost = UpgradeCost(entry);

        if(!_teamResourceManager.UseResource(_upgradeCostType, cost))
        {
            return false;
        }

        int beforeLevel = GetLevel(entry);
        int nextLevel = beforeLevel + 1;

        _upgradeLevel[entry.UpgradeId] = nextLevel;

        TeamUpgradeChanged?.Invoke(entry, nextLevel);
        return true;
    }
    // 빈 오브젝트에 붙여서 사용할 수 있는 간단한 스크립트입니다.
}
