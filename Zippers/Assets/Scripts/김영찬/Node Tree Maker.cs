using UnityEngine;

/// <summary>
/// 정해진 규칙에 따라 노드르리를 생성
/// </summary>
public class NodeTreeMaker : MonoBehaviour
{
    // Node.NextNodes에 규칙에 따라 노드를 삽입하여 트리 구조 형성
    // Node.NodeTreeIndex를 다음 규칙에 따라 지정
    // - 진행도 3번째, 1번 트리(다른 트리가 없어도 1), 1번 분기(다른 분기가 없어도 1)면 3.11({진행도}.{트리번호}{분기번호})로 지정 
    // - 다중 트리와 분기점은 MVP 후순위
    // Node.NextNodes의 Node.StartDir를 다음 과 같이 지정
    // - 기본 상태는 아래쪽에서 시작(Down) (0번 인덱스)
    // - 분기점이 있다면 왼쪽(Left) (1), 오른쪽(Right) (2), 위쪽(Up) (3), 특수(Special) (4)에서 시작 순서로 지정 (다중 분기는 mvp 후순위)
    // - 트리가 비어있다면 NodeType.Empty를 삽입하여 인덱스 규칙을 유지함
}
