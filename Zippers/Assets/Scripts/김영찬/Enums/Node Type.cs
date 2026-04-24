/// <summary>
/// 노드의 Type을 결정하는 Enum<br/>
/// 각 노드는 정해진 Type을 가진다
/// </summary>
public enum NodeType
{
    Start,  // 시작 노드(로비의 더미, 밑의 운영방식에 설명)
    Battle, // 전투 노드
    Boss,   // 보스전투 노드
    Shop,   // 상점 노드
    Escape, // 탈출 노드(멀티 플레이 루프 종료 지점)
    Empty   // 빈 노드(분기 설정 시 이동 불가능한 지점에 채워지는 더미노드)
}
