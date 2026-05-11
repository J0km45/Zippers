/// <summary>
/// 로비에서 선택 가능한 플레이어 클래스. WeaponType (B 작업) 과 정수값 1:1 일치.
///
/// 정수값 정렬:
/// - Melee=0, Rifle=1, Shotgun=2, Pistol=3 → WeaponType 와 동일
/// - None=-1 → 미선택 sentinel (음수라 _classPrefabs 인덱스로 자연스럽게 차단됨)
///
/// 주의:
/// - 실제 클래스 종류/수치는 PlayerClassDataSO 에 시트로 정의됨. PlayerClass 는 단순히 lobby 에서 "어느 클래스인지" 식별용.
/// - PlayerProperty 에 정수 string 으로 직렬화되므로(예: "0"=Melee, "-1"=None),
///   기존 enum 값에 새 항목을 끼워 넣지 말고 항상 끝에 추가할 것.
///   (값이 바뀌면 진행 중인 세션의 클라/호스트 해석이 달라짐)
/// </summary>
public enum PlayerClass
{
    Melee   = 0,
    Rifle   = 1,
    Shotgun = 2,
    Pistol  = 3,
    None    = -1
}
