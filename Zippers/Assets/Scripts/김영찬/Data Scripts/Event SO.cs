using UnityEngine;

/// <summary>
/// 노드의 이벤트를 저장하기 위한 SO
/// </summary>
[CreateAssetMenu(fileName = "EventSO", menuName = "Node Data/Event SO")]
public class EventSO : ScriptableObject
{
    /// <summary>
    /// 이벤트 타입
    /// </summary>
    [field:SerializeField] public NodeEventType EventType { get; private set; }
    
    /// <summary>
    /// 이벤트 인덱스
    /// </summary>
    [field:SerializeField] public int EventIndex { get; private set; }
    
    /// <summary>
    /// 연결된 이벤트 스크립트
    /// </summary>
    [field:SerializeField] public NodeEvent EventScript { get; private set; }
    
    public NodeEvent SetEventScript(MapEventController controller)
    {
        NodeEvent temp = EventScript;
        temp.SetController(controller);
        return temp;
    }
}
