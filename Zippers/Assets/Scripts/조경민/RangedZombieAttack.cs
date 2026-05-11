using UnityEngine;

public class RangedZombieAttack : MonoBehaviour, IZombieAttack
{
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _firePoint;

    public void Attack(ZombieController zombie)
    {
        Vector3 dir = zombie.Player.position - _firePoint.position;
        dir.y = 0f;
        dir.Normalize();

        GameObject obj = PoolManager.Instance.Get(_projectilePrefab, _firePoint.position, Quaternion.LookRotation(dir));

        if (obj.TryGetComponent(out ZombieProjectile projectile))
        {
            float damage = Random.Range(zombie.MinAttackDamage, zombie.MaxAttackDamage);
            float finalDamage = damage * NodeScaling.GetMultiplier(zombie.NodeManager.BattleCount).Damage;
            finalDamage = Mathf.Round(finalDamage * 10f) * 0.1f;
            projectile.Init(zombie, dir, finalDamage);
        }
    }
}
