using System;
using UnityEngine;


[Serializable]
public class WaveSpawnEntry
{
    [Tooltip("스폰 ID (시트 첫 컬럼)")]
    public int SpawnId;
    [Tooltip("그룹 인덱스 (이 값으로 정렬됨)")]
    public int GroupIndex;
    [Tooltip("스폰될 좀비 ID (ZombieStatSO 참조)")]
    public int ZombieId;
    [Tooltip("총 스폰 수")]
    public int Count;
    [Tooltip("시작 딜레이 (초)")]
    public float StartDelay;
    [Tooltip("배치 간 인터벌 (초)")]
    public float Interval;
    [Tooltip("배치당 스폰 수")]
    public int BatchCount;
    [Tooltip("스폰 반경")]
    public float SpawnRadius;
}
