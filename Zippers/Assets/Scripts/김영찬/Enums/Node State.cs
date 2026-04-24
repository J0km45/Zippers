/// <summary>
/// 노드의 진행 상황을 정의하는 Enum<br/>
/// Clear 이전까지는 두 상태를 전환 가능함
/// </summary>
public enum NodeState
{
    Ready,  // 플레이어가 전투 준비 중
    Battle, // 플레이어가 전투 중
    Clear   // 플레이어가 해당 노드 클리어
}