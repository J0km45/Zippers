using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Node SO를 목록화 하고 검색
/// </summary>
public class NodeDictionary : MonoBehaviour
{
    /// <summary>
    /// SingleTon Instance
    /// </summary>
    public static NodeDictionary Instance { get; private set; }

    [Header("테스트 노드 데이터")]
    [SerializeField] private NodeSO _testNode;
    
    [Header("테스트 노드 제외한 나머지 노드 데이터들")]
    [SerializeField] private NodeSO[] _nodes;
    
    private Dictionary<int, NodeSO> _dict_Start;
    private Dictionary<int, NodeSO> _dict_Battle;
    private Dictionary<int, NodeSO> _dict_Boss;
    private Dictionary<int, NodeSO> _dict_Shop;
    private Dictionary<int, NodeSO> _dict_Escape;
    
    private int _postBattleIndex;
    private int _postBossIndex;
    private int _postShopIndex;

    public bool IsDictionaryReady { get; private set; }

    private void Awake()
    {
        SetSingleTon();
        IsDictionaryReady = false;
        InitDict();
    }
    
    private void SetSingleTon()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void InitDict()
    {
        _dict_Battle = new Dictionary<int, NodeSO>();
        _dict_Boss = new Dictionary<int, NodeSO>();
        _dict_Shop = new Dictionary<int, NodeSO>();
        _dict_Escape = new Dictionary<int, NodeSO>();
        _dict_Start = new Dictionary<int, NodeSO>();

        _postBattleIndex = -1;
        
        foreach (NodeSO node in _nodes)
        {
            switch (node.NodeType)
            {
                case NodeType.Battle:
                    bool tempBattle = _dict_Battle.TryAdd(node.NodeIndex, node);
                    if (!tempBattle) DebugTool.Error($"NodeSO Index Duplicate : {node.NodeType}_{node.NodeIndex}", DebugType.Node, this);
                    break;
                case NodeType.Boss:
                    bool tempBoss = _dict_Boss.TryAdd(node.NodeIndex, node);
                    if (!tempBoss) DebugTool.Error($"NodeSO Index Duplicate : {node.NodeType}_{node.NodeIndex}", DebugType.Node, this);
                    break;
                case NodeType.Shop:
                    bool tempShop = _dict_Shop.TryAdd(node.NodeIndex, node);
                    if (!tempShop) DebugTool.Error($"NodeSO Index Duplicate : {node.NodeType}_{node.NodeIndex}", DebugType.Node, this);
                    break;
                case NodeType.Escape:
                    bool tempEscape = _dict_Escape.TryAdd(node.NodeIndex, node);
                    if (!tempEscape) DebugTool.Error($"NodeSO Index Duplicate : {node.NodeType}_{node.NodeIndex}", DebugType.Node, this);
                    break;
                case NodeType.Start:
                    bool tempStart = _dict_Start.TryAdd(node.NodeIndex, node);
                    if (!tempStart) DebugTool.Error($"NodeSO Index Duplicate : {node.NodeType}_{node.NodeIndex}", DebugType.Node, this);
                    break;
                default:
                    break;
            }
        }
        
        IsDictionaryReady = true;
        DebugTool.Log("Node Dictionary Ready", DebugType.Node, this);
    }
    
    /// <summary>
    /// 노드 데이터 검색
    /// </summary>
    /// <param name="nodeType">노드 타입</param>
    /// <returns></returns>
    public NodeSO GetNodeData(NodeType nodeType)
    {
        if (!IsDictionaryReady)
        {
            DebugTool.Error("Node Dictionary Not Ready", DebugType.Node, this);
            return null;
        }

        NodeSO result;
        
        switch (nodeType)
        {
            case NodeType.Test:
                result = _testNode;
                break;
            case NodeType.Battle:
                result = _dict_Battle.GetValueOrDefault(GetIndexNumber(nodeType));
                if (result == null) DebugTool.Error($"NodeSO Not Found : {nodeType}_{GetIndexNumber(nodeType)}", DebugType.Node, this);
                break;
            case NodeType.Boss:
                result = _dict_Boss.GetValueOrDefault(GetIndexNumber(nodeType));
                if (result == null) DebugTool.Error($"NodeSO Not Found : {nodeType}_{GetIndexNumber(nodeType)}", DebugType.Node, this);
                break;
            case NodeType.Shop:
                result = _dict_Shop.GetValueOrDefault(GetIndexNumber(nodeType));
                if (result == null) DebugTool.Error($"NodeSO Not Found : {nodeType}_{GetIndexNumber(nodeType)}", DebugType.Node, this);
                break;
            case NodeType.Escape:
                result = _dict_Escape.GetValueOrDefault(GetIndexNumber(nodeType));
                if (result == null) DebugTool.Error($"NodeSO Not Found : {nodeType}_{GetIndexNumber(nodeType)}", DebugType.Node, this);
                break;
            case NodeType.Start:
                result = _dict_Start.GetValueOrDefault(GetIndexNumber(nodeType));
                if (result == null) DebugTool.Error($"NodeSO Not Found : {nodeType}_{GetIndexNumber(nodeType)}", DebugType.Node, this);
                break;
            case NodeType.Empty:
                return null;
            default:
                DebugTool.Error($"NodeSO Not Found : {nodeType}_{GetIndexNumber(nodeType)}", DebugType.Node, this);
                return null;
        }
        
        return result;
    }

    private int GetIndexNumber(NodeType nodeType)
    {
        switch (nodeType)
        {
            case NodeType.Battle:
                int temp = Random.Range(0, _dict_Battle.Count - 1);
                while (temp == _postBattleIndex)
                {
                    temp = Random.Range(0, _dict_Battle.Count - 1);
                }
                _postBattleIndex = temp;
                return temp;
            case NodeType.Boss:
                return Random.Range(0, _dict_Boss.Count - 1);
            case NodeType.Shop:
                return Random.Range(0, _dict_Shop.Count - 1);
            case NodeType.Escape:
                return Random.Range(0, _dict_Escape.Count - 1);
            case NodeType.Start:
                return Random.Range(0, _dict_Start.Count - 1);
            default:
                return 0;
        }
    }
}
