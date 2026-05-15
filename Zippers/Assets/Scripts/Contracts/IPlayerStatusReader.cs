using System;

namespace Zippers.Network.Contracts
{
    /// <summary>
    /// 한 플레이어의 현재 체력 / 생사 / 스테미나 / 탄약 상태를 읽기 전용으로 제공한다.
    ///
    /// 구현체: PlayerCombatNetState (B 소유, per-player)
    ///   - 위치: Assets/Scripts/최완용/Player/Network/PlayerCombatNetState.cs
    ///
    /// 사용처:
    ///   - F: TeamBattleUpgradeEffect.ApplyClearHeal (대상 선정 + 현재 체력 확인)
    ///   - F: SessionPlayerStateController 등 외부 전멸 판정
    ///   - UI: 체력바 / 스테미나바 / 탄약 표시
    ///
    /// 주의:
    ///   - MaxHealth 는 IPlayerStatProvider.TotalMaxHealth 와 동기화 캐시.
    ///     스탯 재계산 시 PlayerCombatNetState 가 OnStatsRecalculated 를 받아 MaxHealth 갱신.
    /// </summary>
    public interface IPlayerStatusReader
    {
        ulong OwnerClientId { get; }
        float CurrentHealth { get; }
        float MaxHealth { get; }
        bool IsDead { get; }
        float CurrentStamina { get; }
        float MaxStamina { get; } // 추가
        float CurrentAmmo { get; }
        float MaxAmmo { get; } // 추가
        bool IsReloading { get; }

        event Action<float, float> OnHealthChanged;   // (current, max)

        //추가
        event Action<float, float> OnStaminaChanged;  // (current, max)
        event Action<float, float> OnAmmoChanged;     // (current, max)
        event Action<bool> OnReloadStateChanged;      // isReloading
        //
        event Action OnPlayerDied;
        event Action OnPlayerRevived;
    }
}
