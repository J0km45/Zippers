using System;
using UnityEngine;

/// <summary>
/// 노드의 Data를 저장하기 위한 SO<br/>
/// 맵의 프리팹과 연결되어 있다.
/// </summary>
[CreateAssetMenu(fileName = "NodeSO", menuName = "Node Data/Node SO")]
public class NodeSO : ScriptableObject
{
    #region 노드 설정 변수

    /// <summary>
    /// 노드 타입
    /// </summary>
    [Tooltip("노드 타입")]
    [field:SerializeField] public NodeType NodeType {get; private set;}
    
    /// <summary>
    /// 노드의 식별 번호
    /// </summary>
    [Tooltip("노드의 식별 번호")]
    [field:SerializeField] public int NodeID {get; private set;}
    
    /// <summary>
    /// 이 노드에 해당 되는 Map 프리팹
    /// </summary>
    [Tooltip("이 노드에 해당 되는 Map 프리팹")]
    [field:SerializeField] public GameObject Map {get; private set;}

    #endregion

    private void Awake()
    {
        SetMapData();
    }

    private void SetMapData()
    {
        MapData data = Map.GetComponent<MapData>();
        if (data != null) data.SetNodeType(NodeType);
    }
}
