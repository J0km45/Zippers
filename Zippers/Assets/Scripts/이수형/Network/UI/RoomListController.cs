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
/// - LobbyManager.OnError 구독해서 사용자 가시 에러를 _statusText 에 표시.
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
    [SerializeField] private Button _joinByCodeButton;
    [SerializeField] private Button _refreshButton;

    [Header("Status")]
    [SerializeField] private TMP_Text _statusText;

    [Header("Dialogs")]
    [SerializeField] private CreateRoomDialogController _createRoomDialog;
    [SerializeField] private JoinByCodeDialogController _joinByCodeDialog;

    private readonly List<RoomEntryUI> _spawnedEntries = new List<RoomEntryUI>();
    private bool _isBusy;
    private bool _hasTransitioned;

    private void Awake()
    {
        AutoWireMissingReferences();
        BindButtonEvents();
    }

    private void OnDestroy()
    {
        UnbindButtonEvents();
        UnbindLobbyManagerEvents();
    }

    private void Start()
    {
        if (LobbyManager.Instance == null)
        {
            DebugTool.Error("LobbyManager.Instance 가 null - Title 씬을 거치지 않았거나 Bootstrap 누락", DebugType.Network, this);
            SetStatus("로비 매니저 미초기화");
            return;
        }

        BindLobbyManagerEvents();

        // 방어용: 이미 세션에 들어와 있는 상태로 RoomList 에 들어왔다면 (예외 흐름) 즉시 LobbyScene 으로
        if (LobbyManager.Instance.CurrentSession != null)
        {
            DebugTool.Warning("RoomList 진입 시 이미 세션 보유 - LobbyScene 으로 즉시 전환", DebugType.Network, this);
            TryTransitionToLobby();
            return;
        }

        RefreshRoomList();
    }

    private void BindButtonEvents()
    {
        if (_createRoomButton != null) _createRoomButton.onClick.AddListener(OnCreateRoomClicked);
        if (_quickJoinButton != null) _quickJoinButton.onClick.AddListener(OnQuickJoinClicked);
        if (_joinByCodeButton != null) _joinByCodeButton.onClick.AddListener(OnJoinByCodeClicked);
        if (_refreshButton != null) _refreshButton.onClick.AddListener(RefreshRoomList);
    }

    private void UnbindButtonEvents()
    {
        if (_createRoomButton != null) _createRoomButton.onClick.RemoveListener(OnCreateRoomClicked);
        if (_quickJoinButton != null) _quickJoinButton.onClick.RemoveListener(OnQuickJoinClicked);
        if (_joinByCodeButton != null) _joinByCodeButton.onClick.RemoveListener(OnJoinByCodeClicked);
        if (_refreshButton != null) _refreshButton.onClick.RemoveListener(RefreshRoomList);
    }

    private void AutoWireMissingReferences()
    {
        if (_quickJoinButton == null)
        {
            _quickJoinButton = FindButtonByTextOrName("빠른", "Quick", "선택한 방 참가");
        }
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

    // ─────────────────────────────────────────────────────────────────
    // List refresh
    // ─────────────────────────────────────────────────────────────────

    public async void RefreshRoomList()
    {
        if (_isBusy) return;
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            SetStatus("로그인 상태가 아닙니다.");
            return;
        }
        if (LobbyManager.Instance == null) return;

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
        if (_entryPrefab == null || _entryContainer == null) return;
        for (int i = 0; i < sessions.Count; i++)
        {
            RoomEntryUI entry = Instantiate(_entryPrefab, _entryContainer);
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
        if (_emptyListText != null) _emptyListText.gameObject.SetActive(count == 0);
    }

    // ─────────────────────────────────────────────────────────────────
    // Button handlers
    // ─────────────────────────────────────────────────────────────────

    private void OnCreateRoomClicked()
    {
        if (_isBusy) return;
        if (_createRoomDialog != null) _createRoomDialog.Open();
    }

    private void OnJoinByCodeClicked()
    {
        if (_isBusy) return;
        if (_joinByCodeDialog != null) _joinByCodeDialog.Open();
    }

    private async void OnQuickJoinClicked()
    {
        if (_isBusy) return;
        if (LobbyManager.Instance == null) return;

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
        if (LobbyManager.Instance == null) return;

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

    // ─────────────────────────────────────────────────────────────────
    // Lobby manager events
    // ─────────────────────────────────────────────────────────────────

    private void OnSessionUpdated(ISession session)
    {
        // 세션 진입 성공 시점 — LobbyScene 으로 전환 (호스트만 NGO LoadScene, 클라는 NGO auto-sync)
        if (session != null && !_hasTransitioned)
        {
            TryTransitionToLobby();
        }
    }

    private void OnSessionLeft()
    {
        // 게임/Lobby 에서 Leave 후 RoomList 로 돌아온 경우 — 목록 새로고침
        _hasTransitioned = false;
        RefreshRoomList();
    }

    private void OnLobbyError(string message)
    {
        SetStatus(message);
    }

    private void TryTransitionToLobby()
    {
        _hasTransitioned = true;
        if (LobbyManager.Instance != null && LobbyManager.Instance.IsHost)
        {
            DebugTool.Log("호스트 - LobbyScene 으로 NGO LoadScene", DebugType.Network, this);
            SceneLoader.LoadNetworked(SceneId.Lobby);
        }
        else
        {
            DebugTool.Log("클라이언트 - NGO auto-sync 대기 (LobbyScene)", DebugType.Network, this);
            // NGO 가 호스트 씬에 맞춰 자동 sync. 별도 LoadLocal 호출 안 함.
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // Busy state / status
    // ─────────────────────────────────────────────────────────────────

    private void SetBusy(bool busy)
    {
        _isBusy = busy;
        if (_createRoomButton != null) _createRoomButton.interactable = !busy;
        if (_quickJoinButton != null) _quickJoinButton.interactable = !busy;
        if (_joinByCodeButton != null) _joinByCodeButton.interactable = !busy;
        if (_refreshButton != null) _refreshButton.interactable = !busy;
        for (int i = 0; i < _spawnedEntries.Count; i++)
        {
            if (_spawnedEntries[i] != null) _spawnedEntries[i].SetInteractable(!busy);
        }
    }

    private void SetStatus(string message)
    {
        if (_statusText != null) _statusText.text = message;
    }
}
