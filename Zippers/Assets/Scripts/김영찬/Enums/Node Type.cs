/// <summary>
/// 노드의 Type을 결정하는 Enum<br/>
/// 각 노드는 정해진 Type을 가진다
/// </summary>
public enum NodeType
{
    Empty = -2 ,    // 빈 노드(더미노드)
    Test = -1,      // 테스트 노드
    Start = 0,      // 시작 노드
    Battle,         // 전투 노드
    Shop,           // 상점 노드
    Boss,           // 보스전투 노드
    Escape          // 탈출 노드(멀티 플레이 루프 종료 지점)
}
