/// <summary>
/// 현재 진행중인 Event가 없음<br/>
/// 기본 Event 상태
/// </summary>
public class NoEvent : INodeEvent
{
    private MapEventController _controller;
    
    public NoEvent(MapEventController controller)
    {
        _controller = controller;
    }
    
    public void EventEnter()
    {
        
    }

    public void EventUpdate()
    {
        
    }

    public void EventExit()
    {
        
    }
}
