/// <summary>
/// Map의 Action을 제어
/// </summary>
public class MapActionController
{
    /// <summary>
    /// MapActionController에서 사용하는 MapController 변수
    /// </summary>
    public MapController Controller { get; private set; }
    private ActionMachine _machine;
    
    private INodeAction _readyAction;
    private INodeAction _battleAction;
    private INodeAction _clearAction;
    
    public MapActionController(MapController controller)
    {
        Controller = controller;
    }

    /// <summary>
    /// MapActionController 초기화
    /// </summary>
    public void InitActionController()
    {
        _machine = new ActionMachine();

        switch (Controller.Data.NodeType)
        {
            case NodeType.Empty:
                _readyAction = new EmptyNodeReadyAction(this);
                _battleAction = new EmptyNodeBattleAction(this);
                _clearAction = new EmptyNodeClearAction(this);
                break;
            case NodeType.Battle:
                _readyAction = new BattleNodeReadyAction(this);
                _battleAction = new BattleNodeBattleAction(this);
                _clearAction = new BattleNodeClearAction(this);
                break;
            case NodeType.Boss:
                _readyAction = new BossNodeReadyAction(this);
                _battleAction = new BossNodeBattleAction(this);
                _clearAction = new BossNodeClearAction(this);
                break;
            case NodeType.Escape:
                _readyAction = new EscapeNodeReadyAction(this);
                _battleAction = new EscapeNodeBattleAction(this);
                _clearAction = new EscapeNodeClearAction(this);
                break;
            case NodeType.Shop:
                _readyAction = new ShopNodeReadyAction(this);
                _battleAction = new ShopNodeBattleAction(this);
                _clearAction = new ShopNodeClearAction(this);
                break;
            case NodeType.Start:
                _readyAction = new StartNodeReadyAction(this);
                _battleAction = new StartNodeBattleAction(this);
                _clearAction = new StartNodeClearAction(this);
                break;
            case NodeType.Test:
                _readyAction = new TestNodeReadyAction(this);
                _battleAction = new TestNodeBattleAction(this);
                _clearAction = new TestNodeClearAction(this);
                break;
        }
        DebugTool.Log($"{Controller.Data.NodeType}_{Controller.Data.NodeIndex} Action Controller Ready", DebugType.Node);
    }
    
    /// <summary>
    /// Node State가 전환 되면 현재 Action 전환<br/>
    /// 이벤트 체인으로 연결 되어 있음으로 직접 발동 금지<br/>
    /// 이 함수를 발동 시키려면 MapData의 SetNodeState 메서드를 사용할 것
    /// </summary>
    /// <param name="newState">맵 데이터의 NodeState 변수와 이벤트로 연결</param>
    public void ChangeState(NodeState newState)
    {
        switch (newState)
        {
            case NodeState.Ready:
                _machine.ChangeState(_readyAction);
                break;
            case NodeState.Battle:
                _machine.ChangeState(_battleAction);
                break;
            case NodeState.Clear:
                _machine.ChangeState(_clearAction);
                break;
        }
        DebugTool.Log($"{Controller.Data.NodeType}_{Controller.Data.NodeIndex} Node Action Change : {newState}", DebugType.Node);
    }

    /// <summary>
    /// ActionMachine의 RunningState를 유니티 Update에 올리기 위함
    /// </summary>
    public void Update()
    {
        _machine.RunningState();
    }
}
