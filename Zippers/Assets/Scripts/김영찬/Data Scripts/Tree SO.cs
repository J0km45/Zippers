using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 노드 트리의 순서를 저장하는 SO
/// </summary>
[CreateAssetMenu(fileName = "Tree SO", menuName = "Node Data/Tree SO")]
public class TreeSO : ScriptableObject
{
    /// <summary>
    /// 노드 난이도
    /// </summary>
    [field: SerializeField] public NodeDifficulty Difficulty { get; private set; }
    
    /// <summary>
    /// 노드 트리 순서 ID<br/>
    /// 반드시 0번부터 채울것
    /// </summary>
    [field: SerializeField] public int TreeIndex { get; private set; }
    
    [Header("기본 인덱스")]
    [field: SerializeField] public NodeType[] UpsideTreeData { get; private set; }
    
    [Header("왼쪽으로 추가")]
    [field: SerializeField] public NodeType[] LeftSideNodeTreeData { get; private set; }

    [Header("오른쪽으로 추가")]
    [field: SerializeField] public NodeType[] RightSideNodeTreeData { get; private set; }
}
