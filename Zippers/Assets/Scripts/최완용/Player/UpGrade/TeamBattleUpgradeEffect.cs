using System.Collections.Generic;
using UnityEngine;

public class TeamBattleUpgradeEffect : MonoBehaviour
{
    [SerializeField] private TeamUpgradeData _teamUpgradeData;
    [SerializeField] private TeamUpgradeProvider _teamUpgradeProvider;
    [SerializeField] private MapData _mapData;

    [SerializeField] private bool _isBattle;

    public bool IsBattle => _isBattle;

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
        if (_mapData == null)
        {
            _mapData = FindFirstObjectByType<MapData>();
        }
    }

    private void OnEnable()
    {
        if(_mapData == null)
        {
            _mapData = FindFirstObjectByType<MapData>();
            return;
        }
        //TODO 실제 전투시작 클리어 종료 이벤트 구독하기
        _mapData.NetworkMapData.NodeState.OnValueChanged += NodeStateChange;

        //예시
        //BattleManager.Instance.OnBattleStarted += StartBattle;
        //BattleManager.Instance.OnBattleCleared += ClearBattle;
        //BattleManager.Instance.OnBattleEnded += EndBattle;
    }
    private void OnDisable()
    {
        if(_mapData ==null)
        {
            return;
        }
        //TODO 실제 전투시작 클리어 종료 이벤트 구독 해제하기
        _mapData.NetworkMapData.NodeState.OnValueChanged -= NodeStateChange;
        //예시
        //BattleManager.Instance.OnBattleStarted -= StartBattle;
        //BattleManager.Instance.OnBattleCleared -= ClearBattle;
        //BattleManager.Instance.OnBattleEnded -= EndBattle;
    }
    private void NodeStateChange(NodeState temp, NodeState state)
    {
        switch (state)
        {
            case NodeState.Ready:
                EndBattle();
                break;

            case NodeState.Battle:
                StartBattle();
                break;

            case NodeState.Clear:
                ClearBattle();
                break;
        }

        DebugTool.Log($"[TeamBattleUpgradeEffect] NodeState 이벤트 수신: {state}", DebugType.Data, this);
    }

    public void StartBattle()
    {
        _isBattle = true;
    }

    public void ClearBattle()
    {
        if (!_isBattle)
        {
            return;
        }

        ApplyClearHeal();
        EndBattle();
    }

    public void EndBattle()
    {
        _isBattle = false;
        if(_teamUpgradeData != null)
        {
            _teamUpgradeData.ClearOneTimeBattleUpgrades();
        }
    }

    public float ApplyBattleStat(float baseValue, TeamUpgradeStatKey statKey, bool isCooldownStat = false)
    {
        if (!_isBattle)
        {
            return baseValue;
        }
        
        if(_teamUpgradeData ==null || _teamUpgradeProvider == null)
        {
            return baseValue;
        }

        float addValue = 0f;
        float percentValue = 0f;

        List<TeamUpgradeEntry> upgrades = _teamUpgradeProvider.GetAllUpgrades();
        
        foreach(TeamUpgradeEntry entry in upgrades)
        {
            if(entry ==null)
            {
                continue;
            }
            if(entry.StatKey != statKey)
            {
                continue;
            }
            int level = _teamUpgradeData.GetLevel(entry);

            if(level <= 0)
            {
                continue;
            }

            float value = entry.ValuePerLevel;

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

        if(isCooldownStat)
        {
            result *= 1f - percentValue;
            return  Mathf.Max(0.1f, result);
        }
        result *= 1f + percentValue;
        return result;

    }

    // 모든 플레이어 힐 적용
    private void ApplyClearHeal()
    {
        float healPercent = GetClearHealPercent();

        if (healPercent <= 0f)
        {
            DebugTool.Log("[TeamBattleUpgradeEffect] ClearHeal 레벨이 없어 회복하지 않습니다.", DebugType.Data, this);
            return;
        }

        PlayerHealth[] playerHealths = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);

        foreach (PlayerHealth playerHealth in playerHealths)
        {
            if (playerHealth == null)
            {
                continue;
            }

            float healAmount = playerHealth.MaxHealth * healPercent;
            playerHealth.Heal(healAmount);
        }

        DebugTool.Log($"[TeamBattleUpgradeEffect] ClearHeal 적용 완료 / 회복 비율: {healPercent * 100f}% / 대상 수: {playerHealths.Length}", DebugType.Data, this
        );
    }

    // ClearHeal 총 회복 비율을 계산한다.
    private float GetClearHealPercent()
    {
        if (_teamUpgradeData == null || _teamUpgradeProvider == null)
        {
            return 0f;
        }

        float healPercent = 0f;
        List<TeamUpgradeEntry> upgrades = _teamUpgradeProvider.GetAllUpgrades();

        foreach (TeamUpgradeEntry entry in upgrades)
        {
            if (entry == null)
            {
                continue;
            }

            if (entry.StatKey != TeamUpgradeStatKey.ClearHeal)
            {
                continue;
            }

            int level = _teamUpgradeData.GetLevel(entry);

            if (level <= 0)
            {
                continue;
            }

            healPercent += entry.ValuePerLevel;
        }

        return healPercent;
    }
}
