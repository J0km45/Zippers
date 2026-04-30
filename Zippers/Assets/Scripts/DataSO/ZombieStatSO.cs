using UnityEngine;

// Create -> 메뉴: Assets > Create > Zippers > Zombie Stat Data
[CreateAssetMenu(fileName = "ZombieStatSO", menuName = "Zippers/Zombie/Stat Data")]
public class ZombieStatSO : ScriptableObject, ISheetParsable
{
    [Header("기본 정보")]
    [Tooltip("좀비 ID (시트 첫 컬럼)")]
    public int ZombieId;
    [Tooltip("좀비 타입")]
    public ZombieType Type;
    public int Id => ZombieId;

    [Header("체력")]
    [Tooltip("최대 체력")]
    public float MaxHp;
    [Tooltip("체력 회복량")]
    public float HealthRegen;
    [Tooltip("체력 회복 주기")]
    public float HealthPeriod;

    [Header("공격")]
    [Tooltip("최소 공격력")]
    public float MinAttackDamage;
    [Tooltip("최대 공격력")]
    public float MaxAttackDamage;
    [Tooltip("공격 간격")]
    public float AttackCooldown;
    [Tooltip("공격 범위")]
    public float AttackRange;

    [Header("이동 / 감지")]
    [Tooltip("기본 이동 속도")]
    public float MoveSpeed;
    [Tooltip("감지 시 이동 속도")]
    public float DetectMoveSpeed;
    [Tooltip("감지 범위")]
    public float DetectRange;

    [Header("드롭")]
    [Tooltip("최소 스크랩")]
    public float MinScrap;
    [Tooltip("최대 스크랩")]
    public float MaxScrap;
    [Tooltip("스크랩 드롭 확률 (0~100)")]
    public float ScrapDropChance;
    [Tooltip("최소 보급품")]
    public float MinSupplies;
    [Tooltip("최대 보급품")]
    public float MaxSupplies;
    [Tooltip("보급품 드롭 확률 (0~100)")]
    public float SuppliesDropChance;
    [Tooltip("감염 샘플")]
    public float InfectionSample;

    [Header("(시트에 없음 / SetData 미설정)")]
    [Tooltip("손 범위 - 시트에 없는 상수값")]
    public float HandRadius;


    public void SetData(string[] cols)
    {
        ZombieId = int.Parse(cols[0]);
        Type = (ZombieType)System.Enum.Parse(typeof(ZombieType), cols[1]);
        MaxHp = float.Parse(cols[2]);
        HealthRegen = float.Parse(cols[3]);
        HealthPeriod = float.Parse(cols[4]);
        MinAttackDamage = float.Parse(cols[5]);
        MaxAttackDamage = float.Parse(cols[6]);
        AttackCooldown = float.Parse(cols[7]);
        AttackRange = float.Parse(cols[8]);
        MoveSpeed = float.Parse(cols[9]);
        DetectMoveSpeed = float.Parse(cols[10]);
        DetectRange = float.Parse(cols[11]);
        MinScrap = float.Parse(cols[12]);
        MaxScrap = float.Parse(cols[13]);
        ScrapDropChance = float.Parse(cols[14]);
        MinSupplies = float.Parse(cols[15]);
        MaxSupplies = float.Parse(cols[16]);
        SuppliesDropChance = float.Parse(cols[17]);
        InfectionSample = float.Parse(cols[18]);


    }
}
