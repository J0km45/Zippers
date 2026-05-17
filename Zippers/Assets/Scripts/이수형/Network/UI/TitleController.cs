using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// (temp)TitleScene 의 UI 컨트롤러.
/// EnterLobby 버튼 클릭 → UGS 인증 → DataLoadScene 으로 전환.
/// 닉네임 입력 없음 (PlayerInfo 에 닉네임 필드 자체를 두지 않음).
/// </summary>
public class TitleController : MonoBehaviour
{
    [SerializeField] private Button _enterLobbyButton;
    [SerializeField] private TMP_Text _statusText;

    private bool _isProcessing;

    private void OnEnable()
    {
        if (_enterLobbyButton != null) _enterLobbyButton.onClick.AddListener(OnEnterLobbyClicked);
        ClearStatus();
    }

    private void OnDisable()
    {
        if (_enterLobbyButton != null) _enterLobbyButton.onClick.RemoveListener(OnEnterLobbyClicked);
    }

    private async void OnEnterLobbyClicked()
    {
        if (_isProcessing) return;
        _isProcessing = true;
        SetButtonInteractable(false);
        SetStatus("로그인 중...");

        try
        {
            await AuthService.InitializeAsync();
            SetStatus("데이터 로드로 이동...");
            
            SceneChangeController.Instance?.OnExitScene(true);
        }
        catch (Exception e)
        {
            DebugTool.Error($"로그인 실패: {e.Message}", DebugType.Network);
            SetStatus("로그인 실패. 다시 시도하세요.");
            _isProcessing = false;
            SetButtonInteractable(true);
        }
    }

    private void SetButtonInteractable(bool value)
    {
        if (_enterLobbyButton != null) _enterLobbyButton.interactable = value;
    }

    private void SetStatus(string message)
    {
        if (_statusText != null) _statusText.text = message;
    }

    private void ClearStatus()
    {
        if (_statusText != null) _statusText.text = string.Empty;
    }
}
