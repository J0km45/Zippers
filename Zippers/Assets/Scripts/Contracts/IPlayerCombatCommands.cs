namespace Zippers.Network.Contracts
{
    /// <summary>
    /// 서버에서 회복 / 데미지 / 사망 / 부활을 요청하는 명령 엔드포인트.
    ///
    /// 구현체: PlayerCombatNetState (B 소유, per-player)
    ///   - 위치: Assets/Scripts/최완용/Player/Network/PlayerCombatNetState.cs
    ///
    /// 사용처:
    ///   - F: TeamBattleUpgradeEffect.ApplyClearHeal → ServerApplyHeal
    ///   - F/B 공통: 디버그 / 스크립트성 사망 → ServerKill
    ///   - 부활 시스템: ServerRevive
    ///
    /// 모든 메서드는 호스트에서만 호출. 내부에 다음 가드 포함:
    ///   - !IsServer 면 무시
    ///   - IsDead 상태에서 ServerApplyHeal / ServerApplyDamage 무시
    ///   - !IsDead 상태에서 ServerRevive 무시
    /// </summary>
    public interface IPlayerCombatCommands
    {
        void ServerApplyHeal(float amount, string source);
        void ServerApplyDamage(float damage, ulong attackerClientId, string source);
        void ServerKill(string reason);              // 디버그/스크립트성 사망
        void ServerRevive(float healthRatio);         // 0.0 ~ 1.0, MaxHealth 기준 비율
    }
}
