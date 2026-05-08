using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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

    private void Awake()
    {
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

    private int GetIndex(int count)
    {
        if (count <= 0) return 0;
        
        return Random.Range(0, count);;
    }

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
            GetIndex(count);
        }
        
        _postBattleNodeIndex = temp;
        
        return _postBattleNodeIndex;
    }
}
