using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GameScene 진입 시 Map을 생성해주는 역할
/// </summary>
public class MapMaker
{
    private NodeManager _manager;
    
    private Dictionary<(int, int), GameObject> _gridMapData;

    public MapMaker(NodeManager manager)
    {
        _manager = manager;
    }
    
    public event Action OnMapSettingComplete; 
    
    /// <summary>
    /// Node Grid Map에 Map 이식
    /// </summary>
    /// <param name="treeData"></param>
    public void SetMap(Dictionary<(int,int), NodeSO> treeData)
    {
        for (int i = 0; i < 11; i++)
        {
            for (int j = 0; j < 11; j++)
            {
                if(!treeData.ContainsKey((i, j))) continue;
                NodeSO tempNodeData = treeData.GetValueOrDefault((i, j));
                if(tempNodeData == null) continue;
                if(tempNodeData.NodeType == NodeType.Empty) continue;
                GameObject tempMap = tempNodeData.GetNodeMap(_manager.NodeGrid.GridMap[i][j]);
                _gridMapData.Add((i, j), tempMap);
            }
        }
        MakePath();
        OnMapSettingComplete?.Invoke();
        DebugTool.Log("Map Making Complete",DebugType.Node);
    }

    private void MakePath()
    {
        for (int i = 0; i < 11; i++)
        {
            for (int j = 0; j < 11; j++)
            {
                if(!_gridMapData.ContainsKey((i,j))) continue;
                MapData tempData = _gridMapData[(i,j)].GetComponent<MapData>();
                tempData.SetNodeTreeIndex(i+(j/100d));
                if(_gridMapData.ContainsKey((i,j+1)))
                {
                    tempData.SetNextMap(NodeStartDir.Up, _gridMapData[(i,j+1)]);
                }

                if (_gridMapData.ContainsKey((i-1,j+1)))
                {
                    tempData.SetNextMap(NodeStartDir.Left, _gridMapData[(i-1,j+1)]);
                }
                
                if (_gridMapData.ContainsKey((i+1,j+1)))
                {
                    tempData.SetNextMap(NodeStartDir.Right, _gridMapData[(i+1,j+1)]);
                }
            }
        }
    }
}
