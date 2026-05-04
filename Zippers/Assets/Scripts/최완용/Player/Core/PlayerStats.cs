using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("클래스 데이터")]
    [SerializeField] private PlayerClassDataSO _playerClassData;
    public PlayerClassDataSO PlayerClassData => _playerClassData;

    public int ClassID => _playerClassData.ClassId;
    public WeaponType WeaponType => _playerClassData.WeaponType;
    public string ClassName => _playerClassData.ClassName;
    public float MaxHealth => _playerClassData.MaxHealth;
    public float Stamina => _playerClassData.MaxStamina;
    public float StaminaDelay => _playerClassData.StaminaDelay;
    public float StaminaPeriod => _playerClassData.StaminaPeriod;
    public float StaminaRegen => _playerClassData.StaminaRegen;
    public float MinDamage => _playerClassData.MinDamage;
    public float MaxDamage => _playerClassData.MaxDamage;
    public float TotalMinDamage => MinDamage; // 향후 버프/디버프 적용 시 계산식 추가
    public float TotalMaxDamage => MaxDamage; // 향후 버프/디버프 적용 시 계산식 추가
    public float AttackSpeed => _playerClassData.AttackSpeed;
    public float MagazineCapacity => _playerClassData.MagazineCapacity;
    public float ReloadTime => _playerClassData.ReloadTime;
    public float BulletSpeed => _playerClassData.BulletSpeed;
    public float BulletDistance => _playerClassData.BulletDistance;
    public float MoveSpeed => _playerClassData.MoveSpeed;
    public float SprintSpeed => _playerClassData.SprintSpeed;
    public float SightRange => _playerClassData.SightRange;
    public float CollectRange => _playerClassData.CollectRange;

    public bool UseBullet => 
        WeaponType == WeaponType.Rifle || 
        WeaponType == WeaponType.Shotgun || 
        WeaponType == WeaponType.Pistol;

    public float GetRandomDamage()
    {
        return Random.Range(TotalMinDamage, TotalMaxDamage);
    }
}


