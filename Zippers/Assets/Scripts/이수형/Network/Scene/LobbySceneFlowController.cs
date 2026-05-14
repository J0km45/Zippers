using System.Collections;
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
    private const float SESSION_WAIT_TIMEOUT_SEC = 5f;

    private bool _hasNavigated;
    
    [SerializeField] private SceneChangeController _SceneChangeController;

    private IEnumerator Start()
    {
        if (LobbyManager.Instance == null)
        {
            DebugTool.Error("LobbyManager.Instance 가 null - Title 씬 거치지 않음", DebugType.Network, this);
            yield break;
        }

        // 클라이언트 빠른 참가 때 NGO scene sync 가 ISession 할당보다 먼저 도착할 수 있어 짧게 대기한다.
        float deadline = Time.realtimeSinceStartup + SESSION_WAIT_TIMEOUT_SEC;
        while (LobbyManager.Instance.CurrentSession == null && Time.realtimeSinceStartup < deadline)
        {
            yield return null;
        }

        // 대기 후에도 세션이 없으면 예외 흐름으로 보고 RoomList 로 복귀
        if (LobbyManager.Instance.CurrentSession == null)
        {
            DebugTool.Warning("LobbyScene 진입 시 세션 없음 - RoomList 로 즉시 복귀", DebugType.Network, this);
            NavigateToRoomList();
            yield break;
        }

        LobbyManager.Instance.OnSessionLeft += OnSessionLeft;
        
        yield return null;
        
        if(_SceneChangeController != null)
            _SceneChangeController.OnEnterScene();
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
