/// <summary>
/// 노드의 시작 방향을 표기하는 Enum<br/>
/// Default는 Down으로 처리 할 것(기획 상 하단에서 시작하는것이 제일 자연 스러움)
/// </summary>
public enum NodeStartDir
{
    Up,     // 위쪽에서 시작
    Down,   // 아래쪽에서 시작
    Left,   // 왼쪽에서 시작
    Right,  // 오른쪽에서 시작
    Special // 특수 시작점
}