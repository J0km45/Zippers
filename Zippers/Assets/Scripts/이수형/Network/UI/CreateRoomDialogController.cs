using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 방 생성 모달 다이얼로그.
/// 빈 이름 금지 — InputField 에 글자가 있어야만 Confirm 버튼 활성.
/// 닉네임 자동 채움 안 함 (PlayerInfo 에 닉네임 자체가 없음).
/// </summary>
public class CreateRoomDialogController : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TMP_InputField _roomNameInput;
    [SerializeField] private TMP_Text _warningText;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _cancelButton;

    private bool _isProcessing;

    private void OnEnable()
    {
        if (_confirmButton != null) _confirmButton.onClick.AddListener(OnConfirmClicked);
        if (_cancelButton != null) _cancelButton.onClick.AddListener(Close);
        if (_roomNameInput != null) _roomNameInput.onValueChanged.AddListener(OnNameInputChanged);
    }

    private void OnDisable()
    {
        if (_confirmButton != null) _confirmButton.onClick.RemoveListener(OnConfirmClicked);
        if (_cancelButton != null) _cancelButton.onClick.RemoveListener(Close);
        if (_roomNameInput != null) _roomNameInput.onValueChanged.RemoveListener(OnNameInputChanged);
    }

    /// <summary>모달 열기. 외부 (RoomListController) 에서 Create 버튼 클릭 시 호출.</summary>
    public void Open()
    {
        if (_panel != null) _panel.SetActive(true);
        ResetFields();
    }

    /// <summary>모달 닫기.</summary>
    public void Close()
    {
        if (_panel != null) _panel.SetActive(false);
    }

    private void ResetFields()
    {
        if (_roomNameInput != null) _roomNameInput.text = string.Empty;
        SetWarning("방 이름을 입력하세요");
        _isProcessing = false;
        // 빈 이름 상태로 시작 — Confirm 비활성
        if (_confirmButton != null) _confirmButton.interactable = false;
    }

    private void OnNameInputChanged(string value)
    {
        if (_isProcessing) return;
        bool hasName = !string.IsNullOrWhiteSpace(value);
        if (_confirmButton != null) _confirmButton.interactable = hasName;
        SetWarning(hasName ? string.Empty : "방 이름을 입력하세요");
    }

    private async void OnConfirmClicked()
    {
        if (_isProcessing) return;
        string roomName = _roomNameInput != null ? _roomNameInput.text?.Trim() : null;
        if (string.IsNullOrWhiteSpace(roomName))
        {
            SetWarning("방 이름을 입력하세요");
            return;
        }
        if (LobbyManager.Instance == null)
        {
            DebugTool.Error("LobbyManager.Instance 가 null - 씬에 LobbyManager 가 없습니다.", DebugType.Network, this);
            SetWarning("로비 매니저 미초기화");
            return;
        }

        _isProcessing = true;
        if (_confirmButton != null) _confirmButton.interactable = false;
        SetWarning("방 생성 중...");

        bool success = await LobbyManager.Instance.CreateSessionAsync(roomName);

        _isProcessing = false;
        if (success)
        {
            // 씬 전환은 RoomListController 가 OnSessionUpdated 받아서 처리.
            Close();
        }
        else
        {
            // 실패 시 InputField 의 현재 상태에 맞게 Confirm 재활성
            bool hasName = _roomNameInput != null && !string.IsNullOrWhiteSpace(_roomNameInput.text);
            if (_confirmButton != null) _confirmButton.interactable = hasName;
            SetWarning("방 생성 실패. 다시 시도하세요.");
        }
    }

    private void SetWarning(string message)
    {
        if (_warningText != null) _warningText.text = message;
    }
}
