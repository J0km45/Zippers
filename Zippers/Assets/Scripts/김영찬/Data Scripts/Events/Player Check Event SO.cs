using UnityEngine;

/// <summary>
/// Map 입장 시 모든 플레이어가 이동 되어있는지 체크하는 이벤트
/// </summary>
[CreateAssetMenu(fileName = "PlayerCheckEvent SO", menuName = "Node Data/Event Data/Player Check Event SO")]
public class PlayerCheckEventSO : EventSO
{
    public override void EventEnter()
    {
        _controller.Controller.Data.OnChangeAlivePlayerCount += PlayerCheck;
    }

    public override void EventUpdate()
    {
        
    }

    public override void EventExit()
    {
        _controller.Controller.Data.OnChangeAlivePlayerCount -= PlayerCheck;
    }
    
    private void PlayerCheck(int count)
    {
        // ToDo : if(유효 플레이어 수 > count) return;
        _controller.Controller.Data.SetNodeState(NodeState.Battle);
        _controller.SetDefaultEvent();
    }
}
