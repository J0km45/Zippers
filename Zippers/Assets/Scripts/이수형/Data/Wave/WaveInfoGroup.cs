using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 같은 BattleNodeIndex에 속하는 WaveInfoSO 묶음.
/// Entries는 WaveIndex 오름차순으로 정렬되어 있다.
/// </summary>
[Serializable]
public class WaveInfoGroup
{
    [Tooltip("이 그룹의 배틀 노드 인덱스")]
    public int BattleNodeIndex;
    [Tooltip("이 배틀 노드의 웨이브 정보들 (WaveIndex 오름차순)")]
    public List<WaveInfoSO> Entries = new();
}
