using UnityEngine;


[CreateAssetMenu(menuName = "Zippers/Wave Spawn Data")]
public class WaveSpawnSO : ScriptableObject, ISheetParsable
{
    [Header("기본 정보")]
    [Tooltip("스폰 ID (시트 첫 컬럼)")]
    public int SpawnId;


    public int Id => SpawnId;

    [Header("참조")]
    [Tooltip("연결되는 웨이브 ID (WaveInfoSO 참조)")]
    public int WaveId;
    [Tooltip("스폰될 좀비 ID (ZombieStatSO 참조)")]
    public int ZombieId;

    [Header("위치 / 그룹")]
    [Tooltip("그룹 인덱스")]
    public int GroupIndex;
    [Tooltip("스폰 포지션 인덱스")]
    public int PositionIndex;

    [Header("스폰 수치")]
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

    public void SetData(string[] cols)
    {
        SpawnId = int.Parse(cols[0]);
        WaveId = int.Parse(cols[1]);
        GroupIndex = int.Parse(cols[2]);
        PositionIndex = int.Parse(cols[3]);
        ZombieId = int.Parse(cols[4]);
        Count = int.Parse(cols[5]);
        StartDelay = float.Parse(cols[6]);
        Interval = float.Parse(cols[7]);
        BatchCount = int.Parse(cols[8]);
        SpawnRadius = float.Parse(cols[9]);
    }
}
