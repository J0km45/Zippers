using UnityEngine;

/// <summary>
/// 노드에 플레이어가 진입 하면 발동되는 상황 개별 단위를 Event로 칭함<br/>
/// Event는 상태 패턴으로 구현<br/>
/// Event 상태 패턴 구현을 위한 추상클래스
/// </summary>
public abstract class NodeEvent : MonoBehaviour
{
    private MapEventController _controller;

    public abstract void EventEnter();
    public abstract void EventUpdate();
    public abstract void EventExit();

    public void SetController(MapEventController controller)
    {
        _controller = controller;
    }
}
