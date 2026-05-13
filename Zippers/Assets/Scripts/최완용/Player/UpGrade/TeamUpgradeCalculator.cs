using System.Collections.Generic;
using UnityEngine;

/// 팀 업그레이드 레벨을 기준으로 최종 스탯 보정값을 계산한다.
public class TeamUpgradeCalculator : MonoBehaviour
{
    [Header("팀 업그레이드 참조")]
    [SerializeField] private TeamUpgradeData _teamUpgradeData;
    [SerializeField] private TeamUpgradeProvider _teamUpgradeProvider;

    private void Awake()
    {
        if (_teamUpgradeData == null)
        {
            _teamUpgradeData = GetComponent<TeamUpgradeData>();
        }

        if (_teamUpgradeProvider == null)
        {
            _teamUpgradeProvider = GetComponent<TeamUpgradeProvider>();
        }

        if (_teamUpgradeData == null)
        {
            DebugTool.Log("[TeamUpgradeCalculator] TeamUpgradeData가 없습니다.", DebugType.Data, this);
        }

        if (_teamUpgradeProvider == null)
        {
            DebugTool.Log("[TeamUpgradeCalculator] TeamUpgradeProvider가 없습니다.", DebugType.Data, this);
        }
    }

    // 팀 업그레이드가 적용된 값을 반환한다.
    // isCooldownStat이 true면 퍼센트 증가를 쿨타임 감소로 계산한다.
    public float Apply(float baseValue, TeamUpgradeStatKey statKey, bool isCooldownStat = false)
    {
        if (_teamUpgradeData == null || _teamUpgradeProvider == null)
        {
            return baseValue;
        }

        List<TeamUpgradeEntry> upgrades = _teamUpgradeProvider.GetAllUpgrades();

        float addValue = 0f;
        float percentValue = 0f;

        foreach (TeamUpgradeEntry entry in upgrades)
        {
            if (entry == null)
            {
                continue;
            }

            if (!IsPermanentStat(entry.StatKey))
            {
                continue;
            }

            if (entry.StatKey != statKey)
            {
                continue;
            }

            int level = _teamUpgradeData.GetLevel(entry);

            if (level <= 0)
            {
                continue;
            }

            float value = entry.ValuePerLevel * level;

            switch (entry.ApplyType)
            {
                case TeamUpgradeApplyType.Add:
                    addValue += value;
                    break;

                case TeamUpgradeApplyType.AddPercent:
                    percentValue += value;
                    break;
            }
        }

        float result = baseValue + addValue;

        if (isCooldownStat)
        {
            result *= 1f - percentValue;
            return Mathf.Max(0.01f, result);
        }

        result *= 1f + percentValue;
        return result;
    }

    /// <summary>
    /// PlayerStats에 항상 반영되는 영구 팀 스탯인지 확인한다.
    /// </summary>
    private bool IsPermanentStat(TeamUpgradeStatKey statKey)
    {
        switch (statKey)
        {
            case TeamUpgradeStatKey.MaxHealth:
            case TeamUpgradeStatKey.Stamina:
            case TeamUpgradeStatKey.Damage:
            case TeamUpgradeStatKey.AttackSpeed:
            case TeamUpgradeStatKey.MoveSpeed:
                return true;

            default:
                return false;
        }
    }
}