using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 조인 코드로 특정 방에 바로 참여하는 모달 다이얼로그.
/// 코드는 호스트가 방 생성 후 ISession.Code 로 받음 (RoomUI 등에서 표시 후 공유).
/// </summary>
public class JoinByCodeDialogController : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TMP_InputField _codeInput;
    [SerializeField] private TMP_Text _warningText;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _cancelButton;

    private bool _isProcessing;

    private void OnEnable()
    {
        if (_confirmButton != null) _confirmButton.onClick.AddListener(OnConfirmClicked);
        if (_cancelButton != null) _cancelButton.onClick.AddListener(Close);
    }

    private void OnDisable()
    {
        if (_confirmButton != null) _confirmButton.onClick.RemoveListener(OnConfirmClicked);
        if (_cancelButton != null) _cancelButton.onClick.RemoveListener(Close);
    }

    public void Open()
    {
        if (_panel != null) _panel.SetActive(true);
        ResetFields();
    }

    public void Close()
    {
        if (_panel != null) _panel.SetActive(false);
    }

    private void ResetFields()
    {
        if (_codeInput != null) _codeInput.text = string.Empty;
        SetWarning(string.Empty);
        _isProcessing = false;
        if (_confirmButton != null) _confirmButton.interactable = true;
    }

    private async void OnConfirmClicked()
    {
        if (_isProcessing) return;

        string code = _codeInput != null ? _codeInput.text?.Trim() : null;
        if (string.IsNullOrEmpty(code))
        {
            SetWarning("조인 코드를 입력하세요");
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
        SetWarning("참여 중...");

        bool success = await LobbyManager.Instance.JoinSessionByCodeAsync(code);

        _isProcessing = false;
        if (success)
        {
            Close();
        }
        else
        {
            if (_confirmButton != null) _confirmButton.interactable = true;
            SetWarning("참여 실패. 코드를 확인하세요.");
        }
    }

    private void SetWarning(string message)
    {
        if (_warningText != null) _warningText.text = message;
    }
}
