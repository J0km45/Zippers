/// <summary>
/// 팀 업그레이드 시트의 StatKey 컬럼에 매핑되는 enum.
/// 시트의 문자열 값을 그대로 멤버명으로 사용한다 (대소문자 일치 필요).
///
/// - Battle* / ClearHeal 계열 : 다음 전투 1회 한정 일회성 버프
/// - MaxHealth, Stamina, Damage, AttackSpeed, MoveSpeed : 영구 스탯 가산
/// </summary>
public enum TeamUpgradeStatKey
{
    // ─── 일회성 (다음 전투 한정) ───
    ClearHeal,
    BattleDamage,
    BattleMoveSpeed,
    BattleAttackSpeed,

    // ─── 영구 스탯 가산 ───
    MaxHealth,
    Stamina,
    Damage,
    AttackSpeed,
    MoveSpeed,
}
