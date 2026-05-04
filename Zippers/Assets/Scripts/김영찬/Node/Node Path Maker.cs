using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

/// <summary>
/// Tree SO 기반으로 연결 순서 지정
/// </summary>
public class NodePathMaker
{
    private NodeManager _nodeManager;

    public Queue<MapRow> Path { get; private set; }

    private DifficultyDataSO _difficultyData;
    
    private Dictionary<NodeType, int> _weightInfo;
    private int _sumWeight;
    
    public event Action OnPathMakingComplete;
    
    public NodePathMaker(NodeManager nodeManager)
    {
        _nodeManager = nodeManager;
        Path = new();
        
    }

    public void MakePath(NodeDifficulty difficulty)
    {
        _weightInfo = new();
        _sumWeight = 0;
        Path.Clear();
        
        _difficultyData = _nodeManager.DataContainer.GetDifficultyData(difficulty);
        
        if (difficulty == NodeDifficulty.Test)
        {
            TestPathWay();
            return;
        }
        
        SumNodeWeight();

        for (int i = 0; i < _difficultyData.PathNodeCount; i++)
        {
            NodeType tempUp = NodeType.Empty;
            NodeType tempLeft = NodeType.Empty;
            NodeType tempRight = NodeType.Empty;
            
            CulNextPathWay(out bool canMoveUpperSide, out bool canMoveLeftSide, out bool canMoveRightSide);
            
            if (i == _difficultyData.PathNodeCount - 2)
            {
                if(canMoveUpperSide) tempUp = NodeType.Boss;
                if(canMoveLeftSide) tempLeft = NodeType.Boss;
                if(canMoveRightSide) tempRight = NodeType.Boss;
            }
            else if (i == _difficultyData.PathNodeCount - 1)
            {
                if(canMoveUpperSide) tempUp = NodeType.Escape;
                if(canMoveLeftSide) tempLeft = NodeType.Escape;
                if(canMoveRightSide) tempRight = NodeType.Escape;
            }
            else
            {
                if(canMoveUpperSide) tempUp = WeightNodeSelection();
                if(canMoveLeftSide) tempLeft = WeightNodeSelection();
                if(canMoveRightSide) tempRight = WeightNodeSelection();
            }
            
            Path.Enqueue(new MapRow(tempLeft, tempUp, tempRight));
        }
        
        DebugTool.Log("PathMaking Complete", DebugType.Node);
        OnPathMakingComplete?.Invoke();
    }
    
    private void CulNextPathWay(out bool canMoveUpperSide, out bool canMoveLeftSide, out bool canMoveRightSide)
    {
        bool tempMoveUpperSide = Random.Range(0f, 1f) <= _difficultyData.UpsideChance;
        bool tempMoveLeftSide = Random.Range(0f, 1f) <= _difficultyData.LeftSideChance;
        bool tempMoveRightSide = Random.Range(0f, 1f) <= _difficultyData.RightSideChance;
        
        canMoveUpperSide = tempMoveUpperSide;
        canMoveLeftSide = tempMoveLeftSide;
        canMoveRightSide = tempMoveRightSide;
        
        if (!tempMoveLeftSide && !tempMoveRightSide && !tempMoveUpperSide)
        {
            canMoveUpperSide = true;
        }
    }
    
    private void TestPathWay()
    {
        Path.Enqueue(new MapRow(NodeType.Empty, NodeType.Battle, NodeType.Empty));
        Path.Enqueue(new MapRow(NodeType.Empty, NodeType.Shop, NodeType.Empty));
        Path.Enqueue(new MapRow(NodeType.Empty, NodeType.Battle, NodeType.Empty));
        Path.Enqueue(new MapRow(NodeType.Empty, NodeType.Shop, NodeType.Empty));
        Path.Enqueue(new MapRow(NodeType.Empty, NodeType.Boss, NodeType.Empty));
        Path.Enqueue(new MapRow(NodeType.Empty, NodeType.Escape, NodeType.Empty));
    }

    private void SumNodeWeight()
    {
        if(_difficultyData.WeightData == null) return;
        
        foreach (NodeTypeWeight weightData in _difficultyData.WeightData)
        {
            bool verification = _weightInfo.TryAdd(weightData.NodeType, weightData.Weight);
            if(verification) _sumWeight += weightData.Weight;
        }
    }

    private NodeType WeightNodeSelection()
    {
        if(_weightInfo == null || _weightInfo.Count == 0) return NodeType.Battle;
        
        int selectedNumber = Random.Range(0, _sumWeight);
        
        foreach (var weightInfo in _weightInfo)
        {
            if (selectedNumber < weightInfo.Value) return weightInfo.Key;
            
            selectedNumber -= weightInfo.Value;
        }
        
        return NodeType.Battle;
    }
}
