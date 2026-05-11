using UnityEngine;

public class ZombieProjectile : MonoBehaviour, IPoolable
{
    [SerializeField] private float _speed;
    [SerializeField] private float _lifeTime;

    private ZombieController _zombie;
    private Vector3 _dir;
    private float _damage;
    private float _timer;

    public void Init(ZombieController zombie, Vector3 dir, float damage)
    {
        _zombie = zombie;
        _dir = dir;
        _damage = damage;
    }

    public void OnSpawn()
    {
        _timer = 0;
    }

    public void OnDespawn()
    {

    }

    private void Update()
    {
        _timer += Time.deltaTime;
        transform.position += _dir * _speed * Time.deltaTime;
        if(_timer >= _lifeTime)
        {
            PoolManager.Instance.Release(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsTargetLayer(other.gameObject)) return;

        if (other.TryGetComponent(out IDamagable player))
        {
            player.TakeDamage(_damage);
            PoolManager.Instance.Release(gameObject);
        }
    }

    private bool IsTargetLayer(GameObject target)
    {
        int targetLayer = target.layer;
        return (_zombie.PlayerLayer.value & (1 << targetLayer)) != 0;
    }
}
