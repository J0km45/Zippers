using System;
using UnityEngine;
using Zippers.Network;
using Zippers.Network.Contracts;

/// <summary>
/// 플레이어의 최종 스탯을 합산해 제공한다.
/// 입력: PlayerClassDataSO(기본 스탯) + PlayerIngameData(개인 업그레이드) +
///       TeamUpgradeCalculator(영구 팀 업그레이드) + TeamBattleUpgradeEffect(전투 한정 효과)
/// 출력: IPlayerStatProvider 의 Total* 프로퍼티들 — 전투 라인(B) 이 인터페이스로 read.
///
/// OnStatsRecalculated 는 다음 시점에 발화:
///   - 개인 업그레이드 변경 (PlayerIngameData.UpgradeChange)
///   - 팀 업그레이드 변경 (TeamEconomyNetState.OnTeamUpgradeChanged)
///   - 전투 상태 변경 (TeamBattleNetState.OnBattleStarted/Cleared/Ended) — Battle* 일회성 효과 토글
/// </summary>
public class PlayerStats : MonoBehaviour, IPlayerStatProvider
{
    public event Action OnStatsRecalculated;

    [Header("클래스 데이터")]
    [SerializeField] private PlayerClassDataSO _playerClassData;

    [SerializeField] private PlayerIngameData _playerIngameData;

    [SerializeField] private TeamUpgradeCalculator _teamUpgradeCalculator;
    [SerializeField] TeamBattleUpgradeEffect _teamBattleUpgradeEffect;

    private ClassUpgradeData _classUpgradeData;
    public PlayerClassDataSO PlayerClassData => _playerClassData;

    public int ClassID => _playerClassData.ClassId;
    public int ClassId => _playerClassData.ClassId;    // IPlayerStatProvider 시그니처 별칭
    public WeaponType WeaponType => _playerClassData.WeaponType;
    public string ClassName => _playerClassData.ClassName;
    //-------------------------------------ㅅ--------------------
    //기본스탯
    //________________________________________________________________
    public float MaxHealth => _playerClassData.MaxHealth;
    public float Stamina => _playerClassData.MaxStamina;
    public float StaminaDelay => _playerClassData.StaminaDelay;
    public float StaminaConsume => _playerClassData.StaminaConsume;
    public float StaminaPeriod => _playerClassData.StaminaPeriod;
    public float StaminaRegen => _playerClassData.StaminaRegen;
    //---------------------------------------------------------
    public float MinDamage => _playerClassData.MinDamage;
    public float MaxDamage => _playerClassData.MaxDamage;

    public float AttackSpeed => _playerClassData.AttackSpeed;
    public float MagazineCapacity => _playerClassData.MagazineCapacity;
    public float ReloadTime => _playerClassData.ReloadTime;

    public float BulletSpeed => _playerClassData.BulletSpeed;
    public float BulletDistance => _playerClassData.BulletDistance;

    public float MoveSpeed => _playerClassData.MoveSpeed;
    public float SprintSpeed => _playerClassData.SprintSpeed;

    public float SightRange => _playerClassData.SightRange;
    public float CollectRange => _playerClassData.CollectRange;
    //------------------------------------------------------------------
    //추가 스탯
    //-----------------------------------------------------------

    public float AddMaxHealth => GetBonus(CommonUpgrade("MaxHealth"));
    public float AddStamina => GetBonus(CommonUpgrade("Stamina"));
    public float AddStaminaRegen => GetBonus(CommonUpgrade("StaminaRegen"));

    public float AddDamage => GetBonus(CommonUpgrade("Damage"));
    public float AddAttackSpeed => GetBonus(CommonUpgrade("AttackSpeed"));
    public float AddMoveSpeed => GetBonus(CommonUpgrade("MoveSpeed"));

    public float AddMagazineCapacity => GetBonus(ClassUpgrade("MagazineCapacity"));
    public float AddReloadTime => GetBonus(ClassUpgrade("ReloadTime"));
    public float AddBulletDistance => GetBonus(ClassUpgrade("BulletDistance"));
    public float AddSprintSpeed => GetBonus(ClassUpgrade("SprintSpeed"));
    public float AddSightRange => GetBonus(ClassUpgrade("SightRange"));
    public float AddCollectRange => GetBonus(ClassUpgrade("CollectRange"));

    // 아직 실제 기능이 구현되지 않은 업그레이드 틀
    public float AddDamageReduction => GetBonus(ClassUpgrade("DamageReduction"));
    public float AddPierceCount => GetBonus(ClassUpgrade("PierceCount"));
    public float AddKnockbackPower => GetBonus(ClassUpgrade("KnockbackPower"));
    public float AddProjectileCount => GetBonus(ClassUpgrade("ProjectileCount"));

    // ─────────────────────────────────────
    // 총 스탯
    // 계산식: 기본 스탯 + 추가 스탯
    // ─────────────────────────────────────

    public float TotalMaxHealth => ApplyTeamUpgrade(MaxHealth + AddMaxHealth, TeamUpgradeStatKey.MaxHealth);
    public float TotalStamina => ApplyTeamUpgrade(Stamina + AddStamina, TeamUpgradeStatKey.Stamina);
    public float TotalMaxStamina => TotalStamina;    // IPlayerStatProvider 시그니처 별칭
    public float TotalStaminaRegen => StaminaRegen + AddStaminaRegen;

    public float TotalMinDamage => ApplyBattleUpgrade(ApplyTeamUpgrade(MinDamage + AddDamage, TeamUpgradeStatKey.Damage), TeamUpgradeStatKey.BattleDamage);
    public float TotalMaxDamage => ApplyBattleUpgrade(ApplyTeamUpgrade(MaxDamage + AddDamage, TeamUpgradeStatKey.Damage), TeamUpgradeStatKey.BattleDamage);

    public float TotalAttackSpeed => ApplyBattleUpgrade(ApplyTeamUpgrade(AttackSpeed + AddAttackSpeed, TeamUpgradeStatKey.AttackSpeed, true), TeamUpgradeStatKey.BattleAttackSpeed,true);
    public float TotalMagazineCapacity => MagazineCapacity + AddMagazineCapacity;
    public float TotalReloadTime => ReloadTime + AddReloadTime;

    public float TotalBulletSpeed => BulletSpeed;
    public float TotalBulletDistance => BulletDistance + AddBulletDistance;

    public float TotalMoveSpeed => ApplyBattleUpgrade(ApplyTeamUpgrade(MoveSpeed + AddMoveSpeed, TeamUpgradeStatKey.MoveSpeed), TeamUpgradeStatKey.BattleMoveSpeed);
    public float TotalSprintSpeed => SprintSpeed + AddSprintSpeed;

    /// <summary>
    /// 달리기 최종 이동 속도.
    /// 기획식: 총 이동속도 + (기본 이동속도 * 달리기 속도)
    /// </summary>
    public float TotalSprintMoveSpeed => TotalMoveSpeed + (MoveSpeed * TotalSprintSpeed);

    public float TotalSightRange => SightRange + AddSightRange;
    public float TotalCollectRange => CollectRange + AddCollectRange;

    // 아직 실제 기능에 연결하지 않은 총 수치
    public float TotalDamageReduction => AddDamageReduction;
    public float TotalPierceCount => AddPierceCount;
    public float TotalKnockbackPower => AddKnockbackPower;
    public float TotalProjectileCount => AddProjectileCount;

    public bool UseBullet => 
        WeaponType == WeaponType.Rifle || 
        WeaponType == WeaponType.Shotgun || 
        WeaponType == WeaponType.Pistol;

    private void Awake()
    {
        if (_playerIngameData == null)
        {
            _playerIngameData = GetComponent<PlayerIngameData>();
        }
        if(_teamUpgradeCalculator == null)
        {
            _teamUpgradeCalculator = FindFirstObjectByType<TeamUpgradeCalculator>();
        }
        if(_teamBattleUpgradeEffect == null)
        {
            _teamBattleUpgradeEffect = FindFirstObjectByType<TeamBattleUpgradeEffect>();
        }
    }
    private void Start()
    {
        LoadUpgradeData();
    }
    private bool _netEventsBound;
    private TeamEconomyNetState _boundTeamEconomy;
    private TeamBattleNetState _boundTeamBattle;

    private void OnEnable()
    {
        if(_playerIngameData != null)
        {
            _playerIngameData.UpgradeChange += OnUpgradeChange;
        }
    }

    private void OnDisable()
    {
        if (_playerIngameData != null)
        {
            _playerIngameData.UpgradeChange -= OnUpgradeChange;
        }
        UnbindNetEvents();
    }

    private void Update()
    {
        // NetworkBehaviour spawn 시점이 늦을 수 있어 매 프레임 idempotent 시도. 한 번 연결되면 skip.
        TryBindNetEvents();
    }

    private void TryBindNetEvents()
    {
        if (_netEventsBound) return;

        TeamEconomyNetState te = TeamEconomyNetState.Instance;
        TeamBattleNetState tb = TeamBattleNetState.Instance;
        if (te == null || tb == null) return;

        te.OnTeamUpgradeChanged += OnTeamUpgradeChanged;
        tb.OnBattleStarted += OnBattleStateChangedAny;
        tb.OnBattleCleared += OnBattleStateChangedAny;
        tb.OnBattleEnded += OnBattleStateChangedAny;

        _boundTeamEconomy = te;
        _boundTeamBattle = tb;
        _netEventsBound = true;
    }

    private void UnbindNetEvents()
    {
        if (!_netEventsBound) return;
        if (_boundTeamEconomy != null)
        {
            _boundTeamEconomy.OnTeamUpgradeChanged -= OnTeamUpgradeChanged;
        }
        if (_boundTeamBattle != null)
        {
            _boundTeamBattle.OnBattleStarted -= OnBattleStateChangedAny;
            _boundTeamBattle.OnBattleCleared -= OnBattleStateChangedAny;
            _boundTeamBattle.OnBattleEnded -= OnBattleStateChangedAny;
        }
        _boundTeamEconomy = null;
        _boundTeamBattle = null;
        _netEventsBound = false;
    }

    private void OnTeamUpgradeChanged(int upgradeId, int level)
    {
        OnStatsRecalculated?.Invoke();
        DebugTool.Log($"OnStatsRecalculated (teamUpgrade id={upgradeId}, level={level})", DebugType.Data, this);
    }

    private void OnBattleStateChangedAny()
    {
        // Battle* 일회성 효과(BattleDamage/BattleMoveSpeed/BattleAttackSpeed/ClearHeal) 토글 → 재계산
        OnStatsRecalculated?.Invoke();
        DebugTool.Log("OnStatsRecalculated (battleState 변경)", DebugType.Data, this);
    }

    private void LoadUpgradeData()
    {
        if(_playerClassData ==null)
        {
            return;
        }

        if(LocalDataAccess.Instance == null)
        {
            return;
        }

        _classUpgradeData = LocalDataAccess.Instance.Game.GetUpgrade(WeaponType);

        if(_classUpgradeData == null )
        {
            return;
        }
    }

    private void OnUpgradeChange(UpgradeEntry entry, int level)
    {
        OnStatsRecalculated?.Invoke();
        DebugTool.Log($"OnStatsRecalculated (personalUpgrade id={entry?.Id}, level={level})", DebugType.Data, this);
    }

    private void CheckUpgradeData()
    {
        if(_classUpgradeData != null)
        {
            return;
        }
        LoadUpgradeData();
    }
    private UpgradeEntry CommonUpgrade(string statKey)
    {
        CheckUpgradeData();

        if(_classUpgradeData ==null)
        {
            return null;
        }

        switch(statKey)
        {
            case "MaxHealth":
                return _classUpgradeData.MaxHealth;
            case "Stamina":
                return _classUpgradeData.Stamina;
            case "StaminaRegen":
                return _classUpgradeData.StaminaRegen;
            case "Damage":
                return _classUpgradeData.Damage;
            case "AttackSpeed":
                return _classUpgradeData.AttackSpeed;
            case "MoveSpeed":
                return _classUpgradeData.MoveSpeed;
            default:
                return null;
        }
    }

    private UpgradeEntry ClassUpgrade(string statKey)
    {
        CheckUpgradeData();

        if(_classUpgradeData ==null)
        {
            return null;
        }
        switch(_classUpgradeData)
        {
            case MeleeUpgradeData melee:
                return statKey switch
                {
                    "DamageReduction" => melee.DamageReduction,
                    "SprintSpeed" => melee.SprintSpeed,
                    _ => null
                };
            case RifleUpgradeData rifle:
                return statKey switch
                {
                    "MagazineCapacity" => rifle.MagazineCapacity,
                    "ReloadTime" => rifle.ReloadTime,
                    "PierceCount" => rifle.PierceCount,
                    "BulletDistance" => rifle.BulletDistance,
                    _ => null
                };
            case ShotgunUpgradeData shotgun:
                return statKey switch
                {
                    "MagazineCapacity" => shotgun.MagazineCapacity,
                    "ReloadTime" => shotgun.ReloadTime,
                    "KnockbackPower" => shotgun.KnockbackPower,
                    "ProjectileCount" => shotgun.ProjectileCount,
                    _ => null
                };
            case PistolUpgradeData pistol:
                return statKey switch
                {
                    "MagazineCapacity" => pistol.MagazineCapacity,
                    "ReloadTime" => pistol.ReloadTime,
                    "SightRange" => pistol.SightRange,
                    "CollectRange" => pistol.CollectRange,
                    _ => null
                };
            default:
                return null;
        }
    }
    private float GetBonus(UpgradeEntry entry)
    {
        if(_playerIngameData ==null || entry ==null)
        {
            return 0f;
        }
        return _playerIngameData.GetValue(entry);
    }

    private float ApplyTeamUpgrade(float baseValue, TeamUpgradeStatKey statKey, bool isCooldownStat = false)
    {
        if(_teamUpgradeCalculator == null)
        {
            return baseValue;
        }
        return _teamUpgradeCalculator.Apply(baseValue, statKey, isCooldownStat);
    }

    private float ApplyBattleUpgrade(float baseValue, TeamUpgradeStatKey statKey, bool isCooldownStat = false)
    {
        if(_teamBattleUpgradeEffect == null)
        {
            return baseValue;
        }
 
        return _teamBattleUpgradeEffect.ApplyBattleStat(baseValue, statKey, isCooldownStat);
    }
    public float GetRandomDamage()
    {
        return UnityEngine.Random.Range(TotalMinDamage, TotalMaxDamage);
    }
}


