using System;

namespace Zippers.Network.Contracts
{
    /// <summary>
    /// 전투 상태(전투 시작 / 클리어 / 종료) 이벤트 버스.
    /// 자원·성장 라인이 NodeState / 맵 시스템을 직접 구독하고 정규화해서 전투 라인에 알린다.
    ///
    /// 구현체: TeamBattleNetState (F 소유, team singleton)
    ///   - 위치: Assets/Scripts/이수형/Network/Battle/TeamBattleNetState.cs
    ///
    /// 사용처:
    ///   - F: TeamBattleUpgradeEffect (BattleDamage / BattleMoveSpeed / BattleAttackSpeed / ClearHeal 토글)
    ///   - B (옵션): 사망 시 부활 가능 여부 등에서 IsBattle 참조
    ///
    /// 발화 조건 (TeamBattleNetState 가 NodeState 변화를 받아 정규화):
    ///   OnBattleStarted : NodeState.Ready  → NodeState.Battle 진입
    ///   OnBattleCleared : NodeState.Battle → NodeState.Clear  진입
    ///   OnBattleEnded   : 전투 종료 (실패 / 강제 / Ready 복귀)
    /// </summary>
    public interface IBattleStateBus
    {
        bool IsBattle { get; }
        BattleState CurrentState { get; }

        event Action OnBattleStarted;
        event Action OnBattleCleared;
        event Action OnBattleEnded;
    }
}
