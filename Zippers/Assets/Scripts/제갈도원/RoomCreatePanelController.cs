using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


// RoomListScene 등에서 방 이름 입력 후 호출.
// 방 생성자는 항상 호스트이므로, 성공 시 로 LobbyScene 동기화 로드.

public class RoomCreatePanelController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField _roomNameInput;
    [SerializeField] private Button _createButton;
    [SerializeField] private TMP_Text _statusText;

    [Header("Events")]
    [SerializeField] private UnityEvent _onSucceeded;

    [Header("Scene")]
    [SerializeField] private bool _loadLobbyAfterCreateSuccess = true;

    private bool _busy;

    private void OnEnable()
    {
        if (_createButton != null) _createButton.onClick.AddListener(OnCreateClicked);
        if (_roomNameInput != null) _roomNameInput.onValueChanged.AddListener(OnRoomNameChanged);
        RefreshCreateButtonState();
    }

    private void OnDisable()
    {
        if (_createButton != null) _createButton.onClick.RemoveListener(OnCreateClicked);
        if (_roomNameInput != null) _roomNameInput.onValueChanged.RemoveListener(OnRoomNameChanged);
    }

    // 입력버튼 상태만 초기화 (패널 열 때 호출 가능)
    public void ResetPanel()
    {
        _busy = false;
        if (_roomNameInput != null) _roomNameInput.text = string.Empty;
        SetStatus(string.Empty);
        RefreshCreateButtonState();
    }

    private void OnRoomNameChanged(string _)
    {
        if (_busy) return;
        RefreshCreateButtonState();
    }

    private void RefreshCreateButtonState()
    {
        if (_createButton == null) return;
        bool hasName = _roomNameInput != null && !string.IsNullOrWhiteSpace(_roomNameInput.text);
        _createButton.interactable = !_busy && hasName;
    }

    private async void OnCreateClicked()
    {
        if (_busy) return;

        string roomName = _roomNameInput != null ? _roomNameInput.text.Trim() : string.Empty;
        if (string.IsNullOrWhiteSpace(roomName))
        {
            SetStatus("방 이름을 입력하세요.");
            return;
        }

        if (LobbyManager.Instance == null)
        {
            DebugTool.Error("LobbyManager.Instance 가 null 입니다.", DebugType.Network, this);
            SetStatus("로비 매니저 미초기화");
            return;
        }

        SetBusy(true);
        SetStatus($"'{roomName}' 방 생성 중...");

        bool isRoomSet = await LobbyManager.Instance.CreateSessionAsync(roomName);

        SetBusy(false);

        if (isRoomSet)
        {
            SetStatus("방 생성 완료. 로비로 이동 중...");
            if (_loadLobbyAfterCreateSuccess)
            {
                TryTransitionHostToLobby();
            }

            _onSucceeded?.Invoke();
        }
        else
        {
            SetStatus("방 생성에 실패했습니다.");
            RefreshCreateButtonState();
        }
    }

    
    public Task<bool> TryCreateRoomAsync(string roomName)
    {
        if (LobbyManager.Instance == null)
        {
            DebugTool.Error("LobbyManager.Instance 가 null 입니다.", DebugType.Network, this);
            return Task.FromResult(false);
        }

        string trimmed = roomName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return Task.FromResult(false);
        }

        return LobbyManager.Instance.CreateSessionAsync(trimmed);
    }

    // SetBusy : 지금 처리 중 입니다.
    private void SetBusy(bool busy)
    {
        _busy = busy;
        RefreshCreateButtonState();
    }

    private void SetStatus(string message)
    {
        if (_statusText != null)
        {
            _statusText.text = message;
        }
    }

    // 방을 만든 클라이언트는 호스트 -> NGO 동기화 로드는 호스트만 수행.
    private void TryTransitionHostToLobby()
    {
        if (LobbyManager.Instance == null || !LobbyManager.Instance.IsHost)
        {
            return;
        }

        string lobbySceneName = SceneId.Lobby.GetName();
        if (SceneManager.GetActiveScene().name == lobbySceneName)
        {
            return;
        }

        DebugTool.Log("호스트 - LobbyScene 으로 NGO LoadScene", DebugType.Network, this);
        SceneLoader.LoadNetworked(SceneId.Lobby);
    }
}
