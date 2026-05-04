using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Zippers/Wave Spawn Table")]
public class WaveSpawnTableSO : ScriptableObject
{
    [SerializeField] private List<WaveSpawnGroup> _groups = new();


    private Dictionary<int, List<WaveSpawnEntry>> _index;


    public int GroupCount => _groups.Count;


    public IEnumerable<int> WaveIds
    {
        get
        {
            foreach (var g in _groups) yield return g.WaveId;
        }
    }


    public void LoadFromSheet(char split, string[] lines, int headerRowCount = 1)
    {
        if (lines == null)
        {
            DebugTool.Error("[WaveSpawnTableSO] lines가 null - 로드 중단", DebugType.Data, this);
            return;
        }

        _groups.Clear();
        _index = null;

        var temp = new Dictionary<int, WaveSpawnGroup>();

        for (int i = headerRowCount; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] cols = line.Split(split);
            if (cols.Length < 9)
            {
                DebugTool.Error(
                    $"[WaveSpawnTableSO] {i}번째 줄 컬럼 수 부족: {cols.Length}/9",
                    DebugType.Data, this);
                continue;
            }

            if (!int.TryParse(cols[1], out int waveId))
            {
                DebugTool.Error(
                    $"[WaveSpawnTableSO] {i}번째 줄 WaveId 파싱 실패: '{cols[1]}'",
                    DebugType.Data, this);
                continue;
            }

            if (!temp.TryGetValue(waveId, out var group))
            {
                group = new WaveSpawnGroup { WaveId = waveId };
                temp[waveId] = group;
                _groups.Add(group);
            }

            try
            {
                var entry = new WaveSpawnEntry
                {
                    SpawnId       = int.Parse(cols[0]),
                    GroupIndex    = int.Parse(cols[2]),
                    ZombieId      = int.Parse(cols[3]),
                    Count         = int.Parse(cols[4]),
                    StartDelay    = float.Parse(cols[5]),
                    Interval      = float.Parse(cols[6]),
                    BatchCount    = int.Parse(cols[7]),
                    SpawnRadius   = float.Parse(cols[8]),
                };
                group.Entries.Add(entry);
            }
            catch (System.Exception e)
            {
                DebugTool.Error(
                    $"[WaveSpawnTableSO] {i}번째 줄 파싱 실패: {e.Message}",
                    DebugType.Data, this);
            }
        }


        foreach (var g in _groups)
        {
            g.Entries.Sort((a, b) => a.GroupIndex.CompareTo(b.GroupIndex));
        }

        DebugTool.Log(
            $"[WaveSpawnTableSO] 시트 로드 완료 (그룹 {_groups.Count}개)",
            DebugType.Data, this);
    }



    public List<WaveSpawnEntry> GetEntries(int waveId)
    {
        EnsureIndex();
        return _index.TryGetValue(waveId, out var list) ? list : new List<WaveSpawnEntry>();
    }


    public bool TryGetEntries(int waveId, out List<WaveSpawnEntry> entries)
    {
        EnsureIndex();
        return _index.TryGetValue(waveId, out entries);
    }

    private void EnsureIndex()
    {
        if (_index != null) return;
        _index = new Dictionary<int, List<WaveSpawnEntry>>(_groups.Count);
        foreach (var g in _groups)
        {
            _index[g.WaveId] = g.Entries;
        }
    }
}
