/// <summary>
/// 로비에서 선택 가능한 플레이어 클래스.
/// None = 미선택. 슬롯/세션은 살아있지만 아직 클래스를 고르지 않은 상태.
///
/// 주의:
/// - 실제 클래스 종류/수치는 B(전투) 담당과 합의되면 교체.
/// - PlayerProperty 에 정수 string 으로 직렬화되므로(예: "2"),
///   기존 enum 값에 새 항목을 끼워 넣지 말고 항상 끝에 추가할 것.
///   (값이 바뀌면 진행 중인 세션의 클라/호스트 해석이 달라짐)
/// </summary>
public enum PlayerClass
{
    None    = 0,
    Warrior = 1,
    Ranger  = 2,
    Tank    = 3,
    Support = 4
}
