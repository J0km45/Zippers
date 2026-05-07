using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 타이틀 씬 UI. 플레이어 이름 입력 후 UGS 인증 후 로비 씬으로 전환
/// </summary>
public class TitleUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField _playerNameInput;
    [SerializeField] private Button _enterLobbyButton;
    [SerializeField] private TMP_Text _statusText;

    private void Awake()
    {
        if (string.IsNullOrEmpty(_playerNameInput.text))
        {
            _playerNameInput.text = $"Player{UnityEngine.Random.Range(100, 1000)}";
        }
    }

    private void OnEnable()
    {
        BindButtonEvents();
    }

    private void OnDisable()
    {
        UnbindButtonEvents();
    }

    private void BindButtonEvents()
    {
        _enterLobbyButton.onClick.AddListener(OnEnterLobbyClicked);
    }

    private void UnbindButtonEvents()
    {
        _enterLobbyButton.onClick.RemoveListener(OnEnterLobbyClicked);
    }

    private async void OnEnterLobbyClicked()
    {
        _enterLobbyButton.interactable = false;
        SetStatus("로그인 중...");
        try
        {
            await AuthService.InitializeAsync();
            LobbyManager.Instance.SetPlayerName(GetPlayerName());
            SetStatus("로비로 이동 중...");
            SceneLoader.LoadLocal(SceneId.Lobby);
        }
        catch (Exception e)
        {
            Debug.LogError($"TitleUI: 로그인 실패: {e.Message}");
            SetStatus("로그인 실패. 다시 시도하세요.");
            _enterLobbyButton.interactable = true;
        }
    }

    private string GetPlayerName()
    {
        string playerName = _playerNameInput.text;
        return string.IsNullOrWhiteSpace(playerName) ? "Player" : playerName;
    }

    private void SetStatus(string message)
    {
        _statusText.text = message;
    }
}
