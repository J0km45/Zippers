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
    
    [SerializeField] private NodeSO[] _nodes;

    private Dictionary<(NodeType ,int), NodeSO> _dict;
    
    private int _countBattleNodes;
    private int _countBossNodes;
    private int _countShopNodes;
    private int _countEscapeNodes;
    private int _countStartNodes;
    private int _postBattleIndex;

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
        _dict = new Dictionary<(NodeType ,int), NodeSO>();

        _countBattleNodes = 0;
        _countBossNodes = 0;
        _countEscapeNodes = 0;
        _countShopNodes = 0;
        _countStartNodes = 0;
        _postBattleIndex = -1;
        
        foreach (NodeSO node in _nodes)
        {
            bool verification = _dict.TryAdd((node.NodeType, node.NodeIndex), node);
            if(!verification) DebugTool.Error($"Node Dictionary Duplication Error : {node.NodeType}_{node.NodeIndex}", DebugType.Node, this);
            else
            {
                switch (node.NodeType)
                {
                    case NodeType.Battle:
                        _countBattleNodes++;
                        break;
                    case NodeType.Boss:
                        _countBossNodes++;
                        break;
                    case NodeType.Shop:
                        _countShopNodes++;
                        break;
                    case NodeType.Escape:
                        _countEscapeNodes++;
                        break;
                    case NodeType.Start:
                        _countStartNodes++;
                        break;
                    default:
                        break;
                }
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
        
        int index = GetIndexNumber(nodeType);

        NodeSO result = _dict.GetValueOrDefault((nodeType, index));
        
        if (result == null) DebugTool.Error($"Node Not Found : {nodeType}_{index}", DebugType.Node, this);
        
        return result;
    }

    private int GetIndexNumber(NodeType nodeType)
    {
        switch (nodeType)
        {
            case NodeType.Battle:
                if (_countBattleNodes <= 1) return 0;
                int temp = Random.Range(0, _countBattleNodes);
                while (temp == _postBattleIndex)
                {
                    temp = Random.Range(0, _countBattleNodes);
                }
                _postBattleIndex = temp;
                return temp;
            case NodeType.Boss:
                if (_countBossNodes <= 1) return 0;
                return Random.Range(0, _countBossNodes);
            case NodeType.Shop:
                if (_countShopNodes <= 1) return 0;
                return Random.Range(0, _countShopNodes);
            case NodeType.Escape:
                if (_countEscapeNodes <= 1) return 0;
                return Random.Range(0, _countEscapeNodes);
            case NodeType.Start:
                if (_countStartNodes <= 1) return 0;
                return Random.Range(0, _countStartNodes);
            default:
                return 0;
        }
    }
}
