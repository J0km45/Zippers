using Unity.Netcode;
using UnityEngine;

public class Reward : NetworkBehaviour
{
    private ResourcesType _type = ResourcesType.None; // 재화 타입
    private float _amount; // 재화량
    private bool _isCollected;

    public void Init(ResourcesType type, float amount)
    {
        if (!IsServer) return;
        _type = type;
        _amount = amount;
        _isCollected = false;
    }

    public override void OnNetworkDespawn()
    {
        _type = ResourcesType.None;
        _amount = 0f;
        _isCollected = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if (_isCollected) return;

        if (other.TryGetComponent(out IResourceCollectable resourceCollector))
        {
            _isCollected = true;
            resourceCollector.CollectResource(_type, _amount);
            PoolManager.Instance.Release(gameObject);
        }
    }
}
