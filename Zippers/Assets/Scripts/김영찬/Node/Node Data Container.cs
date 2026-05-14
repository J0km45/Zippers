using System.Collections.Generic;
using UnityEngine;
// Phase C: UnityEngine.Random 의존 제거. NodeManager.Rng (System.Random) 사용.

public class NodeDataContainer : MonoBehaviour
{
    [SerializeField] private NodeSO[] _nodeData;
    [SerializeField] private EventContainerSO[] _eventDataContainers;
    [SerializeField] private DifficultyDataSO[] _difficultyData;

    private Dictionary<NodeType, List<MapController>> _nodeDict = new();
    private Dictionary<NodeEventType, EventContainerSO> _eventDict = new();
    private Dictionary<NodeDifficulty, List<DifficultyDataSO>> _difficultyDict = new();

    int _postBattleNodeIndex;

    public bool IsDictReady { get; private set; }

    // Phase C: NodeManager 의 격리 RNG 사용을 위한 참조. Awake 에서 1회 lookup 후 캐시.
    // NodeManager / NodeDataContainer 가 같은 씬에 있어 Awake 시점에 양쪽 다 살아있음.
    private NodeManager _manager;

    private void Awake()
    {
        _manager = FindFirstObjectByType<NodeManager>();
        if (_manager == null)
        {
            DebugTool.Error("NodeManager 를 찾을 수 없음 - RNG 사용 메서드 호출 시 폴백 적용", DebugType.Node, this);
        }
        InitDict();
        SetDict();
    }
    
    private void InitDict()
    {
        IsDictReady = false;
        
        _postBattleNodeIndex = -1;
    }

    private void SetDict()
    {
        foreach (EventContainerSO data in _eventDataContainers)
        {
            bool verification = _eventDict.TryAdd(data.EventType, data);
            if (!verification) DebugTool.Error($"Event Data Duplicated: {data.EventType}", DebugType.Node, this);
        }
        
        foreach (DifficultyDataSO data in _difficultyData)
        {
            if (!_difficultyDict.ContainsKey(data.Difficulty))
            {
                _difficultyDict.Add(data.Difficulty, new List<DifficultyDataSO>());
            }
        
            _difficultyDict[data.Difficulty].Add(data);
        }
        
        IsDictReady = true;
        DebugTool.Log($"Node Data Container Ready", DebugType.Node, this);
    }

    /// <summary>
    /// 맵 등록
    /// </summary>
    /// <param name="map">노드에 쓰이는 맵</param>
    public void RegisterMap(MapController map)
    {
        NodeType type = map.NodeType;

        if (!_nodeDict.ContainsKey(type))
        {
            _nodeDict.Add(type, new List<MapController>());
        }
        
        _nodeDict[type].Add(map);
        DebugTool.Log($"{map.gameObject.name} Nodes Registered", DebugType.Node, this);
    }

    /// <summary>
    /// 지정 타입의 랜덤 맵 반환
    /// </summary>
    /// <param name="type">Node Type 지정</param>
    /// <returns>선택 된 맵의 MapController</returns>
    public MapController GetRandomMap(NodeType type)
    {
        if (_nodeDict.ContainsKey(type) && _nodeDict[type].Count > 0)
        {
            MapController map;
            
            if (type == NodeType.Battle)
            {
                map = _nodeDict[type][GetIndexBattleNode(_nodeDict[type].Count)];
            }
            else
            {
                map = _nodeDict[type][GetIndex(_nodeDict[type].Count)];
            }

            if (map != null) return map;
        }
        
        DebugTool.Error($"Map Data Not Found: {type}", DebugType.Node, this);
        return null;
    }

    /// <summary>
    /// 난이도 데이터 반환
    /// </summary>
    /// <param name="difficulty">설정된 난이도</param>
    /// <returns>난이도 데이터 SO</returns>
    public DifficultyDataSO GetDifficultyData(NodeDifficulty difficulty)
    {
        if (_difficultyDict.ContainsKey(difficulty) && _difficultyDict[difficulty].Count > 0)
        {
            DifficultyDataSO data = _difficultyDict[difficulty][GetIndex(_difficultyDict[difficulty].Count)];
            
            if (data != null) return data;
        }
        
        DebugTool.Error($"Difficulty Data Not Found: {difficulty}", DebugType.Node, this);
        return null;
    }
    
    /// <summary>
    /// Event SO를 반환
    /// </summary>
    /// <param name="type">불러올 이벤트 타입</param>
    /// <returns>해당 타입에 대응하는 Event SO</returns>
    public EventContainerSO GetEventData(NodeEventType type)
    {
        if (!IsDictReady)
        {
            DebugTool.Error("Data Container Not Ready", DebugType.Node, this);
            return null;
        }
        
        bool verification = _eventDict.TryGetValue(type, out EventContainerSO data);
        if (!verification)
        {
            DebugTool.Error($"Event Data Not Found: {type}", DebugType.Node, this);
            return null;
        }
        return data;
    }

    /// <summary>
    /// 0 ~ count-1 범위의 인덱스 무작위 반환. Phase C: NodeManager.Rng (System.Random) 사용.
    /// _manager 또는 Rng 가 준비되지 않은 비정상 상황엔 0 반환 + 에러 로그.
    /// </summary>
    private int GetIndex(int count)
    {
        if (count <= 0) return 0;

        if (_manager == null || _manager.Rng == null)
        {
            DebugTool.Error($"GetIndex: Node RNG 미초기화 (NodeManager.InitRng 가 호출되지 않음) - 0 반환", DebugType.Node, this);
            return 0;
        }

        return _manager.Rng.Next(0, count);
    }

    /// <summary>
    /// 직전 Battle 노드 인덱스와 다른 인덱스를 반환 (Battle 맵 연속 등장 방지).
    /// Phase C 에서 수정된 버그:
    ///   기존: while 루프 안에서 GetIndex(count) 호출만 하고 반환값을 temp 에 재대입 안 함
    ///         → temp 가 _postBattleNodeIndex 와 같으면 무한 루프
    ///   수정: temp = GetIndex(count) 로 반환값 받음
    /// </summary>
    private int GetIndexBattleNode(int count)
    {
        if (count <= 0) return 0;
        if (count == 1)
        {
            _postBattleNodeIndex = 0;
            return 0;
        }

        int temp = GetIndex(count);

        while (temp == _postBattleNodeIndex)
        {
            temp = GetIndex(count);   // Phase C: 반환값 받음 (이전엔 무한 루프 가능 버그)
        }

        _postBattleNodeIndex = temp;

        return _postBattleNodeIndex;
    }
}
