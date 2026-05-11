using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// New Input System의 Keyboard.current 방식으로 업그레이드를 테스트하는 스크립트.
/// U 키를 누르면 업그레이드가 1개씩 적용된다.
/// I 키를 누르면 현재 스탯을 출력한다.
/// 테스트가 끝나면 삭제해도 된다.
/// </summary>
public class PlayerUpgradeKeyInputTest : MonoBehaviour
{
    [Header("플레이어 참조")]
    [SerializeField] private PlayerStats _playerStats;
    [SerializeField] private PlayerIngameData _playerIngameData;
    [SerializeField] private PlayerUpgradeProvider _playerUpgradeProvider;
    [SerializeField] private PlayerHealth _playerHealth;
    [SerializeField] private PlayerStamina _playerStamina;
    [SerializeField] private PlayerReload _playerReload;

    [Header("재화 테스트")]
    [SerializeField] private PlayerResourceCollector _playerResourceCollector;
    [SerializeField] private float _testResourceAmount = 500f;

    private List<UpgradeEntry> _upgradeList = new();
    private int _upgradeIndex = 0;

    private void Awake()
    {
        FindComponents();
    }

    private void Start()
    {
        LoadUpgradeList();
        PrintStats();
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.uKey.wasPressedThisFrame)
        {
            Debug.Log("[PlayerUpgradeKeyInputTest] U 입력 감지");
            UpgradeCurrentIndex();
        }

        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            Debug.Log("[PlayerUpgradeKeyInputTest] I 입력 감지");
            PrintStats();
        }
        if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            AddTestResources();
        }
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            Debug.Log("[PlayerUpgradeKeyInputTest] t 입력 감지 / 랜덤 업그레이드 목록 재생성");
            LoadUpgradeList();
        }
    }

    /// <summary>
    /// 같은 오브젝트에 있는 필요한 컴포넌트를 자동으로 찾는다.
    /// </summary>
    private void FindComponents()
    {
        if (_playerResourceCollector == null)
        {
            _playerResourceCollector = GetComponent<PlayerResourceCollector>();
        }

        if (_playerStats == null)
        {
            _playerStats = GetComponent<PlayerStats>();
        }

        if (_playerIngameData == null)
        {
            _playerIngameData = GetComponent<PlayerIngameData>();
        }

        if (_playerUpgradeProvider == null)
        {
            _playerUpgradeProvider = GetComponent<PlayerUpgradeProvider>();
        }

        if (_playerHealth == null)
        {
            _playerHealth = GetComponent<PlayerHealth>();
        }

        if (_playerStamina == null)
        {
            _playerStamina = GetComponent<PlayerStamina>();
        }

        if (_playerReload == null)
        {
            _playerReload = GetComponent<PlayerReload>();
        }

    }

    /// <summary>
    /// 현재 플레이어 클래스 기준으로 사용 가능한 업그레이드 목록을 불러온다.
    /// </summary>
    private void LoadUpgradeList()
    {
        if (_playerStats == null)
        {
            Debug.LogWarning("[PlayerUpgradeKeyInputTest] PlayerStats가 없습니다.");
            return;
        }

        if (_playerUpgradeProvider == null)
        {
            Debug.LogWarning("[PlayerUpgradeKeyInputTest] PlayerUpgradeProvider가 없습니다.");
            return;
        }

        _upgradeList = _playerUpgradeProvider.RandomUpgrade(
            _playerStats.WeaponType,
            _playerIngameData,
            4
        );
        Debug.Log($"[PlayerUpgradeKeyInputTest] 클래스: {_playerStats.WeaponType}");
        Debug.Log($"[PlayerUpgradeKeyInputTest] 테스트 가능 업그레이드 수: {_upgradeList.Count}");

        for (int i = 0; i < _upgradeList.Count; i++)
        {
            UpgradeEntry entry = _upgradeList[i];

            Debug.Log(
                $"[PlayerUpgradeKeyInputTest] Index:{i} / " +
                $"ID:{entry.Id} / " +
                $"Name:{entry.UpgradeName} / " +
                $"StatKey:{entry.StatKey} / " +
                $"Value:{entry.ValuePerLevel}"
            );
        }
    }

    /// <summary>
    /// 현재 인덱스의 업그레이드를 1레벨 증가시킨다.
    /// U 키를 누를 때마다 다음 업그레이드로 넘어간다.
    /// </summary>
    private void UpgradeCurrentIndex()
    {
        if (_playerIngameData == null)
        {
            Debug.LogWarning("[PlayerUpgradeKeyInputTest] PlayerIngameData가 없습니다.");
            return;
        }

        if (_upgradeList == null || _upgradeList.Count == 0)
        {
            Debug.LogWarning("[PlayerUpgradeKeyInputTest] 업그레이드 목록이 비어있습니다.");
            return;
        }

        if (_upgradeIndex >= _upgradeList.Count)
        {
            _upgradeIndex = 0;
        }

        UpgradeEntry entry = _upgradeList[_upgradeIndex];

        int beforeLevel = _playerIngameData.GetLevel(entry);
        int cost = _playerIngameData.UpgradeCost(entry);

        bool isSuccess = _playerIngameData.Upgrade(entry);

        int afterLevel = _playerIngameData.GetLevel(entry);

        Debug.Log(
            $"[PlayerUpgradeKeyInputTest] 업그레이드 테스트 / " +
            $"Index:{_upgradeIndex} / " +
            $"Name:{entry.UpgradeName} / " +
            $"Cost:{cost} / " +
            $"Level:{beforeLevel} -> {afterLevel} / " +
            $"Success:{isSuccess}"
        );

        if (isSuccess)
        {
            RefreshRuntimeStats(entry);
            PrintStats();
        }

        _upgradeIndex++;
    }
    /// <summary>
    /// 테스트용 재화를 지급한다.
    /// K 키를 누르면 모든 재화를 일정량 증가시킨다.
    /// </summary>
    private void AddTestResources()
    {
        if (_playerResourceCollector == null)
        {
            Debug.LogWarning("[PlayerUpgradeKeyInputTest] PlayerResourceCollector가 없습니다.");
            return;
        }

        _playerResourceCollector.CollectResource(ResourcesType.Scrap, _testResourceAmount);
        _playerResourceCollector.CollectResource(ResourcesType.Supplies, _testResourceAmount);
        _playerResourceCollector.CollectResource(ResourcesType.InfectionSample, _testResourceAmount);

        Debug.Log(
            $"[PlayerUpgradeKeyInputTest] 테스트 재화 지급 완료 / " +
            $"Scrap +{_testResourceAmount}, " +
            $"Supplies +{_testResourceAmount}, " +
            $"InfectionSample +{_testResourceAmount}"
        );

        Debug.Log(
            $"[PlayerUpgradeKeyInputTest] 현재 재화 / " +
            $"Scrap: {_playerResourceCollector.Scrap}, " +
            $"Supplies: {_playerResourceCollector.Supplies}, " +
            $"InfectionSample: {_playerResourceCollector.InfectionSample}"
        );
    }

    /// <summary>
    /// 업그레이드 종류에 따라 현재값을 가진 시스템을 갱신한다.
    /// </summary>
    private void RefreshRuntimeStats(UpgradeEntry entry)
    {
        switch (entry.StatKey)
        {
            case "MaxHealth":
                _playerHealth?.RefreshHealth();
                break;

            case "Stamina":
                _playerStamina?.RefreshMaxStamina();
                break;

            case "MagazineCapacity":
                _playerReload?.RefreshMaxBullet();
                break;
        }
    }

    /// <summary>
    /// 현재 PlayerStats 계산 결과를 출력한다.
    /// </summary>
    private void PrintStats()
    {
        if (_playerStats == null)
        {
            Debug.LogWarning("[PlayerUpgradeKeyInputTest] PlayerStats가 없습니다.");
            return;
        }

        Debug.Log($"[Stats] 체력: {_playerStats.MaxHealth} + {_playerStats.AddMaxHealth} = {_playerStats.TotalMaxHealth}");
        Debug.Log($"[Stats] 스테미나: {_playerStats.Stamina} + {_playerStats.AddStamina} = {_playerStats.TotalStamina}");
        Debug.Log($"[Stats] 스테미나 회복량: {_playerStats.StaminaRegen} + {_playerStats.AddStaminaRegen} = {_playerStats.TotalStaminaRegen}");

        Debug.Log($"[Stats] 데미지: {_playerStats.MinDamage}~{_playerStats.MaxDamage} + {_playerStats.AddDamage} = {_playerStats.TotalMinDamage}~{_playerStats.TotalMaxDamage}");
        Debug.Log($"[Stats] 공격속도: {_playerStats.AttackSpeed} + {_playerStats.AddAttackSpeed} = {_playerStats.TotalAttackSpeed}");

        Debug.Log($"[Stats] 이동속도: {_playerStats.MoveSpeed} + {_playerStats.AddMoveSpeed} = {_playerStats.TotalMoveSpeed}");
        Debug.Log($"[Stats] 달리기 이동속도: {_playerStats.TotalSprintMoveSpeed}");

        Debug.Log($"[Stats] 장탄수: {_playerStats.MagazineCapacity} + {_playerStats.AddMagazineCapacity} = {_playerStats.TotalMagazineCapacity}");
        Debug.Log($"[Stats] 재장전 시간: {_playerStats.ReloadTime} + {_playerStats.AddReloadTime} = {_playerStats.TotalReloadTime}");

        Debug.Log($"[Stats] 탄환 거리: {_playerStats.BulletDistance} + {_playerStats.AddBulletDistance} = {_playerStats.TotalBulletDistance}");
        Debug.Log($"[Stats] 시야 범위: {_playerStats.SightRange} + {_playerStats.AddSightRange} = {_playerStats.TotalSightRange}");
        Debug.Log($"[Stats] 재화 획득 범위: {_playerStats.CollectRange} + {_playerStats.AddCollectRange} = {_playerStats.TotalCollectRange}");
    }
}