/// <summary>
/// State Patten 기반 노드 액션 컨트롤러
/// </summary>
public class ActionMachine
{
    INodeAction _nodeAction;
    
    /// <summary>
    /// Node State가 전환 될 때 Action 전환
    /// </summary>
    /// <param name="nodeAction">전환 될 Node State Action</param>
    public void ChangeState(INodeAction nodeAction)
    {
        _nodeAction?.ExitState();
        _nodeAction = nodeAction;
        _nodeAction?.EnterState();
    }
    
    /// <summary>
    /// Node Action의 Update 함수를 유니티 이벤트 함수와 연결
    /// </summary>
    public void RunningState()
    {
        _nodeAction?.RunningState();
    }
}
