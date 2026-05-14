/// <summary>
/// 호스트·클라이언트 사이에서 합의되어야 하는 PlayerProperty / SessionProperty 키와 값 계약.
/// 키 문자열을 변경하면 진행 중인 세션 호환성이 깨지므로 주의.
///
/// 보관 영역 구분:
/// - KEY_PLAYER_*  → PlayerProperty (각 플레이어 본인이 쓰는 영역)
/// - KEY_SESSION_* → SessionProperty (호스트만 쓰고 모두가 읽는 영역)
///
/// 닉네임(KEY_PLAYER_NAME)은 의도적으로 제거. 플레이어 정보에는 클래스/Ready/슬롯만 사용한다.
/// </summary>
public static class LobbyConstants
{
    // ── PlayerProperty 키 ────────────────────────────────────────────
    /// <summary>본인이 선택한 PlayerClass. 값은 enum 정수의 string (예: "1"). 미선택은 "0" (None).</summary>
    public const string KEY_PLAYER_CLASS = "Class";

    /// <summary>본인 Ready 여부 (VALUE_TRUE / VALUE_FALSE). PlayerClass != None 일 때만 true 가능.</summary>
    public const string KEY_PLAYER_READY = "Ready";

    // ── SessionProperty 키 (호스트 권한 영역) ──────────────────────────
    /// <summary>
    /// 슬롯 배정 매핑. 값은 JSON 형식: {"0":"hostPlayerId","2":"otherPlayerId", ...}.
    /// 키는 슬롯 인덱스(string), 값은 ISession.Players[i].Id 문자열.
    /// 호스트가 PlayerJoined / PlayerHasLeft 시점에 갱신.
    /// </summary>
    public const string KEY_SESSION_SLOTS = "Slots";

    /// <summary>
    /// 이번 게임의 난이도. 값은 (int)NodeDifficulty 의 string (예: "3" = Level3).
    /// 호스트가 LobbyScene 에서 선택 (LobbyManager.SetDifficultyAsHostAsync),
    /// GameScene 진입 후 NodeManager.InitializeAsHost 가 LobbyManager.GetCurrentDifficulty 로 읽음.
    /// 미설정 / 파싱 실패 시 LobbySettings.DefaultDifficulty (기본 Level3 = Normal) 로 폴백.
    /// </summary>
    public const string KEY_SESSION_DIFFICULTY = "Difficulty";

    // ── 공통 boolean string ────────────────────────────────────────
    public const string VALUE_TRUE  = "1";
    public const string VALUE_FALSE = "0";
}
