using UnityEngine;

/// <summary>
/// 게임 클리어 처리하는 이벤트
/// </summary>
[CreateAssetMenu(fileName = "GameClearEvent SO", menuName = "Node Data/Event Data/Game Clear Event SO")]
public class GameClearEventSO : EventSO
{
    public override void EventEnter()
    {
        _controller.Controller.Manager.GameClearCanvas.gameObject.SetActive(true);
    }

    public override void EventUpdate()
    {
        
    }

    public override void EventExit()
    {
        
    }
}
