/// <summary>
/// 노드의 state에 따른 Action을 정의하는 인터페이스
/// </summary>
public interface INodeAction
{
    /// <summary>
    /// State 진입 시
    /// </summary>
    void EnterState();
    
    /// <summary>
    /// State 진행 중
    /// </summary>
    void RunningState();
    
    /// <summary>
    /// State 종료 시
    /// </summary>
    void ExitState();
}
