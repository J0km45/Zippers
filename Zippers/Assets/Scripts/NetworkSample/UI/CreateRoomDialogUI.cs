using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 방 생성 팝업. 방 이름만 입력받아 LobbyManager로 생성 요청
/// </summary>
public class CreateRoomDialogUI : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TMP_InputField _roomNameInput;
    [SerializeField] private TMP_Text _warningText;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _cancelButton;

    private bool _isProcessing;

    private void OnEnable()
    {
        BindButtonEvents();
    }

    private void OnDisable()
    {
        UnbindButtonEvents();
    }

    /// <summary>
    /// 팝업 열기
    /// </summary>
    public void Open()
    {
        _panel.SetActive(true);
        ResetFields();
    }

    /// <summary>
    /// 팝업 닫기
    /// </summary>
    public void Close()
    {
        _panel.SetActive(false);
    }

    private void BindButtonEvents()
    {
        _confirmButton.onClick.AddListener(OnConfirmClicked);
        _cancelButton.onClick.AddListener(Close);
    }

    private void UnbindButtonEvents()
    {
        _confirmButton.onClick.RemoveListener(OnConfirmClicked);
        _cancelButton.onClick.RemoveListener(Close);
    }

    private void ResetFields()
    {
        _roomNameInput.text = string.Empty;
        _warningText.text = string.Empty;
        _isProcessing = false;
        _confirmButton.interactable = true;
    }

    private async void OnConfirmClicked()
    {
        if (_isProcessing) return;

        string roomName = _roomNameInput.text;
        if (string.IsNullOrWhiteSpace(roomName))
        {
            roomName = $"{LobbyManager.Instance.PlayerName}'s Room";
        }

        _isProcessing = true;
        _confirmButton.interactable = false;
        SetWarning("방 생성 중...");

        bool success = await LobbyManager.Instance.CreateSessionAsync(roomName);

        _isProcessing = false;
        if (success)
        {
            Close();
        }
        else
        {
            _confirmButton.interactable = true;
            SetWarning("방 생성 실패. 다시 시도하세요.");
        }
    }

    private void SetWarning(string message)
    {
        _warningText.text = message;
    }
}
