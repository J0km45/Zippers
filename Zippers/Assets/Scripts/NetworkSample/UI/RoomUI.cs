using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Multiplayer;
using TMPro;

/// <summary>
/// 룸(방) 내부 UI. 플레이어 슬롯 + 레디/나가기 버튼 + 상태 메시지
/// </summary>
public class RoomUI : MonoBehaviour
{
    [SerializeField] private LobbySettings _settings;
    [SerializeField] private TMP_Text _roomNameText;
    [SerializeField] private TMP_Text _lobbyCodeText;
    [SerializeField] private List<RoomPlayerSlotUI> _playerSlots = new List<RoomPlayerSlotUI>();
    [SerializeField] private Button _readyButton;
    [SerializeField] private TMP_Text _readyButtonLabel;
    [SerializeField] private Button _leaveButton;
    [SerializeField] private TMP_Text _statusText;

    private bool _isLocalPlayerReady;
    private bool _isProcessingReady;

    private void OnEnable()
    {
        BindEvents();
        ResetInteractables();
        if (LobbyManager.Instance.CurrentSession != null)
        {
            Refresh(LobbyManager.Instance.CurrentSession);
        }
    }

    private void OnDisable()
    {
        UnbindEvents();
    }

    private void ResetInteractables()
    {
        // 이전 진입에서 OnLeaveClicked / OnGameStarting에 의해 false로 남아있을 수 있어 재진입 시 복구
        _readyButton.interactable = true;
        _leaveButton.interactable = true;
        _isProcessingReady = false;
        _isLocalPlayerReady = false;
    }

    private void BindEvents()
    {
        BindButtonEvents();
        BindLobbyManagerEvents();
    }

    private void UnbindEvents()
    {
        UnbindButtonEvents();
        UnbindLobbyManagerEvents();
    }

    private void BindButtonEvents()
    {
        _readyButton.onClick.AddListener(OnReadyClicked);
        _leaveButton.onClick.AddListener(OnLeaveClicked);
    }

    private void UnbindButtonEvents()
    {
        _readyButton.onClick.RemoveListener(OnReadyClicked);
        _leaveButton.onClick.RemoveListener(OnLeaveClicked);
    }

    private void BindLobbyManagerEvents()
    {
        LobbyManager.Instance.OnSessionUpdated += Refresh;
        LobbyManager.Instance.OnGameStarting += OnGameStarting;
        LobbyManager.Instance.OnRestartCooldownEnded += RefreshReadyButton;
    }

    private void UnbindLobbyManagerEvents()
    {
        LobbyManager.Instance.OnSessionUpdated -= Refresh;
        LobbyManager.Instance.OnGameStarting -= OnGameStarting;
        LobbyManager.Instance.OnRestartCooldownEnded -= RefreshReadyButton;
    }

    private void Refresh(ISession session)
    {
        if (session == null) return;
        _roomNameText.text = session.Name;
        _lobbyCodeText.text = $"Join Code: {session.Code}";
        RefreshPlayerSlots(session);
        RefreshReadyButton();
        RefreshStatusText(session);
    }

    private void RefreshPlayerSlots(ISession session)
    {
        bool isLocalHost = session.CurrentPlayer != null && session.CurrentPlayer.Id == session.Host;
        if (!isLocalHost && session.CurrentPlayer != null)
        {
            string readyValue = LobbyManager.GetPlayerProperty(session.CurrentPlayer, LobbyConstants.KEY_PLAYER_READY);
            _isLocalPlayerReady = readyValue == LobbyConstants.VALUE_TRUE;
        }
        else
        {
            _isLocalPlayerReady = false;
        }

        for (int i = 0; i < _playerSlots.Count; i++)
        {
            if (i < session.Players.Count)
            {
                ApplyPlayerToSlot(session, i);
            }
            else
            {
                _playerSlots[i].SetEmpty();
            }
        }
    }

    private void ApplyPlayerToSlot(ISession session, int index)
    {
        IReadOnlyPlayer player = session.Players[index];
        string playerName = LobbyManager.GetPlayerProperty(player, LobbyConstants.KEY_PLAYER_NAME) ?? "Player";
        string readyValue = LobbyManager.GetPlayerProperty(player, LobbyConstants.KEY_PLAYER_READY);
        bool isReady = readyValue == LobbyConstants.VALUE_TRUE;
        bool isHost = player.Id == session.Host;
        _playerSlots[index].SetPlayer(playerName, isReady, isHost);
    }

    private void RefreshReadyButton()
    {
        bool isHost = LobbyManager.Instance.IsHost;

        _readyButtonLabel.text = isHost
            ? "게임 시작"
            : (_isLocalPlayerReady ? "레디 취소" : "레디");

        if (!_isProcessingReady)
        {
            _readyButton.interactable = !isHost || LobbyManager.Instance.CanHostStartGame;
        }
        _leaveButton.interactable = true;
    }

    private void RefreshStatusText(ISession session)
    {
        int total = session.PlayerCount;
        int nonHostTotal = GetNonHostPlayerCount(session);
        int nonHostReady = GetNonHostReadyCount(session);

        if (total < _settings.MinPlayersToStart)
        {
            _statusText.text = $"최소 {_settings.MinPlayersToStart}명 필요 ({total}/{_settings.MaxPlayers})";
        }
        else if (nonHostReady < nonHostTotal)
        {
            _statusText.text = $"다른 플레이어 레디 대기 ({nonHostReady}/{nonHostTotal})";
        }
        else
        {
            _statusText.text = "호스트가 게임을 시작할 수 있습니다";
        }
    }

    private int GetNonHostPlayerCount(ISession session)
    {
        int count = 0;
        for (int i = 0; i < session.Players.Count; i++)
        {
            if (session.Players[i].Id != session.Host) count++;
        }
        return count;
    }

    private int GetNonHostReadyCount(ISession session)
    {
        int count = 0;
        for (int i = 0; i < session.Players.Count; i++)
        {
            IReadOnlyPlayer player = session.Players[i];
            if (player.Id == session.Host) continue;
            string ready = LobbyManager.GetPlayerProperty(player, LobbyConstants.KEY_PLAYER_READY);
            if (ready == LobbyConstants.VALUE_TRUE) count++;
        }
        return count;
    }

    private async void OnReadyClicked()
    {
        if (_isProcessingReady) return;
        _isProcessingReady = true;
        _readyButton.interactable = false;
        try
        {
            if (LobbyManager.Instance.IsHost)
            {
                await LobbyManager.Instance.TryStartGameAsHostAsync();
            }
            else
            {
                await LobbyManager.Instance.SetReadyAsync(!_isLocalPlayerReady);
            }
        }
        finally
        {
            _isProcessingReady = false;
            RefreshReadyButton();
        }
    }

    private async void OnLeaveClicked()
    {
        _leaveButton.interactable = false;
        await LobbyManager.Instance.LeaveSessionAsync();
    }

    private void OnGameStarting()
    {
        _statusText.text = "게임에 입장합니다...";
        _readyButton.interactable = false;
        _leaveButton.interactable = false;
    }
}
