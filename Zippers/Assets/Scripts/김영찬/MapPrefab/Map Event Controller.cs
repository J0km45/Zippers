using UnityEngine;

/// <summary>
/// 맵에 발동되는 이벤트 제어
/// </summary>
public class MapEventController
{
    /// <summary>
    /// MapEventController에서 사용하는 MapController 변수
    /// </summary>
    public MapController Controller { get; private set; }
    private EventMachine _machine;

    private NodeEvent _currentEvent;
    
    public MapEventController(MapController controller)
    {
        Controller = controller;
    }

    /// <summary>
    /// MapEventController 초기 세팅
    /// </summary>
    public void InitEventController()
    {
        _machine = new EventMachine();
    }

    private void ChangeEvent(NodeEvent nodeEvent)
    {
        _machine.ChangeEvent(nodeEvent);
    }

    /// <summary>
    /// EventMachine의 EventUpdate를 유니티 Update에 올리기 위함
    /// </summary>
    public void Update()
    {
        _machine.EventUpdate();
    }

    /// <summary>
    /// 현재 구동중인 이벤트 설정
    /// </summary>
    /// <param name="eventType">NodeEventType enum을 지정</param>
    /// <param name="index">1번부터 시작, No Event만 0번으로 지정</param>
    public void SetCurrentEvent(NodeEventType eventType, int index)
    {
        EventSO temp = Controller.EventDictionary.CallEvent(eventType, index);
        _currentEvent = temp.SetEventScript(this);
        ChangeEvent(_currentEvent);
    }
}
