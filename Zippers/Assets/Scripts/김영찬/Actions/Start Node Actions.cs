//------------------------------------------
// Start Node의 Action을 Node State별로 정의
//------------------------------------------

#region Ready State Action

/// <summary>
/// Start Node의 Ready State일 때 Action.
/// StartMap 은 전투 / 플레이어 체크 필요 없이 입장 직후 바로 다음 노드 선택 단계 (Clear) 로 진입.
/// </summary>
public class StartNodeReadyAction : INodeAction
{
    private MapActionController _controller;

    public StartNodeReadyAction(MapActionController controller)
    {
        _controller = controller;
    }

    public void EnterState()
    {
        // StartMap: PlayerCheckEvent 없이 즉시 Clear 상태로 전환.
        // → StartNodeClearAction → VoteSetting → 비콘 활성화 → 텔레포트 가능
        // SetNodeState 내부에 IsServer 가드 있어 비호스트에선 자동 무시.
        _controller.Controller.Data.NetworkMapData.SetNodeState(NodeState.Clear);
    }

    public void RunningState()
    {

    }

    public void ExitState()
    {

    }
}

#endregion

#region Battle State Action

/// <summary>
/// Start Node의 Battle State일 때 Action
/// </summary>
public class StartNodeBattleAction : INodeAction
{
    private MapActionController _controller;
    
    public StartNodeBattleAction(MapActionController controller)
    {
        _controller = controller;
    }
    
    public void EnterState()
    {
        _controller.Controller.Data.NetworkMapData.SetNodeState(NodeState.Clear);
    }

    public void RunningState()
    {
        
    }

    public void ExitState()
    {
        
    }
}

#endregion

#region Clear State Action

/// <summary>
/// Start Node의 Clear State일 때 Action
/// </summary>
public class StartNodeClearAction : INodeAction
{
    private MapActionController _controller;
    
    public StartNodeClearAction(MapActionController controller)
    {
        _controller = controller;
    }
    
    public void EnterState()
    {
        _controller.Controller.EventController.SetCurrentEvent(NodeEventType.VoteSetting,0);
    }

    public void RunningState()
    {
        
    }

    public void ExitState()
    {
        
    }
}

#endregion


