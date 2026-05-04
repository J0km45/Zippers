using UnityEngine;



[CreateAssetMenu(fileName = "PlayerClassDataSO", menuName = "ScriptableObjects/PlayerClassDataSO", order = 1)]
public class PlayerClassDataSO : ScriptableObject, ISheetParsable
{
    [Header("기본 정보")]
    [Tooltip("클래스 아이디")] public int ClassId;
    [Tooltip("클래스 타입")] public WeaponType WeaponType;
    [Tooltip("클래스 이름")] public string ClassName;

    public int Id => ClassId;

    [Header("클래스 스텟")]
    [Tooltip("클래스 체력")] public float MaxHealth;
    [Tooltip("클래스 스테미나")] public float MaxStamina;
    [Tooltip("스테미나 지연 시간")] public float StaminaDelay;
    [Tooltip("스테미나 주기")] public float StaminaPeriod;
    [Tooltip("스테미나 소비량")] public float StaminaConsume;
    [Tooltip("스테미나 회복량")] public float StaminaRegen;

    [Header("전투 스텟")]
    [Tooltip("무기 최소 피해량")] public float MinDamage;
    [Tooltip("무기 최대 피해량")] public float MaxDamage;

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

    public void SetData(string[] datas)
    {
        ClassId = int.Parse(datas[0]);
        WeaponType = (WeaponType)System.Enum.Parse(typeof(WeaponType), datas[1]);
        ClassName = datas[2];
        MaxHealth = float.Parse(datas[3]);
        MaxStamina = float.Parse(datas[4]);
        StaminaDelay = float.Parse(datas[5]);
        StaminaPeriod = float.Parse(datas[6]);
        StaminaConsume = float.Parse(datas[7]);
        StaminaRegen = float.Parse(datas[8]);
        MinDamage = float.Parse(datas[9]);
        MaxDamage = float.Parse(datas[10]);
        AttackSpeed = float.Parse(datas[11]);
        MagazineCapacity = float.Parse(datas[12]);
        ReloadTime = float.Parse(datas[13]);
        BulletSpeed = float.Parse(datas[14]);
        BulletDistance = float.Parse(datas[15]);
        MoveSpeed = float.Parse(datas[16]);
        SprintSpeed = float.Parse(datas[17]);
        SightRange = float.Parse(datas[18]);
        CollectRange = float.Parse(datas[19]);



    }
}
