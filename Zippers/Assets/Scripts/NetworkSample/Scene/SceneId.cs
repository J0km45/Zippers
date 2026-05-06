using System;

/// <summary>
/// 씬 식별자. 씬 이름(string) 매핑은 SceneIdExtensions.GetName() 에 하드코딩.
/// Build Settings 순서에 의존하지 않으므로 인덱스가 바뀌어도 안전하다.
/// 새 씬 추가 시: enum 값 추가 + GetName() switch 분기 추가 + Build Settings 등록.
/// </summary>
public enum SceneId
{
    Title,
    Lobby,
    Game
}

public static class SceneIdExtensions
{
    // 씬 파일명(확장자 제외) - Build Settings 에 동일 이름이 등록되어 있어야 한다
    public const string TitleSceneName = "TitleSceneTest";
    public const string LobbySceneName = "LobbySceneTest";
    public const string GameSceneName  = "GameSceneTest";

    /// <summary>
    /// SceneId 에 매핑된 씬 이름(확장자 없음) 반환.
    /// Unity SceneManager / NGO NetworkSceneManager 가 string 만 받으므로 변환 유틸로 사용
    /// </summary>
    public static string GetName(this SceneId id) => id switch
    {
        SceneId.Title => TitleSceneName,
        SceneId.Lobby => LobbySceneName,
        SceneId.Game  => GameSceneName,
        _ => throw new ArgumentOutOfRangeException(nameof(id), id, "SceneIdExtensions: 매핑되지 않은 SceneId")
    };
}
