using System;

namespace Zippers.Network.Contracts
{
    /// <summary>
    /// 한 플레이어의 최종 스탯을 제공한다.
    /// 개인 업그레이드 + 팀 업그레이드(영구) + 전투 효과(일회성) 가 모두 합산된 값.
    ///
    /// 구현체: PlayerStats (자원·성장 라인 F 소유)
    ///   - 위치: Assets/Scripts/이수형/Network/Stats/PlayerStats.cs (이전 예정)
    ///
    /// 사용처:
    ///   - B: PlayerHealth, PlayerCombat, PlayerReload, PlayerMovement, PlayerStamina
    ///   - B 가 GetComponent&lt;IPlayerStatProvider&gt;() 로 조회.
    ///
    /// 주의:
    ///   - WeaponType / 데미지 랜덤 굴림은 서버에서만 호출.
    ///   - Total* 프로퍼티는 모든 입력 (개인업+팀업+전투효과) 합산된 값.
    /// </summary>
    public interface IPlayerStatProvider
    {
        int ClassId { get; }
        WeaponType WeaponType { get; }
        bool UseBullet { get; }

        // 체력 / 스테미나
        float TotalMaxHealth { get; }
        float TotalMaxStamina { get; }
        float TotalStaminaRegen { get; }
        float StaminaDelay { get; }
        float StaminaConsume { get; }
        float StaminaPeriod { get; }

        // 공격
        float TotalAttackSpeed { get; }       // 쿨타임 감소까지 반영된 최종값
        float TotalMagazineCapacity { get; }
        float TotalReloadTime { get; }
        float TotalBulletSpeed { get; }
        float TotalBulletDistance { get; }

        // 이동
        float TotalMoveSpeed { get; }
        float TotalSprintMoveSpeed { get; }

        // 데미지 (랜덤 굴림은 서버에서만 호출)
        float TotalMinDamage { get; }
        float TotalMaxDamage { get; }

        // 기타 (추후 기능 연결)
        float TotalDamageReduction { get; }
        float TotalPierceCount { get; }
        float TotalKnockbackPower { get; }
        float TotalProjectileCount { get; }

        /// <summary>
        /// 스탯이 재계산되어 전투 라인의 캐시 값(MaxHealth / MaxAmmo / MaxStamina)을 갱신해야 할 때 발행.
        /// 트리거: personalUpgradeLevels 변경, teamUpgradeLevels 변경, IsBattle 변경.
        /// 구독자: PlayerHealth.RefreshHealth, PlayerReload.RefreshMaxBullet, PlayerStamina.RefreshMaxStamina.
        /// </summary>
        event Action OnStatsRecalculated;
    }
}
