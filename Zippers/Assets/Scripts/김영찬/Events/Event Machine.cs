using System;

/// <summary>
/// State Patten 기반 노드 이벤트 컨트롤러
/// </summary>
public class EventMachine
{
    EventSO _nodeEvent;
    
    public event Action<EventSO> OnEventChangeSendPostEvent;
    
    /// <summary>
    /// 노드의 이벤트 상태를 전환
    /// </summary>
    /// <param name="nodeEvent">전환 될 노드 이벤트</param>
    public void ChangeEvent(EventSO nodeEvent)
    {
        _nodeEvent?.EventExit();
        if(_nodeEvent != null) OnEventChangeSendPostEvent?.Invoke(_nodeEvent);
        _nodeEvent = nodeEvent;
        _nodeEvent?.EventEnter();
    }
    
    /// <summary>
    /// 노드이벤트의 Update 함수를 유니티 이벤트 함수와 연결
    /// </summary>
    public void EventUpdate()
    {
        _nodeEvent?.EventUpdate();
    }
}
