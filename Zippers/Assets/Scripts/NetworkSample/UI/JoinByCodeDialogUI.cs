using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 조인 코드로 특정 방을 바로 찾아 참여하는 팝업. 방 목록이 많을 때의 편의 경로
/// </summary>
public class JoinByCodeDialogUI : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TMP_InputField _codeInput;
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
        _codeInput.text = string.Empty;
        _warningText.text = string.Empty;
        _isProcessing = false;
        _confirmButton.interactable = true;
    }

    private async void OnConfirmClicked()
    {
        if (_isProcessing) return;

        string code = _codeInput.text.Trim();
        if (string.IsNullOrEmpty(code))
        {
            SetWarning("조인 코드를 입력하세요");
            return;
        }

        _isProcessing = true;
        _confirmButton.interactable = false;
        SetWarning("참여 중...");

        bool success = await LobbyManager.Instance.JoinSessionByCodeAsync(code);

        _isProcessing = false;
        if (success)
        {
            Close();
        }
        else
        {
            _confirmButton.interactable = true;
            SetWarning("참여 실패. 코드를 확인하세요.");
        }
    }

    private void SetWarning(string message)
    {
        _warningText.text = message;
    }
}
