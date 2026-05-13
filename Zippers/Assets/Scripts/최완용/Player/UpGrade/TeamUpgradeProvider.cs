using System.Collections.Generic;
using UnityEngine;

// 시트에서 로드된 팀 업그레이드 데이터를 제공한다.
public class TeamUpgradeProvider : MonoBehaviour
{
    // 시트에서 전체 팀 업그레이드 목록을 가져온다.
    public List<TeamUpgradeEntry> GetAllUpgrades()
    {
        List<TeamUpgradeEntry> result = new();

        if (LocalDataAccess.Instance == null || LocalDataAccess.Instance.Game == null)
        {
            DebugTool.Log("[TeamUpgradeProvider] LocalDataAccess가 없습니다.", DebugType.Data, this);
            return result;
        }

        foreach (int upgradeId in LocalDataAccess.Instance.Game.GetAllTeamUpgradeIds())
        {
            TeamUpgradeEntry entry = LocalDataAccess.Instance.Game.GetTeamUpgrade(upgradeId);

            if (entry == null)
            {
                DebugTool.Log($"[TeamUpgradeProvider] 팀 업그레이드 데이터를 찾을 수 없습니다. ID: {upgradeId}", DebugType.Data, this);
                continue;
            }

            result.Add(entry);
        }

        return result;
    }

    // 최대 레벨에 도달하지 않은 팀 업그레이드 목록을 가져온다.
    public List<TeamUpgradeEntry> GetAbleUpgrades(TeamUpgradeData teamUpgradeData)
    {
        List<TeamUpgradeEntry> result = new();
        List<TeamUpgradeEntry> allUpgrades = GetAllUpgrades();

        foreach (TeamUpgradeEntry entry in allUpgrades)
        {
            if (entry == null)
            {
                continue;
            }

            if (teamUpgradeData != null && teamUpgradeData.GetLevel(entry) >= entry.MaxLevel)
            {
                continue;
            }

            result.Add(entry);
        }

        return result;
    }
}