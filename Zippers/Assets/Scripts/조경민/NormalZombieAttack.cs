using UnityEngine;

public class NormalZombieAttack : MonoBehaviour, IZombieAttack
{
    public void Attack(ZombieController zombie)
    {
        Vector3 center = (zombie.LeftHand.position + zombie.RightHand.position) * 0.5f;
        Collider[] hits = Physics.OverlapSphere(center, zombie.HandRadius, zombie.PlayerLayer);

        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent(out IDamagable player))
            {
                float damage = Random.Range(zombie.MinAttackDamage, zombie.MaxAttackDamage);
                float finalDamage = damage * NodeScaling.GetMultiplier(zombie.NodeManager.BattleCount).Damage;
                finalDamage = Mathf.Round(finalDamage * 10f) * 0.1f;
                player.TakeDamage(finalDamage);
                break;
            }
        }
    }
}
