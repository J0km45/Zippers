using System;
using System.Collections.Generic;

/// <summary>
/// 정해진 규칙에 따라 노드르리를 생성
/// </summary>
public class NodeTreeMaker
{
    private NodeManager _manager;

    private TreeSO _1stTree;
    private TreeSO _2ndTree;
    private TreeSO _3rdTree;
    private TreeSO _4thTree;
    private TreeSO _5thTree;
    
    Dictionary<(int gridLine, int gridIndex), NodeSO> _nodeTree = new();
    
    public event Action<Dictionary<(int,int), NodeSO>> OnTreeMakingComplete;

    public NodeTreeMaker(NodeManager manager)
    {
        _manager = manager;
    }

    /// <summary>
    /// 지정된 난이도에 맞게 노드 트리 설정
    /// </summary>
    /// <param name="difficulty">난이도</param>
    public void SetNodeTree(NodeDifficulty difficulty)
    {
        TreeDictionary.Instance.GetTreeData(difficulty, out _1stTree,out _2ndTree, out _3rdTree, out _4thTree,out _5thTree);
        
        MakeNodeTree();
        
        OnTreeMakingComplete?.Invoke(_nodeTree);
        DebugTool.Log("Node Tree Setting Complete", DebugType.Node);
    }

    private void MakeNodeTree()
    {
        HashSet<(int gridLine, int gridIndex)> usedGrid = new HashSet<(int,int)>();
        
        if (_1stTree != null)
        {
            for (int i = 0; i < 11; i++)
            {
                _nodeTree.Add((0, i), NodeDictionary.Instance.GetNodeData(_1stTree.LeftSideNodeTreeData[i]));
                if(_1stTree.LeftSideNodeTreeData[i] != NodeType.Empty) usedGrid.Add((0, i));
                if(_1stTree.LeftSideNodeTreeData[i] == NodeType.Escape) break;
            }
            
            for (int i = 0; i < 11; i++)
            {
                _nodeTree.Add((1, i), NodeDictionary.Instance.GetNodeData(_1stTree.UpsideTreeData[i]));
                if(_1stTree.UpsideTreeData[i] != NodeType.Empty) usedGrid.Add((1, i));
                if(_1stTree.UpsideTreeData[i] == NodeType.Escape) break;
            }
            
            for (int i = 0; i < 11; i++)
            {
                _nodeTree.Add((2, i), NodeDictionary.Instance.GetNodeData(_1stTree.RightSideNodeTreeData[i]));
                if(_1stTree.RightSideNodeTreeData[i] != NodeType.Empty) usedGrid.Add((2, i));
                if(_1stTree.RightSideNodeTreeData[i] == NodeType.Escape) break;
            }
        }

        if (_2ndTree != null)
        {
            for (int i = 0; i < 11; i++)
            {
                if(usedGrid.Contains((2, i))) continue;
                _nodeTree[(2, i)] = NodeDictionary.Instance.GetNodeData(_2ndTree.LeftSideNodeTreeData[i]);
                if(_2ndTree.LeftSideNodeTreeData[i] != NodeType.Empty) usedGrid.Add((2, i));
                if(_2ndTree.LeftSideNodeTreeData[i] == NodeType.Escape) break;
            }
            
            for (int i = 0; i < 11; i++)
            {
                _nodeTree.Add((3, i), NodeDictionary.Instance.GetNodeData(_2ndTree.UpsideTreeData[i]));
                if(_2ndTree.UpsideTreeData[i] != NodeType.Empty) usedGrid.Add((3, i));
                if(_2ndTree.UpsideTreeData[i] == NodeType.Escape) break;
            }
            
            for (int i = 0; i < 11; i++)
            {
                _nodeTree.Add((4, i), NodeDictionary.Instance.GetNodeData(_2ndTree.RightSideNodeTreeData[i]));
                if(_2ndTree.RightSideNodeTreeData[i] != NodeType.Empty) usedGrid.Add((4, i));
                if(_2ndTree.RightSideNodeTreeData[i] == NodeType.Escape) break;
            }
        }
        
        if (_3rdTree != null)
        {
            for (int i = 0; i < 11; i++)
            {
                if(usedGrid.Contains((4, i))) continue;
                _nodeTree[(4, i)] = NodeDictionary.Instance.GetNodeData(_3rdTree.LeftSideNodeTreeData[i]);
                if(_3rdTree.LeftSideNodeTreeData[i] != NodeType.Empty) usedGrid.Add((4, i));
                if(_3rdTree.LeftSideNodeTreeData[i] == NodeType.Escape) break;
            }
            
            for (int i = 0; i < 11; i++)
            {
                _nodeTree.Add((5, i), NodeDictionary.Instance.GetNodeData(_3rdTree.UpsideTreeData[i]));
                if(_3rdTree.UpsideTreeData[i] != NodeType.Empty) usedGrid.Add((5, i));
                if(_3rdTree.UpsideTreeData[i] == NodeType.Escape) break;
            }
            
            for (int i = 0; i < 11; i++)
            {
                _nodeTree.Add((6, i), NodeDictionary.Instance.GetNodeData(_3rdTree.RightSideNodeTreeData[i]));
                if(_3rdTree.RightSideNodeTreeData[i] != NodeType.Empty) usedGrid.Add((6, i));
                if(_3rdTree.RightSideNodeTreeData[i] == NodeType.Escape) break;
            }
        }
        
        if (_4thTree != null)
        {
            for (int i = 0; i < 11; i++)
            {
                if(usedGrid.Contains((6, i))) continue;
                _nodeTree[(6, i)] = NodeDictionary.Instance.GetNodeData(_4thTree.LeftSideNodeTreeData[i]);
                if(_4thTree.LeftSideNodeTreeData[i] != NodeType.Empty) usedGrid.Add((6, i));
                if(_4thTree.LeftSideNodeTreeData[i] == NodeType.Escape) break;
            }
            
            for (int i = 0; i < 11; i++)
            {
                _nodeTree.Add((7, i), NodeDictionary.Instance.GetNodeData(_4thTree.UpsideTreeData[i]));
                if(_4thTree.UpsideTreeData[i] != NodeType.Empty) usedGrid.Add((7, i));
                if(_4thTree.UpsideTreeData[i] == NodeType.Escape) break;
            }
            
            for (int i = 0; i < 11; i++)
            {
                _nodeTree.Add((8, i), NodeDictionary.Instance.GetNodeData(_4thTree.RightSideNodeTreeData[i]));
                if(_4thTree.RightSideNodeTreeData[i] != NodeType.Empty) usedGrid.Add((8, i));
                if(_4thTree.RightSideNodeTreeData[i] == NodeType.Escape) break;
            }
        }
        
        if (_5thTree != null)
        {
            for (int i = 0; i < 11; i++)
            {
                if(usedGrid.Contains((8, i))) continue;
                _nodeTree[(8, i)] = NodeDictionary.Instance.GetNodeData(_5thTree.LeftSideNodeTreeData[i]);
                if(_5thTree.LeftSideNodeTreeData[i] != NodeType.Empty) usedGrid.Add((8, i));
                if(_5thTree.LeftSideNodeTreeData[i] == NodeType.Escape) break;
            }
            
            for (int i = 0; i < 11; i++)
            {
                _nodeTree.Add((9, i), NodeDictionary.Instance.GetNodeData(_5thTree.UpsideTreeData[i]));
                if(_5thTree.UpsideTreeData[i] != NodeType.Empty) usedGrid.Add((9, i));
                if(_5thTree.UpsideTreeData[i] == NodeType.Escape) break;
            }
            
            for (int i = 0; i < 11; i++)
            {
                _nodeTree.Add((10, i), NodeDictionary.Instance.GetNodeData(_5thTree.RightSideNodeTreeData[i]));
                if(_5thTree.RightSideNodeTreeData[i] != NodeType.Empty) usedGrid.Add((10, i));
                if(_5thTree.RightSideNodeTreeData[i] == NodeType.Escape) break;
            }
        }
    }
}
