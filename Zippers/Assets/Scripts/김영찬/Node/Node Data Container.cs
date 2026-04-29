using System;
using System.Collections.Generic;
using UnityEngine;

public class NodeDataContainer : MonoBehaviour
{
    [SerializeField] private NodeSO[] _nodeData;
    [SerializeField] private EventSO[] _eventData;
    [SerializeField] private TreeSO[] _treeData;

    private Dictionary<NodeType, NodeSO> _node_dict;
    private Dictionary<NodeEventType, EventSO> _event_dict;
    private Dictionary<NodeDifficulty, TreeSO> _tree_dict;

    private bool _isReady;
    
    private void Awake()
    {
        InitDict();
        SetDict();
    }
    
    private void InitDict()
    {
        _isReady = false;
        _node_dict = new Dictionary<NodeType, NodeSO>();
        _event_dict = new Dictionary<NodeEventType, EventSO>();
        _tree_dict = new Dictionary<NodeDifficulty, TreeSO>();
    }

    private void SetDict()
    {
        foreach (NodeSO node in _nodeData)
        {
            bool temp = _node_dict.TryAdd(node.NodeType, node);
            if (!temp) DebugTool.Error($"Node Data Duplicated: {node.NodeType}", DebugType.Node, this);
        }

        foreach (EventSO @event in _eventData)
        {
            bool temp = _event_dict.TryAdd(@event.EventType, @event);
            if (!temp) DebugTool.Error($"Event Data Duplicated: {@event.EventType}", DebugType.Node, this);
        }

        foreach (TreeSO tree in _treeData)
        {
            bool temp = _tree_dict.TryAdd(tree.Difficulty, tree);
            if (!temp) DebugTool.Error($"Tree Data Duplicated: {tree.Difficulty}", DebugType.Node, this);
        }
        
        _isReady = true;
    }

    public NodeSO GetNodeData(NodeType type)
    {
        if (!_isReady)
        {
            DebugTool.Error("Data Container Not Ready", DebugType.Node, this);
            return null;
        }
        
        bool verification = _node_dict.TryGetValue(type, out NodeSO data);
        if (!verification)
        {
            DebugTool.Error($"Node Data Not Found: {type}", DebugType.Node, this);
            return null;
        }
        return data;
    }
    
    public EventSO GetEventData(NodeEventType type)
    {
        if (!_isReady)
        {
            DebugTool.Error("Data Container Not Ready", DebugType.Node, this);
            return null;
        }
        
        bool verification = _event_dict.TryGetValue(type, out EventSO data);
        if (!verification)
        {
            DebugTool.Error($"Event Data Not Found: {type}", DebugType.Node, this);
            return null;
        }
        return data;
    }
    
    public TreeSO GetTreeData(NodeDifficulty difficulty)
    {
        if (!_isReady)
        {
            DebugTool.Error("Data Container Not Ready", DebugType.Node, this);
            return null;
        }
        
        bool verification = _tree_dict.TryGetValue(difficulty, out TreeSO data);
        if (!verification)
        {
            DebugTool.Error($"Node Data Not Found: {difficulty}", DebugType.Node, this);
            return null;
        }
        return data;
    }
}
