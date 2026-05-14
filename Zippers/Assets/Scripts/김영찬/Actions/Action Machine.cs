/// <summary>
/// State Patten 기반 노드 액션 컨트롤러
/// </summary>
public class ActionMachine
{
    INodeAction _nodeAction;

    /// <summary>
    /// Node State가 전환 될 때 Action 전환.
    /// 같은 Action 인스턴스가 다시 들어오면 ExitState/EnterState 를 다시 부르지 않음
    /// (race condition 방어용 수동 발화나 중복 OnValueChanged 콜백에서도 안전).
    /// </summary>
    /// <param name="nodeAction">전환 될 Node State Action</param>
    public void ChangeState(INodeAction nodeAction)
    {
        if (_nodeAction == nodeAction) return;   // 동일 Action 재호출 시 중복 발화 방지
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
