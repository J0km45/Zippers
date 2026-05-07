using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using TMPro;

/// <summary>
/// 로비 씬의 세션 목록 UI + 방 생성/빠른참여/새로고침 컨트롤
/// </summary>
public class LobbyListUI : MonoBehaviour
{
    [SerializeField] private GameObject _lobbyListPanel;
    [SerializeField] private GameObject _roomPanel;
    [SerializeField] private Transform _entryContainer;
    [SerializeField] private LobbyEntryUI _entryPrefab;
    [SerializeField] private Button _createRoomButton;
    [SerializeField] private Button _quickJoinButton;
    [SerializeField] private Button _joinByCodeButton;
    [SerializeField] private Button _refreshButton;
    [SerializeField] private TMP_Text _statusText;
    [SerializeField] private TMP_Text _emptyListText;
    [SerializeField] private CreateRoomDialogUI _createRoomDialog;
    [SerializeField] private JoinByCodeDialogUI _joinByCodeDialog;

    private readonly List<LobbyEntryUI> _spawnedEntries = new List<LobbyEntryUI>();
    private bool _isBusy;

    private void Awake()
    {
        BindEvents();
    }

    private void OnDestroy()
    {
        UnbindEvents();
    }

    private void Start()
    {
        ShowLobbyListPanel(LobbyManager.Instance.CurrentSession == null);
        if (_lobbyListPanel.activeSelf)
        {
            RefreshLobbyList();
        }
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
        _createRoomButton.onClick.AddListener(OnCreateRoomClicked);
        _quickJoinButton.onClick.AddListener(OnQuickJoinClicked);
        _joinByCodeButton.onClick.AddListener(OnJoinByCodeClicked);
        _refreshButton.onClick.AddListener(RefreshLobbyList);
    }

    private void UnbindButtonEvents()
    {
        _createRoomButton.onClick.RemoveListener(OnCreateRoomClicked);
        _quickJoinButton.onClick.RemoveListener(OnQuickJoinClicked);
        _joinByCodeButton.onClick.RemoveListener(OnJoinByCodeClicked);
        _refreshButton.onClick.RemoveListener(RefreshLobbyList);
    }

    private void BindLobbyManagerEvents()
    {
        LobbyManager.Instance.OnSessionUpdated += OnSessionUpdated;
        LobbyManager.Instance.OnSessionLeft += OnSessionLeft;
    }

    private void UnbindLobbyManagerEvents()
    {
        LobbyManager.Instance.OnSessionUpdated -= OnSessionUpdated;
        LobbyManager.Instance.OnSessionLeft -= OnSessionLeft;
    }

    private async void RefreshLobbyList()
    {
        if (_isBusy) return;
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            SetStatus("로그인 상태가 아닙니다.");
            return;
        }

        SetBusy(true);
        SetStatus("방 목록 조회 중...");
        try
        {
            IList<ISessionInfo> sessions = await LobbyManager.Instance.QuerySessionsAsync();
            PopulateEntries(sessions);
            RefreshEmptyLabel(sessions.Count);
            SetStatus($"방 {sessions.Count}개 조회됨");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void PopulateEntries(IList<ISessionInfo> sessions)
    {
        ClearEntries();
        for (int i = 0; i < sessions.Count; i++)
        {
            LobbyEntryUI entry = Instantiate(_entryPrefab, _entryContainer);
            entry.Setup(sessions[i], OnEntryJoinClicked);
            _spawnedEntries.Add(entry);
        }
    }

    private void ClearEntries()
    {
        for (int i = 0; i < _spawnedEntries.Count; i++)
        {
            if (_spawnedEntries[i] != null) Destroy(_spawnedEntries[i].gameObject);
        }
        _spawnedEntries.Clear();
    }

    private void RefreshEmptyLabel(int count)
    {
        _emptyListText.gameObject.SetActive(count == 0);
    }

    private void OnCreateRoomClicked()
    {
        if (_isBusy) return;
        _createRoomDialog.Open();
    }

    private void OnJoinByCodeClicked()
    {
        if (_isBusy) return;
        _joinByCodeDialog.Open();
    }

    private async void OnQuickJoinClicked()
    {
        if (_isBusy) return;
        SetBusy(true);
        SetStatus("빠른 참여 중...");
        try
        {
            bool success = await LobbyManager.Instance.QuickJoinAsync();
            if (!success) SetStatus("참여할 방을 찾지 못했습니다.");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void OnEntryJoinClicked(ISessionInfo sessionInfo)
    {
        if (_isBusy) return;
        SetBusy(true);
        SetStatus($"'{sessionInfo.Name}' 참여 중...");
        try
        {
            bool success = await LobbyManager.Instance.JoinSessionByIdAsync(sessionInfo.Id);
            if (!success) SetStatus("방 참여 실패");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SetBusy(bool busy)
    {
        _isBusy = busy;
        _createRoomButton.interactable = !busy;
        _quickJoinButton.interactable = !busy;
        _joinByCodeButton.interactable = !busy;
        _refreshButton.interactable = !busy;
        for (int i = 0; i < _spawnedEntries.Count; i++)
        {
            if (_spawnedEntries[i] != null) _spawnedEntries[i].SetInteractable(!busy);
        }
    }

    private void OnSessionUpdated(ISession session)
    {
        if (session != null) ShowLobbyListPanel(false);
    }

    private void OnSessionLeft()
    {
        ShowLobbyListPanel(true);
        RefreshLobbyList();
    }

    private void ShowLobbyListPanel(bool show)
    {
        _lobbyListPanel.SetActive(show);
        _roomPanel.SetActive(!show);
    }

    private void SetStatus(string message)
    {
        _statusText.text = message;
    }
}
