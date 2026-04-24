using UnityEngine;

/// <summary>
/// 노드 현황을 관리 하고, Node Tree Maker와 Map Maker의 동작을 제어
/// </summary>
public class NodeManager : MonoBehaviour
{
    // Awake때 NodeSO를 종류 별로 NetworkList<Node>에 분할해서 리스트화
    // NodeTreeMaker에서 노드 트리 생성 시 리스트에 있는 노드를 "복제"하여 전달
}
