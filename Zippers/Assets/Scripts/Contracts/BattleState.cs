namespace Zippers.Network.Contracts
{
    /// <summary>
    /// 전투 상태 머신.
    /// TeamBattleNetState 가 NetworkVariable&lt;BattleState&gt; 로 보관, IBattleStateBus 로 노출.
    ///
    /// 김영찬님의 NodeState 와의 정규화 매핑 (TeamBattleNetState 가 호스트에서 변환):
    ///   NodeState.Ready    → BattleState.Ready
    ///   NodeState.Battle   → BattleState.Battle
    ///   NodeState.Clear    → BattleState.Clear
    ///   (전투 실패/강제 종료 등) → BattleState.Ended
    /// </summary>
    public enum BattleState
    {
        Ready,    // 전투 진입 가능 상태
        Battle,   // 전투 진행 중
        Clear,    // 클리어 직후 (ClearHeal 등 일회성 효과 적용 시점)
        Ended,    // 전투 종료 (실패/강제/일반 종료)
    }
}
