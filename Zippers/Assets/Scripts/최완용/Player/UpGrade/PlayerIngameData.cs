using UnityEngine;
using System;
using System.Collections.Generic;

public class PlayerIngameData : MonoBehaviour
{
    private readonly Dictionary<int, int> _upgradeLevel = new();

    //업그레이드 할떄 호출
    public event Action<UpgradeEntry, int> UpgradeChange;

    private PlayerResourceCollector _resourceCollector;

    private readonly ResourcesType _upgradeCostType = ResourcesType.Scrap;

    private void Awake()
    {
        _resourceCollector = GetComponent<PlayerResourceCollector>();

        if (_resourceCollector == null)
        {
            DebugTool.Log("[PlayerIngameData] PlayerResourceCollector가 없습니다.", DebugType.Data, this);
        }
    }

    //현재 레벨 받아오기
    public int GetLevel(UpgradeEntry entry)
    {
        if(entry == null)
        {
            return 0;
        }
        return GetLevel(entry.Id);
    }

    //id 기준 레벨
    public int GetLevel(int upgradeId)
    {
        return _upgradeLevel.TryGetValue(upgradeId, out int level) ? level : 0;
    }

    //업그레이드 비용
    public int UpgradeCost(UpgradeEntry entry)
    {
        if(entry == null)
        {
            return 0;
        }
        int currentLevel = GetLevel(entry);
        return entry.BaseCost + entry.CostIncrease * currentLevel;
    }

    //레벨 기준 스텟 값 변화
    public float GetValue(UpgradeEntry entry)
    {
        if (entry == null)
        {
            return 0;
        }
        int currentLevel = GetLevel(entry);
        return entry.ValuePerLevel * currentLevel;
    }

    public bool CanUpgrade(UpgradeEntry entry)
    {
        if (entry == null)
        {
            return false;
        }

        if (!entry.IsEnabled)
        {
            return false;
        }
        int currentLevel = GetLevel(entry);

        if (currentLevel >= entry.MaxLevel)
        {
            return false;
        }

        int cost = UpgradeCost(entry);

        if (!_resourceCollector.HasEnoughResource(_upgradeCostType, cost))
        {
            DebugTool.Log($"업그레이드 비용 부족 / 필요: {cost}", DebugType.Data, this);
            return false;
        }


        return true;
    }

    //업그레이드 중가 1만큼
    public bool Upgrade(UpgradeEntry entry)
    {
        if (!CanUpgrade(entry))
        {
            return false;
        }
        int cost = UpgradeCost(entry);
        if (!_resourceCollector.UseResource(_upgradeCostType, cost))
        {
            DebugTool.Log($"Scrap 차감 실패 / 필요 Scrap: {cost}", DebugType.Data, this);
            return false;
        }

        int beforeLevel = GetLevel(entry);
        int nextLevel = GetLevel(entry) + 1;
        _upgradeLevel[entry.Id] = nextLevel;

        UpgradeChange?.Invoke(entry, nextLevel);
        return true;
    }
   
}
