using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TeamUpgradeDebugTest : MonoBehaviour
{
    [Header("팀 업그레이드 참조")]
    [SerializeField] private TeamUpgradeProvider _teamUpgradeProvider;
    [SerializeField] private TeamUpgradeData _teamUpgradeData;
    [SerializeField] private TeamResourceManager _teamResourceManager;

    [Header("플레이어 확인용")]
    [SerializeField] private PlayerStats _playerStats;
    [SerializeField] private PlayerResourceCollector _playerResourceCollector;

    [Header("테스트 설정")]
    [SerializeField] private float _testSuppliesAmount = 100f;

    [SerializeField] private TeamBattleUpgradeEffect _teamBattleUpgradeEffect;

    private List<TeamUpgradeEntry> _entries = new();
    private int _currentIndex = 0;

    private void Awake()
    {
        if (_teamUpgradeProvider == null)
        {
            _teamUpgradeProvider = FindFirstObjectByType<TeamUpgradeProvider>();
        }

        if (_teamUpgradeData == null)
        {
            _teamUpgradeData = FindFirstObjectByType<TeamUpgradeData>();
        }

        if (_teamResourceManager == null)
        {
            _teamResourceManager = FindFirstObjectByType<TeamResourceManager>();
        }

        if (_playerStats == null)
        {
            _playerStats = GetComponent<PlayerStats>();
        }

        if (_playerResourceCollector == null)
        {
            _playerResourceCollector = GetComponent<PlayerResourceCollector>();
        }
        if(_teamBattleUpgradeEffect == null)
        {
            _teamBattleUpgradeEffect = FindFirstObjectByType<TeamBattleUpgradeEffect>();
        }
    }

    private void Start()
    {
        LoadTeamUpgradeList();
        PrintCurrentStats();
        PrintTeamSupplies();
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.yKey.wasPressedThisFrame)
        {
            Debug.Log("[TeamUpgradeDebugTest] Y 키 입력 감지");
            UpgradeCurrentIndex();
        }

        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            LoadTeamUpgradeList();
        }

        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            Debug.Log("[TeamUpgradeDebugTest] L 키 입력 감지");
            AddTestSupplies();
        }

        if (Keyboard.current.oKey.wasPressedThisFrame)
        {
            Debug.Log("[TeamUpgradeDebugTest] O 키 입력 감지");
            PrintCurrentStats();
        }

        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            Debug.Log("[TeamUpgradeDebugTest] P 키 입력 감지");
            PrintTeamSupplies();
        }
        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            StartBattleTest();
        }

        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            ClearBattleTest();
        }

        if (Keyboard.current.nKey.wasPressedThisFrame)
        {
            EndBattleTest();
        }

    }
    /// <summary>
    /// 전투 시작 테스트.
    /// </summary>
    private void StartBattleTest()
    {
        if (_teamBattleUpgradeEffect == null)
        {
            return;
        }

        _teamBattleUpgradeEffect.StartBattle();

        Debug.Log("[TeamUpgradeDebugTest] B 입력 / 전투 시작");
        PrintCurrentStats();
    }

    /// <summary>
    /// 전투 클리어 테스트.
    /// </summary>
    private void ClearBattleTest()
    {
        if (_teamBattleUpgradeEffect == null)
        {
            return;
        }

        _teamBattleUpgradeEffect.ClearBattle();

        Debug.Log("[TeamUpgradeDebugTest] C 입력 / 전투 클리어");
        PrintCurrentStats();
    }

    /// <summary>
    /// 전투 종료 테스트.
    /// </summary>
    private void EndBattleTest()
    {
        if (_teamBattleUpgradeEffect == null)
        {
            return;
        }

        _teamBattleUpgradeEffect.EndBattle();

        Debug.Log("[TeamUpgradeDebugTest] N 입력 / 전투 종료");
        PrintCurrentStats();
    }

    private void LoadTeamUpgradeList()
    {
        if (_teamUpgradeProvider == null)
        {
            return;
        }

        _entries = _teamUpgradeProvider.GetAllUpgrades();

        DebugTool.Log($"[TeamUpgradeDebugTest] 팀 업그레이드 전체 개수: {_entries.Count}", DebugType.Data, this);

        for (int i = 0; i < _entries.Count; i++)
        {
            TeamUpgradeEntry entry = _entries[i];

            if (entry == null)
            {
                continue;
            }

            int level = _teamUpgradeData != null ? _teamUpgradeData.GetLevel(entry) : 0;
            int cost = _teamUpgradeData != null ? _teamUpgradeData.UpgradeCost(entry) : 0;

            DebugTool.Log(
                $"[TeamUpgradeDebugTest] Index:{i} / " +
                $"ID:{entry.UpgradeId} / " +
                $"Name:{entry.UpgradeName} / " +
                $"StatKey:{entry.StatKey} / " +
                $"ApplyType:{entry.ApplyType} / " +
                $"ValuePerLevel:{entry.ValuePerLevel} / " +
                $"Level:{level}/{entry.MaxLevel} / " +
                $"Cost:{cost}",
                DebugType.Data,
                this
            );
        }
    }

    private void AddTestSupplies()
    {
        if (_teamResourceManager == null)
        {
            Debug.LogWarning("[TeamUpgradeDebugTest] TeamResourceManager가 없습니다.");
            return;
        }

        _teamResourceManager.AddResource(ResourcesType.Supplies, _testSuppliesAmount);

        Debug.Log(
            $"[TeamUpgradeDebugTest] 테스트 Supplies 지급 / " +
            $"+{_testSuppliesAmount} / 현재 Supplies: {_teamResourceManager.Supplies}"
        );
    }

    private void UpgradeCurrentIndex()
    {
        if (_teamUpgradeData == null)
        {
            Debug.LogWarning("[TeamUpgradeDebugTest] TeamUpgradeData가 없습니다.");
            return;
        }

        if (_entries == null || _entries.Count == 0)
        {
            Debug.LogWarning("[TeamUpgradeDebugTest] 팀 업그레이드 목록이 비어있습니다. H 키로 목록을 먼저 불러오세요.");
            return;
        }

        if (_currentIndex >= _entries.Count)
        {
            _currentIndex = 0;
        }

        TeamUpgradeEntry entry = _entries[_currentIndex];

        int beforeLevel = _teamUpgradeData.GetLevel(entry);
        int cost = _teamUpgradeData.UpgradeCost(entry);
        float beforeSupplies = _teamResourceManager != null ? _teamResourceManager.Supplies : 0f;

        bool isSuccess = _teamUpgradeData.Upgrade(entry);

        int afterLevel = _teamUpgradeData.GetLevel(entry);
        float afterSupplies = _teamResourceManager != null ? _teamResourceManager.Supplies : 0f;

        Debug.Log(
            $"[TeamUpgradeDebugTest] 팀 업그레이드 구매 테스트 / " +
            $"Index:{_currentIndex} / " +
            $"Name:{entry.UpgradeName} / " +
            $"Cost:{cost} / " +
            $"Level:{beforeLevel} -> {afterLevel} / " +
            $"Supplies:{beforeSupplies} -> {afterSupplies} / " +
            $"Success:{isSuccess}"
        );

        _currentIndex++;

        PrintCurrentStats();
    }

    private void PrintCurrentStats()
    {
        if (_playerStats == null)
        {
            Debug.LogWarning("[TeamUpgradeDebugTest] PlayerStats가 없습니다.");
            return;
        }

        Debug.Log(
            "[TeamUpgradeDebugTest] 현재 PlayerStats\n" +
            $"MaxHealth: {_playerStats.TotalMaxHealth}\n" +
            $"Stamina: {_playerStats.TotalStamina}\n" +
            $"Damage: {_playerStats.TotalMinDamage} ~ {_playerStats.TotalMaxDamage}\n" +
            $"AttackSpeed: {_playerStats.TotalAttackSpeed}\n" +
            $"MoveSpeed: {_playerStats.TotalMoveSpeed}"
        );
    }

    private void PrintTeamSupplies()
    {
        float teamSupplies = _teamResourceManager != null ? _teamResourceManager.Supplies : 0f;
        float playerSupplies = _playerResourceCollector != null ? _playerResourceCollector.Supplies : 0f;

        Debug.Log(
            "[TeamUpgradeDebugTest] Supplies 확인\n" +
            $"TeamResourceManager Supplies: {teamSupplies}\n" +
            $"PlayerResourceCollector Supplies 조회값: {playerSupplies}"
        );
    }
}