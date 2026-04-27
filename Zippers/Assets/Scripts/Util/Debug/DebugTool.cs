// ------------------------------------------------------------------------------
// 게임 코드에서 직접 호출하는 디버그 로그 진입점이며, 로그를 구조화해 매니저로 넘기는 공용 유틸 파일이다.
// 멤버별 주석은 해당 변수, 메서드, 클래스가 왜 필요한지와 호출 시 어떤 역할을 하는지를 빠르게 파악하기 위해 추가하였다.
// ------------------------------------------------------------------------------
using System;
using System.IO;
using System.Text;
using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// 디버그 타입에 따라 로그를 필터링하고 별도 런타임 디버그 콘솔로 전달하는 공용 유틸 클래스이다.
/// </summary>
public static class DebugTool
{
    // sequence 상태를 저장한다. 관련 메서드에서 기준값이나 캐시로 사용한다.
    private static long _sequence;

    /// <summary>
    /// 로그 처리 흐름을 수행한다. 관련 상태를 읽거나 갱신해 디버그 콘솔 동작을 이어간다.
    /// </summary>
    public static void Log(
        string text,
        DebugType type,
        Object context = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        Write(DebugLogLevel.Log, text, type, context, memberName, filePath, lineNumber);
    }

    /// <summary>
    /// 경고 처리 흐름을 수행한다. 관련 상태를 읽거나 갱신해 디버그 콘솔 동작을 이어간다.
    /// </summary>
    public static void Warning(
        string text,
        DebugType type,
        Object context = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        Write(DebugLogLevel.Warning, text, type, context, memberName, filePath, lineNumber);
    }

    // 기존 오타 함수명 호환
    public static void Warnning(
        string text,
        DebugType type,
        Object context = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        Write(DebugLogLevel.Warning, text, type, context, memberName, filePath, lineNumber);
    }

    /// <summary>
    /// 오류 처리 흐름을 수행한다. 관련 상태를 읽거나 갱신해 디버그 콘솔 동작을 이어간다.
    /// </summary>
    public static void Error(
        string text,
        DebugType type,
        Object context = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        Write(DebugLogLevel.Error, text, type, context, memberName, filePath, lineNumber);
    }

    /// <summary>
    /// missing 컴포넌트 처리 흐름을 수행한다. 관련 상태를 읽거나 갱신해 디버그 콘솔 동작을 이어간다.
    /// </summary>
    public static void MissingComponent(
        string text = null,
        Object context = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        string message = string.IsNullOrEmpty(text)
            ? "컴포넌트를 찾을 수 없습니다."
            : $"{text}을(를) 찾을 수 없습니다.";

        Write(DebugLogLevel.Warning, message, DebugType.Missing, context, memberName, filePath, lineNumber);
    }

    /// <summary>
    /// debug print all 처리 흐름을 수행한다. 관련 상태를 읽거나 갱신해 디버그 콘솔 동작을 이어간다.
    /// </summary>
    public static void DebugPrintAll(bool value)
    {
        if (DebugConsoleManager.Instance == null)
            return;

        DebugConsoleManager.Instance.GlobalEnabled = value;
    }

    /// <summary>
    /// debug select 처리 흐름을 수행한다. 관련 상태를 읽거나 갱신해 디버그 콘솔 동작을 이어간다.
    /// </summary>
    public static void DebugSelect(DebugType type, bool value)
    {
        if (DebugConsoleManager.Instance == null)
            return;

        DebugConsoleManager.Instance.SetTypeEnabled(type, value);
    }

    /// <summary>
    /// 관련 작업를 기록한다.
    /// </summary>
    private static void Write(
        DebugLogLevel level,
        string text,
        DebugType type,
        Object context,
        string memberName,
        string filePath,
        int lineNumber)
    {
        GetTargetIds(context, out int gameObjectId, out int componentId);

        DebugConsoleManager manager = DebugConsoleManager.Instance;
        if (manager != null && !manager.IsAllowed(type, context))
            return;

        ResolveTargetMetadata(
            context,
            out string sceneKey,
            out string hierarchyPath,
            out string gameObjectKey,
            out string componentKey,
            out string gameObjectName,
            out string componentName,
            out string componentTypeName);

        string color = GetColor(type);
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        string sourceName = GetSourceName(context, fileName);

        if (memberName == ".ctor")
            memberName = "생성자";

        DebugEntry entry = new DebugEntry
        {
            Time = DateTime.Now.ToString("HH:mm:ss.fff"),
            Message = text,
            SourceName = sourceName,
            MemberName = memberName,
            LineNumber = lineNumber,
            Type = type,
            Level = level,
            Context = context,
            GameObjectId = gameObjectId,
            ComponentId = componentId,
            ColorHex = color,
            CallerFilePath = filePath,
            CallerColumn = 1,
            StackTrace = BuildStackTrace(filePath, lineNumber, memberName),
            SequenceId = ++_sequence,
            FrameCount = UnityEngine.Time.frameCount,
            CapturedAtIsoUtc = DateTime.UtcNow.ToString("O"),
            SceneKey = sceneKey,
            HierarchyPath = hierarchyPath,
            GameObjectKey = gameObjectKey,
            ComponentKey = componentKey,
            GameObjectName = gameObjectName,
            ComponentName = componentName,
            ComponentTypeName = componentTypeName
        };

        entry.RefreshDerivedFields();

        if (manager != null)
        {
            manager.AddEntry(entry);

            if (manager.MirrorToUnityConsole)
                PrintToUnityConsole(entry);
        }
        else
        {
            PrintToUnityConsole(entry);
        }
    }

    /// <summary>
    /// resolve 대상 metadata 처리 흐름을 수행한다. 관련 상태를 읽거나 갱신해 디버그 콘솔 동작을 이어간다.
    /// </summary>
    private static void ResolveTargetMetadata(
        Object context,
        out string sceneKey,
        out string hierarchyPath,
        out string gameObjectKey,
        out string componentKey,
        out string gameObjectName,
        out string componentName,
        out string componentTypeName)
    {
        sceneKey = string.Empty;
        hierarchyPath = string.Empty;
        gameObjectKey = string.Empty;
        componentKey = string.Empty;
        gameObjectName = string.Empty;
        componentName = string.Empty;
        componentTypeName = string.Empty;

        if (context is GameObject go)
        {
            sceneKey = DebugConsoleFilterKeyUtility.GetSceneKey(go);
            hierarchyPath = DebugConsoleFilterKeyUtility.GetHierarchyPath(go.transform);
            gameObjectKey = DebugConsoleFilterKeyUtility.GetGameObjectKey(go);
            gameObjectName = go.name;
            return;
        }

        if (context is Component component)
        {
            sceneKey = DebugConsoleFilterKeyUtility.GetSceneKey(component.gameObject);
            hierarchyPath = DebugConsoleFilterKeyUtility.GetHierarchyPath(component.transform);
            gameObjectKey = DebugConsoleFilterKeyUtility.GetGameObjectKey(component.gameObject);
            componentKey = DebugConsoleFilterKeyUtility.GetComponentKey(component);
            gameObjectName = component.gameObject.name;
            componentName = component.GetType().Name;
            componentTypeName = component.GetType().FullName;
        }
    }

    /// <summary>
    /// 대상 ids 값을 계산해 반환한다. 조회용 메서드이므로 호출자는 반환값을 기준으로 다음 동작을 결정한다.
    /// </summary>
    private static void GetTargetIds(Object context, out int gameObjectId, out int componentId)
    {
        gameObjectId = 0;
        componentId = 0;

        if (context is GameObject go)
        {
            gameObjectId = go.GetInstanceID();
            return;
        }

        if (context is Component component)
        {
            gameObjectId = component.gameObject.GetInstanceID();
            componentId = component.GetInstanceID();
        }
    }

    /// <summary>
    /// 출처 이름 값을 계산해 반환한다. 조회용 메서드이므로 호출자는 반환값을 기준으로 다음 동작을 결정한다.
    /// </summary>
    private static string GetSourceName(Object context, string fallbackFileName)
    {
        if (context == null)
            return fallbackFileName;

        if (context is Component component)
            return $"{component.gameObject.name}/{component.GetType().Name}";

        if (context is GameObject go)
            return go.name;

        return context.name;
    }

    /// <summary>
    /// print to 유니티 console 처리 흐름을 수행한다. 관련 상태를 읽거나 갱신해 디버그 콘솔 동작을 이어간다.
    /// </summary>
    private static void PrintToUnityConsole(DebugEntry entry)
    {
        switch (entry.Level)
        {
            case DebugLogLevel.Warning:
                Debug.LogWarning(entry.RichText, entry.Context);
                break;

            case DebugLogLevel.Error:
                Debug.LogError(entry.RichText, entry.Context);
                break;

            default:
                Debug.Log(entry.RichText, entry.Context);
                break;
        }
    }


/// <summary>
/// 스택 트레이스 데이터를 조합해 새 문자열이나 키를 만든다. 동일한 규칙으로 값을 만들기 위해 사용한다.
/// </summary>
private static string BuildStackTrace(string filePath, int lineNumber, string memberName)
{
    try
    {
        var trace = new System.Diagnostics.StackTrace(2, true);
        string raw = trace.ToString();

        StringBuilder builder = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(raw))
            builder.Append(raw.Trim());

        if (!string.IsNullOrWhiteSpace(filePath))
        {
            if (builder.Length > 0)
                builder.AppendLine();

            builder.Append("Caller : ");
            builder.Append(Path.GetFileName(filePath));
            builder.Append(" / ");
            builder.Append(string.IsNullOrWhiteSpace(memberName) ? "-" : memberName);
            builder.Append(" / line ");
            builder.Append(Mathf.Max(1, lineNumber));
        }

        return builder.ToString();
    }
    catch
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return string.Empty;

        return $"Caller : {Path.GetFileName(filePath)} / {memberName} / line {Mathf.Max(1, lineNumber)}";
    }
}

/// <summary>
/// 색상 값을 계산해 반환한다. 조회용 메서드이므로 호출자는 반환값을 기준으로 다음 동작을 결정한다.
/// </summary>
private static string GetColor(DebugType type)
{
        switch (type)
        {
            case DebugType.Game: return "#B388FF";       // 보라
            case DebugType.Character: return "#FFD166";  // 노랑-골드
            case DebugType.Zombie: return "#FF3B30";     // 빨강
            case DebugType.Spawner: return "#00C2FF";    // 하늘색
            case DebugType.Wave: return "#FF7A00";       // 주황
            case DebugType.Node: return "#A3FF12";       // 라임
            case DebugType.Network: return "#00E676";    // 초록
            case DebugType.UI: return "#FF4FD8";         // 핑크
            case DebugType.Data: return "#00D1B2";       // 청록
            case DebugType.Missing: return "#FFFF00";    // 경고 노랑
            default: return "#D0D0D0";    // 기본 회색
        }
    }
}

/// <summary>
/// DebugType 값을 구분하기 위한 열거형이다.
/// </summary>
public enum DebugType
{
    Game,
    Data,
    Network,
    Character,
    Zombie,
    Wave,
    Spawner,
    Node,
    UI,
    Missing,
    Default
}
