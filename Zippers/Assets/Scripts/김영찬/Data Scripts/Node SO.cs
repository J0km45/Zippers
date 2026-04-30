using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// 노드의 Data를 저장하기 위한 SO<br/>
/// 맵의 프리팹과 연결되어 있다.
/// </summary>
[CreateAssetMenu(fileName = "Node SO", menuName = "Node Data/Node SO")]
public class NodeSO : ScriptableObject
{
    [Header("노드 타입")] 
    [SerializeField] private NodeType _nodeType;

    [Header("UI")]
    [SerializeField] private Image _nodeImage;
    
    /// <summary>
    /// 노드 타입
    /// </summary>
    public NodeType NodeType => _nodeType;
    
    /// <summary>
    /// 맵UI에서 보여지는 노드의 Icon Image
    /// </summary>
    public Image NodeImage => _nodeImage;

    // TODO : 이 이하는 노드 맵 UI 작업하실 때 필요한 부분 수정해 주세요
}
