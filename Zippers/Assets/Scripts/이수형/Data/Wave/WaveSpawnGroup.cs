using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class WaveSpawnGroup
{
    [Tooltip("이 그룹이 속한 웨이브 ID (WaveInfoSO 참조)")]
    public int WaveId;
    [Tooltip("이 웨이브의 스폰 엔트리들 (GroupIndex 오름차순)")]
    public List<WaveSpawnEntry> Entries = new();
}
