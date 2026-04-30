using UnityEngine;


[CreateAssetMenu(menuName = "Zippers/Wave Info Data")]
public class WaveInfoSO : ScriptableObject, ISheetParsable
{
    [Header("기본 정보")]
    [Tooltip("웨이브 ID (시트 첫 컬럼)")]
    public int WaveId;


    public int Id => WaveId;

    [Header("위치 / 순서")]
    [Tooltip("배틀 노드 인덱스")]
    public int BattleNodeIndex;
    [Tooltip("배틀 노드 내 웨이브 인덱스")]
    public int WaveIndex;

    [Header("타이밍")]
    [Tooltip("시작 딜레이 (초)")]
    public float StartDelay;
    [Tooltip("다음 웨이브까지 딜레이 (초)")]
    public float NextWaveDelay;
    [Tooltip("타임 리밋 (초)")]
    public float TimeLimit;

    [Header("스폰")]
    [Tooltip("스폰 그룹 개수")]
    public int SpawnGroupCount;

    public void SetData(string[] cols)
    {
        WaveId = int.Parse(cols[0]);
        BattleNodeIndex = int.Parse(cols[1]);
        WaveIndex = int.Parse(cols[2]);
        StartDelay = float.Parse(cols[3]);
        NextWaveDelay = float.Parse(cols[4]);
        TimeLimit = float.Parse(cols[5]);
        SpawnGroupCount = int.Parse(cols[6]);
    }
}
