using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Event SO를 목록화 하고 검색
/// </summary>
public class EventDictionary : MonoBehaviour
{
    /// <summary>
    /// SingleTon Instance
    /// </summary>
    public static EventDictionary Instance { get; private set; }

    [Header("No Event 데이터")]
    [SerializeField] private EventSO _noEvent;
    
    [Header("No Event 제외한 나머지 데이터들")]
    [SerializeField] private EventSO[] _events;
    
    private Dictionary<int, EventSO> _dict_MonsterSpawn;
    private Dictionary<int, EventSO> _dict_MonsterEnhance;
    private Dictionary<int, EventSO> _dict_SupplyItem;
    
    public bool IsDictionaryReady { get; private set; }

    private void Awake()
    {
        SetSingleTon();
        IsDictionaryReady = false;
        InitDict();
    }

    private void SetSingleTon()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void InitDict()
    {
        _dict_MonsterEnhance = new Dictionary<int, EventSO>();
        _dict_MonsterSpawn = new Dictionary<int, EventSO>();
        _dict_SupplyItem = new Dictionary<int, EventSO>();
        
        foreach (EventSO @event in _events)
        {
            switch (@event.EventType)
            {
                case NodeEventType.MonsterSpawn:
                    bool tempMS = _dict_MonsterSpawn.TryAdd(@event.EventIndex, @event);
                    if(!tempMS) DebugTool.Error($"EventSO Index Duplicate : {@event.EventType}_{@event.EventIndex}", DebugType.Node, this);
                    break;
                case NodeEventType.MonsterEnhance:
                    bool tempME = _dict_MonsterEnhance.TryAdd(@event.EventIndex, @event);
                    if(!tempME) DebugTool.Error($"EventSO Index Duplicate : {@event.EventType}_{@event.EventIndex}", DebugType.Node, this);
                    break;
                case NodeEventType.SupplyItem:
                    bool tempSI = _dict_SupplyItem.TryAdd(@event.EventIndex, @event);
                    if(!tempSI) DebugTool.Error($"EventSO Index Duplicate : {@event.EventType}_{@event.EventIndex}", DebugType.Node, this);
                    break;
            }
        }
        IsDictionaryReady = true;
        
        DebugTool.Log("Event Dictionary Ready", DebugType.Node, this);
    }
    
    /// <summary>
    /// 노드 데이터 불러오기
    /// </summary>
    /// <param name="eventType">이벤트 타입</param>
    /// <param name="index">이벤트 인덱스</param>
    /// <returns>EventSO 형태 반환</returns>
    public EventSO CallEvent(NodeEventType eventType, int index)
    {
        if (!IsDictionaryReady)
        {
            DebugTool.Error("Event Dictionary Not Ready", DebugType.Node, this);
            return null;
        }
        
        EventSO result;

        switch (eventType)
        {
            case NodeEventType.NoEvent:
                result = _noEvent;
                break;
            case NodeEventType.MonsterSpawn:
                result = _dict_MonsterSpawn.GetValueOrDefault(index);
                if (result == null) DebugTool.Error($"Event Not Found : {eventType}_{index}", DebugType.Node, this); 
                break;
            case NodeEventType.MonsterEnhance:
                result = _dict_MonsterEnhance.GetValueOrDefault(index);
                if (result == null) DebugTool.Error($"Event Not Found : {eventType}_{index}", DebugType.Node, this); 
                break;
            case NodeEventType.SupplyItem:
                result = _dict_SupplyItem.GetValueOrDefault(index);
                if (result == null) DebugTool.Error($"Event Not Found : {eventType}_{index}", DebugType.Node, this); 
                break;
            default:
                DebugTool.Error($"Event Not Found : {eventType}_{index}", DebugType.Node, this); 
                return null;
        }
        
        return result;
    }
}
