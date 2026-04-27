using System;
using UnityEngine;

/// <summary>
/// 맵이 어떻게 동작하는지 제어하는 역할
/// </summary>
public class MapController : MonoBehaviour
{
    /// <summary>
    /// MapController에서 사용하는 MapData 변수
    /// </summary>
    public MapData Data { get; private set; }
    
    /// <summary>
    /// MapController에서 사용하는 MapActionController 변수
    /// </summary>
    public MapActionController ActionController { get; private set; }
    
    /// <summary>
    /// MapController에서 사용하는 MapEventController 변수
    /// </summary>
    public MapEventController EventController { get; private set; }
    
    /// <summary>
    /// MapController에서 사용하는 EventDictionary 변수
    /// </summary>
    public EventDictionary EventDictionary { get; private set; }
    
    /// <summary>
    /// MapController에서 사용하는 NextMapTeleporter 변수
    /// </summary>
    public NextMapTeleporter Teleporter { get; private set; }
    
    /// <summary>
    /// MapController에서 사용하는 MapMaker 변수
    /// </summary>
    public MapMaker MapMaker { get; private set; }

    private void Awake()
    {
        Init();
    }
    
    private void OnEnable()
    {
        EventEnable();
    }

    private void OnDisable()
    {
        EventDisable();
    }

    private void Start()
    {
        InitController();
        Data.SetNodeState(NodeState.Ready);
        EventController.SetCurrentEvent(NodeEventType.NoEvent, 0);
        Teleporter.DisableBeaconAll();
    }

    private void Update()
    {
        UpdateController();
    }

    private void Init()
    {
        Data = GetComponent<MapData>();
        Teleporter = GetComponent<NextMapTeleporter>();
        MapMaker = FindFirstObjectByType<MapMaker>();
        EventDictionary = FindFirstObjectByType<EventDictionary>();
        ActionController = new MapActionController(this);
        EventController = new MapEventController(this);
    }

    private void InitController()
    {
        ActionController.InitActionController();
        EventController.InitEventController();
    }

    private void UpdateController()
    {
        ActionController.Update();
        EventController.Update();
    }
    
    private void EventEnable()
    {
        Data.OnChangeState += ActionController.ChangeState;
    }

    private void EventDisable()
    {
        Data.OnChangeState -= ActionController.ChangeState;
    }

    
}
