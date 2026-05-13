using UnityEngine;

public class BossZombieAttack : MonoBehaviour, IZombieAttack
{
    [Tooltip("오른손 위치")]
    [SerializeField] private Transform _rightHand;

    public void Attack(ZombieController zombie)
    {
        Collider[] hits = Physics.OverlapSphere(_rightHand.position, zombie.HandRadius, zombie.PlayerLayer);

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
