using UnityEngine;

/// <summary>
/// 노드의 이벤트를 저장하기 위한 SO
/// </summary>
[CreateAssetMenu(fileName = "Event SO", menuName = "Node Data/Event SO")]
public class EventSO : ScriptableObject
{
    /// <summary>
    /// 이벤트 타입
    /// </summary>
    [field:SerializeField] public NodeEventType EventType { get; private set; }
    
    /// <summary>
    /// 이벤트 인덱스<br/>
    /// 이벤트 타입 별로 0부터 시작
    /// </summary>
    [field:SerializeField] public int EventIndex { get; private set; }
    
    /// <summary>
    /// 연결된 이벤트 스크립트
    /// </summary>
    [field:SerializeField] public NodeEvent EventScript { get; private set; }
    
    /// <summary>
    /// 이벤트 데이터 불러오기
    /// </summary>
    /// <param name="controller">제어 할 이벤트 컨트롤러</param>
    /// <returns>Node Event 추상 클래스를 가지는 자식 클래스 반환</returns>
    public NodeEvent GetEventScript(MapEventController controller)
    {
        if (EventScript == null)
        {
            DebugTool.Warning($"Not SerializeField EventScript : {EventType}_{EventIndex}", DebugType.Node, this);
            return null;
        }
        NodeEvent temp = EventScript;
        temp.SetController(controller);
        return temp;
    }
}
