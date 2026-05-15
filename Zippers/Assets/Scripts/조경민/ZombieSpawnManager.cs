using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class ZombieSpawnManager : MonoBehaviour
{
    [SerializeField] private ZombieCountManager _zombieCount;
    [SerializeField] private NodeManager _nodeManager;
    [Tooltip("좀비 프리팹 SO")]
    [SerializeField] private ZombiePrefabsSO _zombiePrefabsSO;
    [Tooltip("스포너 위치")]
    [SerializeField] private Transform[] _spawnPoints;

    public bool IsSpawnStopped { get; private set; }

    private void Start()
    {
        PlayerTransformList.instance.OnAllPlayerDead += StopSpawn;
    }

    private void OnDisable()
    {
        PlayerTransformList.instance.OnAllPlayerDead -= StopSpawn;
    }

    public void StartWave(int waveId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        IsSpawnStopped = false;
        List<WaveSpawnEntry> groups = LocalDataAccess.Instance.Game.GetWaveSpawns(waveId);

        foreach (WaveSpawnEntry group in groups)
        {
            DebugTool.Log($"웨이브 {waveId} 그룹 {group.GroupIndex} 스폰 대기", DebugType.Zombie, this);
            StartCoroutine(SpawnGroup(group));
        }
    }

    private IEnumerator SpawnGroup(WaveSpawnEntry group)
    {
        if (!NetworkManager.Singleton.IsServer) yield break;
        if (IsSpawnStopped) yield break;
        if (group.BatchCount <= 0) yield break;

        if (_spawnPoints == null || _spawnPoints.Length == 0)
        {
            DebugTool.Log("스폰 포인트 없음", DebugType.Zombie, this);
            yield break;
        }

        yield return new WaitForSeconds(group.StartDelay);

        if (!NetworkManager.Singleton.IsServer) yield break;
        if (IsSpawnStopped) yield break;
        DebugTool.Log($"그룹 {group.GroupIndex} 스폰 시작", DebugType.Zombie, this);

        int spawnedCount = 0;

        while (spawnedCount < group.Count)
        {
            if (!NetworkManager.Singleton.IsServer) yield break;
            if (IsSpawnStopped) yield break;
            int remainCount = group.Count - spawnedCount;
            int spawnCount = Mathf.Min(group.BatchCount, remainCount);

            for (int i = 0; i < spawnCount; i++)
            {
                SpawnZombie(group);
                spawnedCount++;
            }

            yield return new WaitForSeconds(group.Interval);
        }
    }

    private void SpawnZombie(WaveSpawnEntry group)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        int index = Random.Range(0, _spawnPoints.Length);
        Transform spawnPoint = _spawnPoints[index];

        Vector2 randomPos = Random.insideUnitCircle * group.SpawnRadius;
        Vector3 spawnPos = spawnPoint.position + new Vector3(randomPos.x, 0f, randomPos.y);

        if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            spawnPos = hit.position;
            DebugTool.Log($"NavMesh 위치 지정{spawnPos}", DebugType.Zombie, this);
        }
        else
        {
            DebugTool.Log("NavMesh 위치 탐색 실패", DebugType.Zombie, this);
            return;
        }

        ZombieStatSO stat = LocalDataAccess.Instance.Game.GetZombieStat(group.ZombieId);

        GameObject prefab = _zombiePrefabsSO.GetZombiePrefab(stat.ZombieType);

        GameObject obj = PoolManager.Instance.Get(prefab, spawnPos, spawnPoint.rotation);
        if(obj.TryGetComponent(out ZombieController zombie))
        {
            zombie.Init(_zombieCount, _nodeManager);
        }
    }

    /// <summary>
    /// 스폰 포인트 설정
    /// </summary>
    public void SetSpawnPoint(Transform[] points)
    {
        _spawnPoints = points;
    }

    /// <summary>
    /// 플레이어 전멸 시 스폰을 중지할 때 사용
    /// </summary>
    public void StopSpawn()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        IsSpawnStopped = true;
        DebugTool.Log("좀비 스폰 중지", DebugType.Zombie, this);
    }
}
