//------------------------------------------
// Start Node의 Action을 Node State별로 정의
//------------------------------------------

#region Ready State Action

/// <summary>
/// Start Node의 Ready State일 때 Action
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


