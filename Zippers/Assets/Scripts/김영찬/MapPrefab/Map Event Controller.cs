/// <summary>
/// 맵에 발동되는 이벤트 제어
/// </summary>
public class MapEventController
{
    /// <summary>
    /// MapEventController에서 사용하는 MapController 변수
    /// </summary>
    public MapController Controller { get; }

    /// <summary>
    /// MapEventController에서 사용하는 EventMachine 변수
    /// </summary>
    public EventMachine Machine { get; private set; }

    private EventSO _currentEvent;
    
    public MapEventController(MapController controller)
    {
        Controller = controller;
    }

    /// <summary>
    /// MapEventController 초기 세팅
    /// </summary>
    public void InitEventController()
    {
        Machine = new EventMachine();
        DebugTool.Log($"{Controller.gameObject.name} Event Controller Ready", DebugType.Node);
    }

    private void ChangeEvent(EventSO nodeEvent)
    {
        Machine.ChangeEvent(nodeEvent);
    }
    
    /// <summary>
    /// EventMachine의 EventUpdate를 유니티 Update에 올리기 위함
    /// </summary>
    public void Update()
    {
        Machine.EventUpdate();
    }

    /// <summary>
    /// 현재 구동중인 이벤트 설정
    /// </summary>
    /// <param name="eventType">NodeEventType enum을 지정</param>
    /// <param name="index">이벤트 타입별 인덱스</param>
    public void SetCurrentEvent(NodeEventType eventType, int index)
    {
        EventContainerSO temp = Controller.Manager.DataContainer.GetEventData(eventType);
        if (temp == null) return;
        _currentEvent = temp.GetEventData(index,this);
        if(_currentEvent == null) return;
        ChangeEvent(_currentEvent);
        DebugTool.Log($"{Controller.gameObject.name} Event Set : {eventType}, {index}", DebugType.Node);
    }

    /// <summary>
    /// 기본 이벤트 상태(No Event)로 회귀
    /// </summary>
    public void SetDefaultEvent()
    {
        SetCurrentEvent(NodeEventType.NoEvent, 0);
    }
}
