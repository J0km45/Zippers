using UnityEngine;

[CreateAssetMenu(menuName = "Zippers/Player Class Data")]
public class PlayerClassDataSO : ScriptableObject
{
    [Header("기본 정보")]
    [Tooltip("클래스 아이디")] public int ClassId;
    [Tooltip("클래스 타입")] public WeaponType WeaponType;
    [Tooltip("클래스 이름")] public string ClassName;

    [Header("클래스 스텟")]
    [Tooltip("클래스 체력")] public float MaxHealth;
    [Tooltip("클래스 스테미나")] public float Stamina;
    [Tooltip("스테미나 지연 시간")] public float StaminaDelay;
    [Tooltip("스테미나 주기")] public float StaminaPeriod;
    [Tooltip("스테미나 회복량")] public float StaminaRegen;

    [Header("전투 스텟")]
    [Tooltip("무기 피해량")] public float WeaponDamage;
    [Tooltip("공격 속도")] public float AttackSpeed;
    [Tooltip("탄창 용량")] public float MagazineCapacity;
    [Tooltip("재장전 시간")] public float ReloadTime;
    [Tooltip("탄환 속도")] public float BulletSpeed;
    [Tooltip("탄환 사거리")] public float BulletDistance;

    [Header("이동속도 / 시야 / 수집 범위")]
    [Tooltip("이동 속도")] public float MoveSpeed;
    [Tooltip("달리기 속도")] public float SprintSpeed;
    [Tooltip("시야 범위")] public float SightRange;
    [Tooltip("아이템 수집 범위")] public float CollectRange;
}
