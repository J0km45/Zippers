//------------------------------------------
// Test Node의 Action을 Node State별로 정의
//------------------------------------------

#region Ready State Action

/// <summary>
/// Test Node의 Ready State일 때 Action
/// </summary>
public class TestNodeReadyAction : INodeAction
{
    private MapActionController _controller;
    
    public TestNodeReadyAction(MapActionController controller)
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
/// Test Node의 Battle State일 때 Action
/// </summary>
public class TestNodeBattleAction : INodeAction
{
    private MapActionController _controller;
    
    public TestNodeBattleAction(MapActionController controller)
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
/// Test Node의 Clear State일 때 Action
/// </summary>
public class TestNodeClearAction : INodeAction
{
    private MapActionController _controller;
    
    public TestNodeClearAction(MapActionController controller)
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


