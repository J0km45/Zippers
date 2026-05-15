using System.Collections.Generic;
using UnityEngine;
using Zippers.Network;
using Zippers.Network.Contracts;

public class TeamBattleUpgradeEffect : MonoBehaviour
{
    [SerializeField] private TeamUpgradeData _teamUpgradeData;
    [SerializeField] private TeamUpgradeProvider _teamUpgradeProvider;

    [SerializeField] private bool _isBattle;

    public bool IsBattle => _isBattle;

    // ── IBattleStateBus 구독 캐시 ─────────────────────────────────
    // Step 7 변경: 옛 MapData.NetworkMapData.NodeState 직접 구독 제거.
    //   - TeamBattleNetState (IBattleStateBus 구현체) 의 이벤트를 통해 정규화된 신호 수신.
    //   - 호스트/클라 모두 같은 이벤트 발화 시점에 받음.
    private IBattleStateBus _battleBus;
    private bool _busBound;

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
    }

    private void OnDisable()
    {
        UnbindBus();
    }

    private void OnDestroy()
    {
        UnbindBus();
    }

    private void Update()
    {
        // TeamBattleNetState 가 NGO 로 늦게 spawn 될 수 있어 매 프레임 idempotent 시도.
        // 한 번 연결되면 skip.
        TryBindBus();
    }

    private void TryBindBus()
    {
        if (_busBound) return;

        // Singleton 직접 사용 (인터페이스 캐스팅으로 의존성은 IBattleStateBus 만)
        TeamBattleNetState net = TeamBattleNetState.Instance;
        if (net == null) return;

        _battleBus = net;
        _battleBus.OnBattleStarted += StartBattle;
        _battleBus.OnBattleCleared += ClearBattle;
        _battleBus.OnBattleEnded += EndBattle;
        _busBound = true;

        // 초기 상태 1회 반영 — 이미 Battle 상태에서 시작했을 수 있음
        if (_battleBus.IsBattle && !_isBattle)
        {
            StartBattle();
        }

        DebugTool.Log(
            $"[TeamBattleUpgradeEffect] IBattleStateBus 구독 시작 (초기 IsBattle={_battleBus.IsBattle})",
            DebugType.Data, this);
    }

    private void UnbindBus()
    {
        if (!_busBound) return;
        if (_battleBus != null)
        {
            _battleBus.OnBattleStarted -= StartBattle;
            _battleBus.OnBattleCleared -= ClearBattle;
            _battleBus.OnBattleEnded -= EndBattle;
        }
        _battleBus = null;
        _busBound = false;
    }

    // ── 전투 상태 핸들러 (IBattleStateBus 이벤트 또는 디버그 키 입력으로 호출) ──
    // public 유지: TeamUpgradeDebugTest 가 키보드 입력으로 직접 호출 중.

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
