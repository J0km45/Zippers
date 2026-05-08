using UnityEngine;

public class Reward : MonoBehaviour, IPoolable
{
    private ResourcesType _type = ResourcesType.None; // 재화 타입
    private float _amount; // 재화량
    private bool _isCollected;

    public void Init(ResourcesType type, float amount)
    {
        _type = type;
        _amount = amount;
    }

    public void OnSpawn()
    {
        _isCollected = false;
    }

    public void OnDespawn()
    {
        _type = ResourcesType.None;
        _amount = 0f;
        _isCollected = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isCollected) return;

        if (other.TryGetComponent(out IResourceCollectable resourceCollector))
        {
            _isCollected = true;
            resourceCollector.CollectResource(_type, _amount);
            PoolManager.Instance.Release(gameObject);
        }
    }
}
