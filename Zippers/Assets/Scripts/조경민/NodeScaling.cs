using UnityEngine;

/// <summary>
/// 전투 노드 진행에 따른 좀비 스텟
/// 사용법 : NodeScaling.GetMultiPlier(전투 노드 수).스텟
/// ex) int health = baseHealth * NodeScaling.GetMultiPlier(battleNodeCount).Health;
/// </summary>
public static class NodeScaling
{
    public static ZombieStatMultiplier GetMultiplier(int battleNodeIndex)
    {
        int x = Mathf.Max(0, battleNodeIndex - 1);

        float health = Mathf.Min(5f, 1f + 0.08f * x + 0.01f * x * x);
        float damage = Mathf.Min(4f, 1f + 0.09f * x + 0.005f * x * x);
        float moveSpeed = Mathf.Min(1.25f, 1f + 0.01f * x + 0.001f * x * x);
        float attackSpeed = Mathf.Min(1.5f, 1f + 0.001f * x + 0.001f * x * x);
        float scrapDrop = Mathf.Min(2f, 1f + 0.05f * x);
        float supplyDrop = Mathf.Min(1.5f, 1f + 0.025f * x);

        return new ZombieStatMultiplier(
            health,
            damage,
            moveSpeed,
            attackSpeed,
            scrapDrop,
            supplyDrop
        );
    }
}
/// <summary>
/// 좀비 스텟 구조체
/// 체력, 데미지, 이동속도, 공격 속도, 스크랩 드랍, 보급품 드랍
/// </summary>
public readonly struct ZombieStatMultiplier
{
    public readonly float Health;
    public readonly float Damage;
    public readonly float MoveSpeed;
    public readonly float AttackSpeed;
    public readonly float ScrapDrop;
    public readonly float SupplyDrop;

    public ZombieStatMultiplier(
        float health,
        float damage,
        float moveSpeed,
        float attackSpeed,
        float scrapDrop,
        float supplyDrop)
    {
        Health = health;
        Damage = damage;
        MoveSpeed = moveSpeed;
        AttackSpeed = attackSpeed;
        ScrapDrop = scrapDrop;
        SupplyDrop = supplyDrop;
    }
}