//------------------------------------------
// Empty Node의 Action을 Node State별로 정의
//------------------------------------------

#region Ready State Action

/// <summary>
/// Empty Node의 Ready State일 때 Action
/// </summary>
public class EmptyNodeReadyAction : INodeAction
{
    private MapActionController _controller;
    
    public EmptyNodeReadyAction(MapActionController controller)
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
/// Empty Node의 Battle State일 때 Action
/// </summary>
public class EmptyNodeBattleAction : INodeAction
{
    private MapActionController _controller;
    
    public EmptyNodeBattleAction(MapActionController controller)
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
/// Empty Node의 Clear State일 때 Action
/// </summary>
public class EmptyNodeClearAction : INodeAction
{
    private MapActionController _controller;
    
    public EmptyNodeClearAction(MapActionController controller)
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


