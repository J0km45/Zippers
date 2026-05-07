/// <summary>
/// 클래스별 업그레이드 데이터의 베이스 클래스.
/// 공통 6개 stat (MaxHealth/Stamina/StaminaRegen/Damage/AttackSpeed/MoveSpeed)을 보유.
/// 파생 클래스(MeleeUpgradeData / RifleUpgradeData / ShotgunUpgradeData / PistolUpgradeData)가
/// 클래스별 고유 stat을 추가한다.
///
/// 사용 예:
///   ClassUpgradeData up = LocalDataAccess.Instance.Game.GetUpgrade("Melee");
///   Debug.Log(up.MaxHealth.ValuePerLevel);
///   // Melee 전용 stat은 캐스팅 후 접근:
///   var melee = (MeleeUpgradeData)up;
///   Debug.Log(melee.DamageReduction.ValuePerLevel);
/// </summary>
public abstract class ClassUpgradeData
{
    public WeaponType ClassType;

    // ─── 공통 6개 stat ────────────────────────────────────
    public UpgradeEntry MaxHealth;
    public UpgradeEntry Stamina;
    public UpgradeEntry StaminaRegen;
    public UpgradeEntry Damage;
    public UpgradeEntry AttackSpeed;
    public UpgradeEntry MoveSpeed;

    /// <summary>StatKey 문자열에 해당하는 필드에 entry를 할당한다.</summary>
    public void AssignByStatKey(string key, UpgradeEntry entry)
    {
        if (TryAssignCommon(key, entry)) return;
        if (TryAssignClassSpecific(key, entry)) return;
        DebugTool.Warning(
            $"[ClassUpgradeData] {ClassType}에 알 수 없는 StatKey: '{key}'",
            DebugType.Data);
    }

    private bool TryAssignCommon(string key, UpgradeEntry entry)
    {
        switch (key)
        {
            case "MaxHealth":    MaxHealth = entry; return true;
            case "Stamina":      Stamina = entry; return true;
            case "StaminaRegen": StaminaRegen = entry; return true;
            case "Damage":       Damage = entry; return true;
            case "AttackSpeed":  AttackSpeed = entry; return true;
            case "MoveSpeed":    MoveSpeed = entry; return true;
        }
        return false;
    }

    protected abstract bool TryAssignClassSpecific(string key, UpgradeEntry entry);
}

// ──────────────────────────────────────────────────────────
// Melee
// ──────────────────────────────────────────────────────────
public class MeleeUpgradeData : ClassUpgradeData
{
    public UpgradeEntry DamageReduction;
    public UpgradeEntry SprintSpeed;

    protected override bool TryAssignClassSpecific(string key, UpgradeEntry entry)
    {
        switch (key)
        {
            case "DamageReduction": DamageReduction = entry; return true;
            case "SprintSpeed":     SprintSpeed = entry; return true;
        }
        return false;
    }
}

// ──────────────────────────────────────────────────────────
// Rifle
// ──────────────────────────────────────────────────────────
public class RifleUpgradeData : ClassUpgradeData
{
    public UpgradeEntry MagazineCapacity;
    public UpgradeEntry ReloadTime;
    public UpgradeEntry PierceCount;
    public UpgradeEntry BulletDistance;

    protected override bool TryAssignClassSpecific(string key, UpgradeEntry entry)
    {
        switch (key)
        {
            case "MagazineCapacity": MagazineCapacity = entry; return true;
            case "ReloadTime":       ReloadTime = entry; return true;
            case "PierceCount":      PierceCount = entry; return true;
            case "BulletDistance":   BulletDistance = entry; return true;
        }
        return false;
    }
}

// ──────────────────────────────────────────────────────────
// Shotgun
// ──────────────────────────────────────────────────────────
public class ShotgunUpgradeData : ClassUpgradeData
{
    public UpgradeEntry MagazineCapacity;
    public UpgradeEntry ReloadTime;
    public UpgradeEntry KnockbackPower;
    public UpgradeEntry ProjectileCount;

    protected override bool TryAssignClassSpecific(string key, UpgradeEntry entry)
    {
        switch (key)
        {
            case "MagazineCapacity": MagazineCapacity = entry; return true;
            case "ReloadTime":       ReloadTime = entry; return true;
            case "KnockbackPower":   KnockbackPower = entry; return true;
            case "ProjectileCount":  ProjectileCount = entry; return true;
        }
        return false;
    }
}

// ──────────────────────────────────────────────────────────
// Pistol
// ──────────────────────────────────────────────────────────
public class PistolUpgradeData : ClassUpgradeData
{
    public UpgradeEntry MagazineCapacity;
    public UpgradeEntry ReloadTime;
    public UpgradeEntry SightRange;
    public UpgradeEntry CollectRange;

    protected override bool TryAssignClassSpecific(string key, UpgradeEntry entry)
    {
        switch (key)
        {
            case "MagazineCapacity": MagazineCapacity = entry; return true;
            case "ReloadTime":       ReloadTime = entry; return true;
            case "SightRange":       SightRange = entry; return true;
            case "CollectRange":     CollectRange = entry; return true;
        }
        return false;
    }
}
