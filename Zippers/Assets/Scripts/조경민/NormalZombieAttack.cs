using UnityEngine;

public class NormalZombieAttack : MonoBehaviour, IZombieAttack
{
    [Tooltip("왼손 위치")]
    [SerializeField] private Transform _leftHand;
    [Tooltip("오른손 위치")]
    [SerializeField] private Transform _rightHand;

    public void Attack(ZombieController zombie)
    {
        Vector3 center = (_leftHand.position + _rightHand.position) * 0.5f;
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
