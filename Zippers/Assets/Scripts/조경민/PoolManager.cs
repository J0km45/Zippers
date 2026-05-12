using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    private Dictionary<GameObject, Queue<GameObject>> _pools = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public GameObject Get(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        // 해당 Prefab이 큐에 있는지 확인
        if (!_pools.ContainsKey(prefab))
        {
            _pools.Add(prefab, new Queue<GameObject>());
        }

        GameObject obj;

        // 해댱 큐에 Prefab 수가 충분한지 체크, 없으면 생성
        if (_pools[prefab].Count > 0)
        {
            // 큐에서 꺼내기
            obj = _pools[prefab].Dequeue();
        }
        else
        {
            obj = CreateObj(prefab);
        }

        obj.transform.position = pos;
        obj.transform.rotation = rot;
        obj.SetActive(true);

        if (obj.TryGetComponent(out IPoolable poolable))
        {
            poolable.OnSpawn();
        }

        if (obj.TryGetComponent(out NetworkObject networkObject))
        {
            if (!networkObject.IsSpawned)
            {
                networkObject.Spawn();
            }
        }

        return obj;
    }

    // 데이터 집어넣기
    public void Release(GameObject obj)
    {
        if (!obj.activeSelf) return;

        if (!obj.TryGetComponent(out PoolObject poolObject))
        {
            Destroy(obj);
            return;
        }

        GameObject prefab = poolObject.OriginPrefab;

        if (!_pools.ContainsKey(prefab))
        {
            _pools.Add(prefab, new Queue<GameObject>());
        }

        if (obj.TryGetComponent(out IPoolable poolable))
        {
            poolable.OnDespawn();
        }

        if(obj.TryGetComponent(out NetworkObject networkObject))
        {
            if (networkObject.IsSpawned)
            {
                networkObject.Despawn(false);
            }
        }

        obj.SetActive(false);
        obj.transform.SetParent(transform);

        _pools[prefab].Enqueue(obj); // 큐에 다시 넣기
    }

    private GameObject CreateObj(GameObject prefab)
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.name = prefab.name;

        // OriginPrefab 저장
        if (!obj.TryGetComponent(out PoolObject poolObject))
        {
            poolObject = obj.AddComponent<PoolObject>();
        }

        poolObject.SetOrigin(prefab);

        return obj;
    }

    // 미리 생성
    public void CreatePool(GameObject prefab, int count)
    {
        if (!_pools.ContainsKey(prefab))
        {
            _pools.Add(prefab, new Queue<GameObject>());
        }

        for (int i = 0; i < count; i++)
        {
            GameObject obj = CreateObj(prefab);
            obj.SetActive(false);
            _pools[prefab].Enqueue(obj);
        }
    }
}