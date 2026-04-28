using UnityEngine;

// Create -> 메뉴: Assets > Create > Zippers > Zombie Stat Data
[CreateAssetMenu(menuName = "Zippers/Zombie Stat Data")]
public class ZombieStatSO : ScriptableObject
{
    [Tooltip("좀비 타입")]
    public ZombieType Type;
    [Tooltip("최대 체력")]
    public float MaxHp;
    [Tooltip("기본 이동 속도")]
    public float MoveSpeed;
    [Tooltip("감지 시 이동 속도")]
    public float DetectMoveSpeed;
    [Tooltip("최소 공격력")]
    public float MinAttackDamage;
    [Tooltip("최대 공격력")]
    public float MaxAttackDamage;
    [Tooltip("공격 간격")]
    public float AttackCooldown;
    [Tooltip("손 범위")]
    public float HandRadius;
    [Tooltip("공격 범위")]
    public float AttackRange;
    [Tooltip("감지 범위")]
    public float DetectRange;
    [Tooltip("처치 재화")]
    public float KillReward;
}

public enum ZombieType
{
    Normal,
}