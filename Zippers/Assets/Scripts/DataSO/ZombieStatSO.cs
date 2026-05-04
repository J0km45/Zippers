using UnityEngine;

// Create -> 메뉴: Assets > Create > Zippers > Zombie Stat Data
[CreateAssetMenu(fileName = "ZombieStatSO", menuName = "Zippers/Zombie/Stat Data")]
public class ZombieStatSO : ScriptableObject, ISheetParsable
{
    [Header("기본 정보")]
    [Tooltip("좀비 ID (시트 첫 컬럼)")]
    public int ZombieId;
    [Tooltip("좀비 타입")]
    public ZombieType ZombieType;
    public int Id => ZombieId;

    [Header("체력")]
    [Tooltip("최대 체력")]
    public float MaxHealth;
    [Tooltip("체력 회복량")]
    public float HealthRegen;
    [Tooltip("체력 회복 주기")]
    public float HealthPeriod;

    [Header("공격")]
    [Tooltip("최소 공격력")]
    public float MinDamage;
    [Tooltip("최대 공격력")]
    public float MaxDamage;
    [Tooltip("공격 간격")]
    public float AttackSpeed;
    [Tooltip("공격 범위")]
    public float AttackRange;

    [Header("이동 / 감지")]
    [Tooltip("기본 이동 속도")]
    public float BaseMoveSpeed;
    [Tooltip("감지 시 이동 속도")]
    public float ChasingMoveSpeed;
    [Tooltip("감지 범위")]
    public float DetectionRange;

    [Header("드롭")]
    [Tooltip("최소 스크랩")]
    public int MinScrap;
    [Tooltip("최대 스크랩")]
    public int MaxScrap;
    [Tooltip("스크랩 드롭 확률 (0~100)")]
    public float ScrapDropChance;
    [Tooltip("최소 보급품")]
    public int MinSupplies;
    [Tooltip("최대 보급품")]
    public int MaxSupplies;
    [Tooltip("보급품 드롭 확률 (0~100)")]
    public float SuppliesDropChance;
    [Tooltip("감염 샘플")]
    public int InfectionSample;
    [Tooltip("드랍 찬스")]
    public float SampleDropChance;

    [Header("(시트에 없음 / SetData 미설정)")]
    [Tooltip("손 범위 - 시트에 없는 상수값")]
    public float HandRadius;


    public void SetData(string[] cols)
    {
        ZombieId = int.Parse(cols[0]);
        ZombieType = (ZombieType)System.Enum.Parse(typeof(ZombieType), cols[1]);
        MaxHealth = float.Parse(cols[2]);
        HealthRegen = float.Parse(cols[3]);
        HealthPeriod = float.Parse(cols[4]);
        MinDamage = float.Parse(cols[5]);
        MaxDamage = float.Parse(cols[6]);
        AttackSpeed = float.Parse(cols[7]);
        AttackRange = float.Parse(cols[8]);
        BaseMoveSpeed = float.Parse(cols[9]);
        ChasingMoveSpeed = float.Parse(cols[10]);
        DetectionRange = float.Parse(cols[11]);
        MinScrap = int.Parse(cols[12]);
        MaxScrap = int.Parse(cols[13]);
        ScrapDropChance = float.Parse(cols[14]);
        MinSupplies = int.Parse(cols[15]);
        MaxSupplies = int.Parse(cols[16]);
        SuppliesDropChance = float.Parse(cols[17]);
        InfectionSample = int.Parse(cols[18]);
        SampleDropChance = float.Parse(cols[19]);


    }
}
