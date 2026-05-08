using UnityEngine;

/// <summary>
/// LobbyScene 의 자동 씬 흐름 컨트롤러. UI 와 분리.
/// 주된 역할: 세션이 사라지면 자동으로 RoomListScene 으로 복귀.
///
/// 트리거 케이스:
/// - 본인이 LeaveSessionAsync 호출 (Leave 버튼)
/// - 호스트가 나가서 세션 자체가 삭제됨 (RemovedFromSession / Deleted 이벤트)
/// 두 케이스 모두 LobbyManager.OnSessionLeft 한 곳으로 모임.
///
/// LobbyScene 안에 빈 GameObject 한 개 만들어 attach 하면 됨.
/// </summary>
public class LobbySceneFlowController : MonoBehaviour
{
    private bool _hasNavigated;

    private void Start()
    {
        if (LobbyManager.Instance == null)
        {
            DebugTool.Error("LobbyManager.Instance 가 null - Title 씬 거치지 않음", DebugType.Network, this);
            return;
        }

        // 진입 직후 세션이 이미 없는 (예외) 상태라면 즉시 복귀
        if (LobbyManager.Instance.CurrentSession == null)
        {
            DebugTool.Warning("LobbyScene 진입 시 세션 없음 - RoomList 로 즉시 복귀", DebugType.Network, this);
            NavigateToRoomList();
            return;
        }

        LobbyManager.Instance.OnSessionLeft += OnSessionLeft;
    }

    private void OnDestroy()
    {
        if (LobbyManager.Instance == null) return;
        LobbyManager.Instance.OnSessionLeft -= OnSessionLeft;
    }

    private void OnSessionLeft()
    {
        DebugTool.Log("OnSessionLeft - RoomList 로 복귀", DebugType.Network, this);
        NavigateToRoomList();
    }

    private void NavigateToRoomList()
    {
        if (_hasNavigated) return;
        _hasNavigated = true;
        SceneLoader.LoadLocal(SceneId.RoomList);
    }
}
