//------------------------------------------
// Escape Node의 Action을 Node State별로 정의
//------------------------------------------

#region Ready State Action

/// <summary>
/// Escape Node의 Ready State일 때 Action
/// </summary>
public class EscapeNodeReadyAction : INodeAction
{
    private MapActionController _controller;
    
    public EscapeNodeReadyAction(MapActionController controller)
    {
        _controller = controller;
    }
    
    public void EnterState()
    {
        _controller.Controller.EventController.SetCurrentEvent(NodeEventType.PlayerCheck,0);
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
/// Escape Node의 Battle State일 때 Action
/// </summary>
public class EscapeNodeBattleAction : INodeAction
{
    private MapActionController _controller;
    
    public EscapeNodeBattleAction(MapActionController controller)
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
/// Escape Node의 Clear State일 때 Action
/// </summary>
public class EscapeNodeClearAction : INodeAction
{
    private MapActionController _controller;
    
    public EscapeNodeClearAction(MapActionController controller)
    {
        _controller = controller;
    }
    
    public void EnterState()
    {
        _controller.Controller.EventController.SetCurrentEvent(NodeEventType.GameClear,0);
    }

    public void RunningState()
    {
        
    }

    public void ExitState()
    {
        
    }
}

#endregion


