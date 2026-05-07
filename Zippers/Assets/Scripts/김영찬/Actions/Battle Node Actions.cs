//------------------------------------------
// Battle Node의 Action을 Node State별로 정의
//------------------------------------------

#region Ready State Action

/// <summary>
/// Battle Node의 Ready State일 때 Action
/// </summary>
public class BattleNodeReadyAction : INodeAction
{
    private MapActionController _controller;
    
    public BattleNodeReadyAction(MapActionController controller)
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
/// Battle Node의 Battle State일 때 Action
/// </summary>
public class BattleNodeBattleAction : INodeAction
{
    private MapActionController _controller;
    
    public BattleNodeBattleAction(MapActionController controller)
    {
        _controller = controller;
    }
    
    public void EnterState()
    {
        _controller.Controller.EventController.SetCurrentEvent(NodeEventType.MonsterSpawn,0);
        _controller.Controller.Data.OnChangeAlivePlayerCount += GameOver;
    }

    public void RunningState()
    {
        
    }

    public void ExitState()
    {
        _controller.Controller.Data.OnChangeAlivePlayerCount -= GameOver;
    }
    
    private void GameOver(int count)
    {
        if(count <= 0) _controller.Controller.EventController.SetCurrentEvent(NodeEventType.GameOver,0);
    }
}

#endregion

#region Clear State Action

/// <summary>
/// Battle Node의 Clear State일 때 Action
/// </summary>
public class BattleNodeClearAction : INodeAction
{
    private MapActionController _controller;
    
    public BattleNodeClearAction(MapActionController controller)
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


