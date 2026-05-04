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
        _controller.Controller.Data.OnChangeAlivePlayerCount += PlayerCheck;
    }

    public void RunningState()
    {
        
    }

    public void ExitState()
    {
        _controller.Controller.Data.OnChangeAlivePlayerCount -= PlayerCheck;
    }

    private void PlayerCheck(int count)
    {
        // ToDo : 차후에 살아있는 전체 플레이어의 숫자를 카운트 하는 변수가 생기면 4 대신 해당 변수에 연결 할 것
        if(count != 4) return; 
        _controller.Controller.Data.SetNodeState(NodeState.Clear);
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
        _controller.Controller.TeleportSupporter.EnableBeaconAvailable();
    }

    public void RunningState()
    {
        
    }

    public void ExitState()
    {
        
    }
}

#endregion


