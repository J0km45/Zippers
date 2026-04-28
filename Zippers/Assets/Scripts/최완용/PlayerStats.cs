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
    public float Stamina => _playerClassData.Stamina;
    public float StaminaDelay => _playerClassData.StaminaDelay;
    public float StaminaPeriod => _playerClassData.StaminaPeriod;
    public float StaminaRegen => _playerClassData.StaminaRegen;
    public float WeaponDamage => _playerClassData.WeaponDamage;
    public float AttackSpeed => _playerClassData.AttackSpeed;
    public float MagazineCapacity => _playerClassData.MagazineCapacity;
    public float ReloadTime => _playerClassData.ReloadTime;
    public float BulletSpeed => _playerClassData.BulletSpeed;
    public float BulletDistance => _playerClassData.BulletDistance;
    public float MoveSpeed => _playerClassData.MoveSpeed;
    public float SightRange => _playerClassData.SightRange;
    public float CollectRange => _playerClassData.CollectRange;

    public bool UseBullet => 
        WeaponType == WeaponType.Rifle || 
        WeaponType == WeaponType.Shotgun || 
        WeaponType == WeaponType.Util;

    //public float CurrentHealth { get; private set; }
    //public float CurrentStamina { get; private set; }

    //private void Awake()
    //{
    //    Init();
    //}

    //private void Init()
    //{
    //    CurrentHealth = MaxHealth;
    //    CurrentStamina = Stamina;
    //}
    //public void TakeDamage(float damage)
    //{
    //    CurrentHealth = Mathf.Max(CurrentHealth - damage, 0f);
    //}
    //public bool IsDie()
    //{
    //    return CurrentHealth <= 0f;
    //}
}


