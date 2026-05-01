using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 노드의 이벤트를 저장하기 위한 SO
/// </summary>
[CreateAssetMenu(fileName = "Event Container SO", menuName = "Node Data/Event Container SO")]
public class EventContainerSO : ScriptableObject
{
    [Header("이벤트 타입")] 
    [SerializeField] private NodeEventType _eventType;
    
    [Header("이 이벤트 타입에 해당하는 스크립트 ")]
    [SerializeField] private EventSO[] _eventData;
    
    /// <summary>
    /// 이벤트 타입
    /// </summary>
    public NodeEventType EventType => _eventType;
    
    /// <summary>
    /// 인덱스에 해당되는 이벤트 스크립트 호출
    /// </summary>
    /// <param name="index">호출할 이벤트의 인덱스</param>
    /// <param name="controller">이벤트가 실행되어야 되는 MapEventController</param>
    /// <returns>NodeEvent 추상 클래스의 자식 스크립트 반환</returns>
    public EventSO GetEventData(int index, MapEventController controller)
    {
        if (_eventData == null)
        {
            DebugTool.Warning($"Not SerializeField EventScript : {_eventType}_{index}", DebugType.Node, this);
            return null;
        }
        
        if (index < 0)
        {
            DebugTool.Error($"Index Wrong range : {index}", DebugType.Node, this);
            return null;
        }
        
        if (index > _eventData.Length - 1)
        {
            DebugTool.Error($"Index out of range : {_eventType}_{index}", DebugType.Node, this);
            return null;
        }
        
        EventSO data = Instantiate(_eventData[index]);
        data.SetController(controller);
        return data;
    }
}
