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

    /// <summary>
    /// MapEventController에서 사용하는 NoEvent 변수
    /// </summary>
    public INodeEvent NoEvent { get; private set; }
    
    /// <summary>
    /// MapEventController에서 사용하는 MonsterSpawnEvent 변수
    /// </summary>
    public INodeEvent MonsterSpawnEvent { get; private set; }
    
    /// <summary>
    /// MapEventController에서 사용하는 MonsterEnhanceEvent 변수
    /// </summary>
    public INodeEvent MonsterEnhanceEvent { get; private set; }
    
    /// <summary>
    /// MapEventController에서 사용하는 SupplyItemEvent 변수
    /// </summary>
    public INodeEvent SupplyItemEvent { get; private set; }
    
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
        
        NoEvent = new NoEvent(this);
        MonsterEnhanceEvent = new MonsterEnhanceEvent(this);
        MonsterSpawnEvent = new MonsterSpawnEvent(this);
        SupplyItemEvent = new SupplyItemEvent(this);
    }

    /// <summary>
    /// 이벤트 변경
    /// </summary>
    /// <param name="nodeEvent">EventController.{eventName} 사용</param>
    public void ChangeEvent(INodeEvent nodeEvent)
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
}
