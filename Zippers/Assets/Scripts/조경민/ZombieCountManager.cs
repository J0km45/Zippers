using UnityEngine;

public class ZombieCountManager : MonoBehaviour
{
    // 현재 웨이브
    public int AliveCount { get; private set; } // 현재 살아있는 좀비 수
    public int SpawnedCount { get; private set; } // 현재 웨이브에서 스폰된 좀비 수
    public bool IsWaveCleared => SpawnedCount > 0 && AliveCount <= 0; // 웨이브 클리어 여부

    // 전체
    public int TotalAliveCount { get; private set; } // 전체 살아있는 좀비 수
    public int TotalSpawnCount { get; private set; } // 전체 웨이브에서 스폰된 좀비 수

    public bool IsNodeCleared => TotalSpawnCount > 0 && TotalAliveCount <= 0; // 노드 클리어 여부

    public void ResetCount()
    {
        AliveCount = 0;
        SpawnedCount = 0;
    }

    public void ResetTotalCount()
    {
        TotalAliveCount = 0;
        TotalSpawnCount = 0;
    }

    public void AddCount()
    {
        SpawnedCount++;
        AliveCount++;

        TotalSpawnCount++;
        TotalAliveCount++;
    }

    public void RemoveCount()
    {
        AliveCount = Mathf.Max(0, AliveCount - 1);
        TotalAliveCount = Mathf.Max(0, TotalAliveCount - 1);
    }
}
