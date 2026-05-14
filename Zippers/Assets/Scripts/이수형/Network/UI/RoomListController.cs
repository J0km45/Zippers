using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// RoomListScene 의 메인 UI 컨트롤러.
/// 방 목록 조회 / 생성 / 빠른참여 / 코드참여 트리거 + 세션 진입 성공 시 LobbyScene 으로 씬 전환.
///
/// 샘플 LobbyListUI 와 차이:
/// - 패널 토글(LobbyListPanel ↔ RoomPanel) 없음. 세션 진입 성공 = 씬 전환.
/// - 호스트는 SceneLoader.LoadNetworked(SceneId.Lobby) 로 NGO sync, 클라는 NGO auto-sync 의존.
/// - LobbyManager.OnError logs user-visible errors through DebugTool.
/// - DebugTool 통합.
/// </summary>
public class RoomListController : MonoBehaviour
{
    [Header("List")]
    [SerializeField] private Transform _entryContainer;
    [SerializeField] private RoomEntryUI _entryPrefab;
    [SerializeField] private TMP_Text _emptyListText;

    [Header("Buttons")]
    [SerializeField] private Button _createRoomButton;
    [SerializeField] private Button _quickJoinButton;
    [SerializeField] private Button _joinSelectedRoomButton;
    [SerializeField] private Button _joinByCodeButton;
    [SerializeField] private Button _refreshButton;

    [Header("Dialogs")]
    [SerializeField] private CreateRoomDialogController _createRoomDialog;
    [SerializeField] private JoinByCodeDialogController _joinByCodeDialog;

    private readonly List<RoomEntryUI> _spawnedEntries = new List<RoomEntryUI>();

    private bool _isBusy;
    private bool _hasTransitioned;
    private string _selectedSessionId;

    private void Awake()
    {
        AutoWireMissingReferences();
        // 씬에 배치된 목록 템플릿은 런타임 목록과 겹치지 않도록 숨김
        HideSceneEntryTemplates();
        BindButtonEvents();
    }

    private void Start()
    {
        if (LobbyManager.Instance == null)
        {
            LogStatus("로비 매니저를 찾을 수 없습니다.");
            return;
        }

        BindLobbyManagerEvents();
        UpdateJoinSelectedButtonState();

        if (LobbyManager.Instance.CurrentSession != null)
        {
            TryTransitionToLobby();
            return;
        }

        RefreshRoomList();
    }

    private void OnDestroy()
    {
        UnbindButtonEvents();
        UnbindLobbyManagerEvents();
    }

    private void AutoWireMissingReferences()
    {
        if (_createRoomButton == null)
        {
            _createRoomButton = FindButtonByTextOrName("새로운 방 생성", "방 생성", "CreateRoom", "Create");
        }

        if (_quickJoinButton == null)
        {
            _quickJoinButton = FindButtonByTextOrName("빠른 참가", "Quick");
        }

        if (_joinSelectedRoomButton == null)
        {
            _joinSelectedRoomButton = FindButtonByTextOrName("선택한 방 참가", "JoinSelected", "Selected Room");
        }

        if (_joinByCodeButton == null)
        {
            _joinByCodeButton = FindButtonByTextOrName("초대 코드", "Invite Code", "Code");
        }

        if (_refreshButton == null)
        {
            _refreshButton = FindButtonByTextOrName("새로 고침", "새로고침", "Refresh");
        }
    }

    private void BindButtonEvents()
    {
        if (_createRoomButton != null) _createRoomButton.onClick.AddListener(OnCreateRoomClicked);
        if (_quickJoinButton != null) _quickJoinButton.onClick.AddListener(OnQuickJoinClicked);
        if (_joinSelectedRoomButton != null) _joinSelectedRoomButton.onClick.AddListener(OnJoinSelectedRoomClicked);
        if (_joinByCodeButton != null) _joinByCodeButton.onClick.AddListener(OnJoinByCodeClicked);
        if (_refreshButton != null) _refreshButton.onClick.AddListener(RefreshRoomList);
    }

    private void UnbindButtonEvents()
    {
        if (_createRoomButton != null) _createRoomButton.onClick.RemoveListener(OnCreateRoomClicked);
        if (_quickJoinButton != null) _quickJoinButton.onClick.RemoveListener(OnQuickJoinClicked);
        if (_joinSelectedRoomButton != null) _joinSelectedRoomButton.onClick.RemoveListener(OnJoinSelectedRoomClicked);
        if (_joinByCodeButton != null) _joinByCodeButton.onClick.RemoveListener(OnJoinByCodeClicked);
        if (_refreshButton != null) _refreshButton.onClick.RemoveListener(RefreshRoomList);
    }

    private void BindLobbyManagerEvents()
    {
        LobbyManager.Instance.OnSessionUpdated += OnSessionUpdated;
        LobbyManager.Instance.OnSessionLeft += OnSessionLeft;
        LobbyManager.Instance.OnError += OnLobbyError;
    }

    private void UnbindLobbyManagerEvents()
    {
        if (LobbyManager.Instance == null) return;
        LobbyManager.Instance.OnSessionUpdated -= OnSessionUpdated;
        LobbyManager.Instance.OnSessionLeft -= OnSessionLeft;
        LobbyManager.Instance.OnError -= OnLobbyError;
    }

    private void HideSceneEntryTemplates()
    {
        if (_entryContainer == null) return;

        RoomEntryUI[] sceneEntries = _entryContainer.GetComponentsInChildren<RoomEntryUI>(true);
        for (int i = 0; i < sceneEntries.Length; i++)
        {
            RoomEntryUI entry = sceneEntries[i];
            if (entry == null) continue;

            entry.gameObject.SetActive(false);
        }
    }

    public async void RefreshRoomList()
    {
        if (_isBusy) return;
        if (LobbyManager.Instance == null) return;
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            DebugTool.Warning("로그인 상태가 아닙니다", DebugType.UI, this);
            return;
        }

        SetBusy(true);
        DebugTool.Log("방 목록 조회를 시작합니다", DebugType.UI, this);

        try
        {
            IList<ISessionInfo> sessions = await LobbyManager.Instance.QuerySessionsAsync();
            PopulateEntries(sessions);
            RefreshEmptyLabel(sessions.Count);
            DebugTool.Log($"방 목록 조회 완료 : {sessions.Count}", DebugType.UI, this);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void PopulateEntries(IList<ISessionInfo> sessions)
    {
        // 새로고침 후에도 기존 선택 방이 목록에 남아 있으면 선택 유지
        string previouslySelectedId = _selectedSessionId;

        ClearEntries();

        if (_entryPrefab == null || _entryContainer == null)
        {
            ClearSelection();
            return;
        }

        RoomEntryUI selectedEntry = null;
        for (int i = 0; i < sessions.Count; i++)
        {
            RoomEntryUI entry = Instantiate(_entryPrefab, _entryContainer);
            // 비활성 템플릿이나 프리팹을 사용해도 생성된 항목은 표시
            entry.gameObject.SetActive(true);
            entry.Setup(sessions[i], OnEntrySelected);
            _spawnedEntries.Add(entry);

            if (!string.IsNullOrEmpty(previouslySelectedId) && sessions[i].Id == previouslySelectedId)
            {
                selectedEntry = entry;
            }
        }

        if (selectedEntry != null)
        {
            ApplySelection(selectedEntry, false);
        }
        else
        {
            ClearSelection();
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
        if (_emptyListText != null)
        {
            _emptyListText.gameObject.SetActive(count == 0);
        }
    }

    private void OnCreateRoomClicked()
    {
        if (_isBusy) return;
        if (_createRoomDialog != null)
        {
            _createRoomDialog.Open();
        }
    }

    private void OnJoinByCodeClicked()
    {
        if (_isBusy) return;
        if (_joinByCodeDialog != null)
        {
            _joinByCodeDialog.Open();
        }
    }

    private async void OnQuickJoinClicked()
    {
        if (_isBusy) return;
        if (LobbyManager.Instance == null) return;

        SetBusy(true);
        LogStatus("빠른 참가 중입니다...");

        try
        {
            bool success = await LobbyManager.Instance.QuickJoinAsync();
            if (!success)
            {
                LogStatus("참가 가능한 방을 찾지 못했습니다.");
            }
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void OnJoinSelectedRoomClicked()
    {
        if (_isBusy) return;
        if (LobbyManager.Instance == null) return;

        RoomEntryUI selectedEntry = GetSelectedEntry();
        if (selectedEntry == null || selectedEntry.SessionInfo == null)
        {
            LogStatus("먼저 참가할 방을 선택해 주세요.");
            UpdateJoinSelectedButtonState();
            return;
        }

        ISessionInfo sessionInfo = selectedEntry.SessionInfo;

        SetBusy(true);
        LogStatus($"'{sessionInfo.Name}' 방에 참가하는 중입니다...");

        try
        {
            bool success = await LobbyManager.Instance.JoinSessionByIdAsync(sessionInfo.Id);
            if (!success)
            {
                LogStatus("선택한 방 참가에 실패했습니다.");
            }
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void OnEntrySelected(RoomEntryUI entry)
    {
        ApplySelection(entry, true);
    }

    private void ApplySelection(RoomEntryUI selectedEntry, bool updateStatus)
    {
        // 실제 입장은 선택된 세션 ID를 기준으로 처리
        _selectedSessionId = selectedEntry != null && selectedEntry.SessionInfo != null
            ? selectedEntry.SessionInfo.Id
            : null;

        for (int i = 0; i < _spawnedEntries.Count; i++)
        {
            RoomEntryUI entry = _spawnedEntries[i];
            if (entry != null)
            {
                entry.SetSelected(entry == selectedEntry);
            }
        }

        UpdateJoinSelectedButtonState();

        if (updateStatus && selectedEntry != null && selectedEntry.SessionInfo != null)
        {
            LogStatus($"선택된 방: {selectedEntry.SessionInfo.Name}");
        }
    }

    private RoomEntryUI GetSelectedEntry()
    {
        if (string.IsNullOrEmpty(_selectedSessionId)) return null;

        for (int i = 0; i < _spawnedEntries.Count; i++)
        {
            RoomEntryUI entry = _spawnedEntries[i];
            if (entry != null && entry.SessionInfo != null && entry.SessionInfo.Id == _selectedSessionId)
            {
                return entry;
            }
        }

        return null;
    }

    private void ClearSelection()
    {
        _selectedSessionId = null;
        UpdateJoinSelectedButtonState();
    }

    private void UpdateJoinSelectedButtonState()
    {
        if (_joinSelectedRoomButton != null)
        {
            _joinSelectedRoomButton.interactable = !_isBusy && !string.IsNullOrEmpty(_selectedSessionId);
        }
    }

    private void OnSessionUpdated(ISession session)
    {
        if (session != null && !_hasTransitioned)
        {
            TryTransitionToLobby();
        }
    }

    private void OnSessionLeft()
    {
        _hasTransitioned = false;
        _selectedSessionId = null;
        UpdateJoinSelectedButtonState();
        RefreshRoomList();
    }

    private void OnLobbyError(string message)
    {
        LogStatus(message);
    }

    private void TryTransitionToLobby()
    {
        _hasTransitioned = true;

        if (LobbyManager.Instance != null && LobbyManager.Instance.IsHost)
        {
            SceneLoader.LoadNetworked(SceneId.Lobby);
        }
    }

    private void SetBusy(bool busy)
    {
        _isBusy = busy;

        if (_createRoomButton != null) _createRoomButton.interactable = !busy;
        if (_quickJoinButton != null) _quickJoinButton.interactable = !busy;
        if (_joinByCodeButton != null) _joinByCodeButton.interactable = !busy;
        if (_refreshButton != null) _refreshButton.interactable = !busy;

        UpdateJoinSelectedButtonState();

        for (int i = 0; i < _spawnedEntries.Count; i++)
        {
            if (_spawnedEntries[i] != null)
            {
                _spawnedEntries[i].SetInteractable(!busy);
            }
        }
    }

    private void LogStatus(string message)
    {
        DebugTool.Log(message, DebugType.UI, this);
    }

    private Button FindButtonByTextOrName(params string[] keywords)
    {
        Button[] buttons = FindObjectsOfType<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null || !button.gameObject.scene.IsValid()) continue;

            if (ContainsAny(button.gameObject.name, keywords) || ButtonTextContainsAny(button.transform, keywords))
            {
                return button;
            }
        }

        return null;
    }

    private static bool ButtonTextContainsAny(Transform buttonRoot, string[] keywords)
    {
        TMP_Text[] texts = buttonRoot.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            if (ContainsAny(texts[i].text, keywords)) return true;
        }

        return false;
    }

    private static bool ContainsAny(string value, string[] keywords)
    {
        if (string.IsNullOrEmpty(value)) return false;

        for (int i = 0; i < keywords.Length; i++)
        {
            if (!string.IsNullOrEmpty(keywords[i]) && value.Contains(keywords[i])) return true;
        }

        return false;
    }
}
