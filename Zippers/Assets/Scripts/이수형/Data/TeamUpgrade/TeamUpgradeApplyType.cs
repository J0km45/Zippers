/// <summary>
/// 팀 업그레이드 시트의 ApplyType 컬럼에 매핑되는 enum.
/// 시트의 문자열 값을 그대로 멤버명으로 사용한다 (대소문자 일치 필요).
///
/// - Add        : 베이스 값에 ValuePerLevel * level 만큼 절대값 가산
/// - AddPercent : 베이스 값에 (1 + ValuePerLevel * level) 배율 적용 (n% 증가)
/// </summary>
public enum TeamUpgradeApplyType
{
    Add,
    AddPercent,
}
