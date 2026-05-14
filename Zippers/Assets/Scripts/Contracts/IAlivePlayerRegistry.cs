using System;

namespace Zippers.Network.Contracts
{
    /// <summary>
    /// 살아있는 플레이어 목록과 전멸 이벤트.
    /// 호스트 권위 카운트(ConnectedPlayerCount / SessionAlivePlayerCount) 를 그대로 활용.
    ///
    /// 구현체: SessionPlayerStateController 확장 (F 소유)
    ///   - 위치: Assets/Scripts/이수형/Network/Core/SessionPlayerStateController.cs (기존 파일에 인터페이스 추가)
    ///   - 기존 NotifyPlayerSpawned / NotifyPlayerHPDeath / NotifyPlayerRevive 메서드가 그대로 사용됨
    ///
    /// 사용처:
    ///   - B: PlayerHealth.Die → SessionPlayerStateController.NotifyPlayerHPDeath 호출
    ///   - F: TeamBattleUpgradeEffect, 게임 흐름 컨트롤러 등이 OnAllPlayersDead 구독
    ///
    /// 호출 패턴:
    ///   var registry = SessionPlayerStateController.Instance as IAlivePlayerRegistry;
    ///   registry?.OnAllPlayersDead += HandleAllDead;
    /// </summary>
    public interface IAlivePlayerRegistry
    {
        int ConnectedPlayerCount { get; }
        int AlivePlayerCount { get; }
        bool IsAlive(ulong clientId);

        event Action<ulong> OnPlayerDied;
        event Action<ulong> OnPlayerRevived;
        event Action OnAllPlayersDead;
    }
}
