//------------------------------------------
// Battle Node의 Action을 Node State별로 정의
//------------------------------------------

#region Ready State Action

using Unity.Netcode;

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
        SessionPlayerStateController.Instance.SessionAlivePlayerCount.OnValueChanged += GameOver;
        _controller.Controller.WaveManager.OnBattleNodeCleared += ToClearState;

    }

    public void RunningState()
    {
        
    }

    public void ExitState()
    {
        SessionPlayerStateController.Instance.SessionAlivePlayerCount.OnValueChanged -= GameOver;
        _controller.Controller.WaveManager.OnBattleNodeCleared -= ToClearState;
    }
    
    private void GameOver(int preCount ,int curCount)
    {
        if(curCount <= 0) _controller.Controller.EventController.SetCurrentEvent(NodeEventType.GameOver,0);
    }

    private void ToClearState(int number)
    {
        _controller.Controller.Data.NetworkMapData.SetNodeState(NodeState.Clear);
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


