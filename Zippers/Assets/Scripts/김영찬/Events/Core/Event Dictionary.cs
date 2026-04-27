using System;
using System.Collections.Generic;
using UnityEngine;

public class EventDictionary : MonoBehaviour
{
    [SerializeField] private EventSO[] _events;
    
    private Dictionary<string, EventSO> _dict;

    private void Awake()
    {
        foreach (EventSO @event in _events)
        {
            string temp1 = @event.EventType.ToString();
            string temp2 = @event.EventIndex.ToString();
            string key = temp1 + temp2;
            _dict.TryAdd(key, @event);
        }
    }
    
    public EventSO CallEvent(NodeEventType eventType, int index)
    {
        string temp1 = eventType.ToString();
        string temp2 = index.ToString();
        string key = temp1 + temp2;
        
        return _dict.TryGetValue(key, out EventSO temp) ? temp : null;
    }
}
