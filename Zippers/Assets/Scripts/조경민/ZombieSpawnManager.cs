using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawnManager : MonoBehaviour
{
    [SerializeField] private ZombieCountManager _zombieCount;
    [Tooltip("좀비 프리팹 SO")]
    [SerializeField] private ZombiePrefabsSO _zombiePrefabsSO;
    [Tooltip("스포너 위치")]
    [SerializeField] private Transform[] _spawnPoints;

    public void StartWave(int waveId)
    {
        List<WaveSpawnEntry> groups = LocalDataAccess.Instance.Game.GetWaveSpawns(waveId);

        foreach (WaveSpawnEntry group in groups)
        {
            DebugTool.Log($"웨이브 {waveId} 그룹 {group.GroupIndex} 대기", DebugType.Zombie, this);
            StartCoroutine(SpawnGroup(group));
        }
    }

    private IEnumerator SpawnGroup(WaveSpawnEntry group)
    {
        if (group.BatchCount <= 0) yield break;

        if (_spawnPoints == null || _spawnPoints.Length == 0)
        {
            DebugTool.Log("스폰 포인트 없음", DebugType.Zombie, this);
            yield break;
        }

        yield return new WaitForSeconds(group.StartDelay);
        DebugTool.Log($"그룹 {group.GroupIndex} 시작", DebugType.Zombie, this);

        int spawnedCount = 0;

        while (spawnedCount < group.Count)
        {
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
        int index = Random.Range(0, _spawnPoints.Length);
        Transform spawnPoint = _spawnPoints[index];

        Vector2 randomPos = Random.insideUnitCircle * group.SpawnRadius;
        Vector3 spawnPos = spawnPoint.position + new Vector3(randomPos.x, 0f, randomPos.y);

        ZombieStatSO stat = LocalDataAccess.Instance.Game.GetZombieStat(group.ZombieId);

        GameObject prefab = _zombiePrefabsSO.GetZombiePrefab(stat.ZombieType);

        GameObject obj = PoolManager.Instance.Get(prefab, spawnPos, spawnPoint.rotation);
        if(obj.TryGetComponent(out ZombieController zombie))
        {
            zombie.Init(_zombieCount);
        }
    }
}
