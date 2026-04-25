//------------------------------------------
// Shop Node의 Action을 Node State별로 정의
//------------------------------------------

#region Ready State Action

/// <summary>
/// Shop Node의 Ready State일 때 Action
/// </summary>
public class ShopNodeReadyAction : INodeAction
{
    private MapActionController _controller;
    
    public ShopNodeReadyAction(MapActionController controller)
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
/// Shop Node의 Battle State일 때 Action
/// </summary>
public class ShopNodeBattleAction : INodeAction
{
    private MapActionController _controller;
    
    public ShopNodeBattleAction(MapActionController controller)
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
/// Shop Node의 Clear State일 때 Action
/// </summary>
public class ShopNodeClearAction : INodeAction
{
    private MapActionController _controller;
    
    public ShopNodeClearAction(MapActionController controller)
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


