/// <summary>
/// 노드에 플레이어가 진입 하면 발동되는 상황 개별 단위를 Event로 칭함<br/>
/// Event는 상태 패턴으로 구현<br/>
/// Event 상태 패턴 구현을 위한 interface
/// </summary>
public interface INodeEvent
{
    public void EventEnter();
    public void EventUpdate();
    public void EventExit();
}
