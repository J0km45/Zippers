using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// WaveInfoSO들을 BattleNodeIndex로 그룹핑하여 검색을 빠르게 해주는 단일 SO.
/// DataManager가 시트 로드 후 Build()를 호출하면 _groups가 채워지고,
/// 인게임에서는 GetEntries(battleNodeIndex)로 해당 배틀 노드의 웨이브 정보들을 WaveIndex 순으로 조회한다.
/// </summary>
[CreateAssetMenu(menuName = "Zippers/Wave Info Table")]
public class WaveInfoTableSO : ScriptableObject
{
    [SerializeField] private List<WaveInfoGroup> _groups = new();

    // 런타임 조회 가속용 사전. Groups가 바뀌면 EnsureIndex에서 다시 빌드.
    private Dictionary<int, List<WaveInfoSO>> _index;

    /// <summary>현재 보관 중인 그룹 수.</summary>
    public int GroupCount => _groups.Count;

    /// <summary>모든 BattleNodeIndex 순회.</summary>
    public IEnumerable<int> BattleNodeIndices
    {
        get
        {
            foreach (var g in _groups) yield return g.BattleNodeIndex;
        }
    }

    /// <summary>
    /// WaveInfoSO 리스트를 받아 BattleNodeIndex로 그룹핑하고 WaveIndex 오름차순 정렬한다.
    /// 호출 후 _index는 무효화되어 다음 조회 시 재빌드된다.
    /// </summary>
    public void Build(IEnumerable<WaveInfoSO> infos)
    {
        _groups.Clear();
        _index = null;

        if (infos == null)
        {
            DebugTool.Warning("[WaveInfoTableSO] Build: infos가 null", DebugType.Data, this);
            return;
        }

        var temp = new Dictionary<int, WaveInfoGroup>();
        foreach (var info in infos)
        {
            if (info == null) continue;
            if (!temp.TryGetValue(info.BattleNodeIndex, out var group))
            {
                group = new WaveInfoGroup { BattleNodeIndex = info.BattleNodeIndex };
                temp[info.BattleNodeIndex] = group;
                _groups.Add(group);
            }
            group.Entries.Add(info);
        }

        // 각 그룹 내부를 WaveIndex 오름차순 정렬
        foreach (var g in _groups)
        {
            g.Entries.Sort((a, b) => a.WaveIndex.CompareTo(b.WaveIndex));
        }

        DebugTool.Log(
            $"[WaveInfoTableSO] Build 완료 (그룹 {_groups.Count}개)",
            DebugType.Data, this);
    }

    /// <summary>
    /// 지정한 BattleNodeIndex의 WaveInfoSO들을 반환. 없으면 빈 리스트.
    /// 결과는 WaveIndex 오름차순.
    /// </summary>
    public List<WaveInfoSO> GetEntries(int battleNodeIndex)
    {
        EnsureIndex();
        return _index.TryGetValue(battleNodeIndex, out var list) ? list : new List<WaveInfoSO>();
    }

    /// <summary>
    /// 지정한 BattleNodeIndex의 엔트리 존재 여부를 반환. true일 때 entries에 실제 리스트 할당.
    /// </summary>
    public bool TryGetEntries(int battleNodeIndex, out List<WaveInfoSO> entries)
    {
        EnsureIndex();
        return _index.TryGetValue(battleNodeIndex, out entries);
    }

    private void EnsureIndex()
    {
        if (_index != null) return;
        _index = new Dictionary<int, List<WaveInfoSO>>(_groups.Count);
        foreach (var g in _groups)
        {
            _index[g.BattleNodeIndex] = g.Entries;
        }
    }
}
