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
    
    [SerializeField] private EventSO[] _events;

    private Dictionary<(NodeEventType, int), EventSO> _dict;
    
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
        _dict = new Dictionary<(NodeEventType, int), EventSO>();
        
        foreach (EventSO @event in _events)
        {
            bool verification = _dict.TryAdd((@event.EventType, @event.EventIndex), @event);
            if(!verification) DebugTool.Error($"Event Dictionary Duplication Error : {@event.EventType}_{@event.EventIndex}", DebugType.Node, this);
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
        
        EventSO result = _dict.GetValueOrDefault((eventType, index));
        
        if (result == null) DebugTool.Error($"Event Not Found : {eventType}_{index}", DebugType.Node, this);
        
        return result;
    }
}
