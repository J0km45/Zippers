using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomCreatePanelController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField _roomNameInput;
    [SerializeField] private TMP_InputField _maxPlayersInput;
    [SerializeField] private Button _createButton;
    [SerializeField] private TMP_Text _statusText;
    [SerializeField] private int _defaultMaxPlayers = 4;
    [SerializeField] private int _minMaxPlayers = 1;
    [SerializeField] private int _maxMaxPlayers = 4;

    [Header("Events")]
    [SerializeField] private UnityEvent _onSucceeded;

    [Header("Scene")]
    [SerializeField] private bool _loadLobbyAfterCreateSuccess = true;

    private bool _busy;

    private void Awake()
    {
        // 인원 수 입력칸이 연결되지 않은 경우 자식 오브젝트에서 자동 탐색
        AutoWireOptionalReferences();
    }

    private void OnEnable()
    {
        if (_createButton != null) _createButton.onClick.AddListener(OnCreateClicked);
        if (_roomNameInput != null) _roomNameInput.onValueChanged.AddListener(OnRoomNameChanged);
        if (_maxPlayersInput != null) _maxPlayersInput.onValueChanged.AddListener(OnMaxPlayersChanged);
        RefreshCreateButtonState();
    }

    private void OnDisable()
    {
        if (_createButton != null) _createButton.onClick.RemoveListener(OnCreateClicked);
        if (_roomNameInput != null) _roomNameInput.onValueChanged.RemoveListener(OnRoomNameChanged);
        if (_maxPlayersInput != null) _maxPlayersInput.onValueChanged.RemoveListener(OnMaxPlayersChanged);
    }

    public void ResetPanel()
    {
        _busy = false;
        if (_roomNameInput != null) _roomNameInput.text = string.Empty;
        if (_maxPlayersInput != null) _maxPlayersInput.text = _defaultMaxPlayers.ToString();
        SetStatus(string.Empty);
        RefreshCreateButtonState();
    }

    private void OnRoomNameChanged(string _)
    {
        if (_busy) return;
        RefreshCreateButtonState();
    }

    private void OnMaxPlayersChanged(string value)
    {
        if (_busy || _maxPlayersInput == null) return;

        // 인원 수 입력은 숫자만 허용
        string digitsOnly = ExtractDigits(value);
        if (_maxPlayersInput.text != digitsOnly)
        {
            _maxPlayersInput.SetTextWithoutNotify(digitsOnly);
        }
    }

    private void RefreshCreateButtonState()
    {
        if (_createButton == null) return;
        bool hasName = _roomNameInput != null && !string.IsNullOrWhiteSpace(_roomNameInput.text);
        _createButton.interactable = !_busy && hasName;
    }

    private async void OnCreateClicked()
    {
        if (_busy) return;

        string roomName = _roomNameInput != null ? _roomNameInput.text.Trim() : string.Empty;
        if (string.IsNullOrWhiteSpace(roomName))
        {
            SetStatus("방 이름을 입력해 주세요.");
            return;
        }

        if (!TryGetMaxPlayers(out int maxPlayers))
        {
            // LobbyManager는 기본 생성 API를 유지하므로 여기서는 입력값 검증만 수행
            SetStatus($"인원 수는 {_minMaxPlayers}~{_maxMaxPlayers} 사이 숫자로 입력해 주세요.");
            return;
        }

        if (LobbyManager.Instance == null)
        {
            DebugTool.Error("LobbyManager.Instance is null.", DebugType.Network, this);
            SetStatus("로비 매니저를 찾을 수 없습니다.");
            return;
        }

        SetBusy(true);
        SetStatus($"'{roomName}' 방을 생성하는 중입니다...");

        bool isRoomSet = await LobbyManager.Instance.CreateSessionAsync(roomName);

        SetBusy(false);

        if (isRoomSet)
        {
            SetStatus("방 생성이 완료되었습니다. 로비로 이동합니다...");
            if (_loadLobbyAfterCreateSuccess)
            {
                TryTransitionHostToLobby();
            }

            _onSucceeded?.Invoke();
        }
        else
        {
            SetStatus("방 생성에 실패했습니다.");
            RefreshCreateButtonState();
        }
    }

    public Task<bool> TryCreateRoomAsync(string roomName)
    {
        return TryCreateRoomAsync(roomName, _defaultMaxPlayers);
    }

    public Task<bool> TryCreateRoomAsync(string roomName, int maxPlayers)
    {
        if (LobbyManager.Instance == null)
        {
            DebugTool.Error("LobbyManager.Instance is null.", DebugType.Network, this);
            return Task.FromResult(false);
        }

        string trimmed = roomName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return Task.FromResult(false);
        }

        return LobbyManager.Instance.CreateSessionAsync(trimmed);
    }

    private void SetBusy(bool busy)
    {
        _busy = busy;
        RefreshCreateButtonState();
    }

    private void SetStatus(string message)
    {
        if (_statusText != null)
        {
            _statusText.text = message;
        }
    }

    private void TryTransitionHostToLobby()
    {
        if (LobbyManager.Instance == null || !LobbyManager.Instance.IsHost)
        {
            return;
        }

        string lobbySceneName = SceneId.Lobby.GetName();
        if (SceneManager.GetActiveScene().name == lobbySceneName)
        {
            return;
        }

        // 방 생성자는 호스트이므로 LobbyScene 전환은 호스트만 네트워크 로드
        DebugTool.Log("Host transitions to LobbyScene.", DebugType.Network, this);
        SceneLoader.LoadNetworked(SceneId.Lobby);
    }

    private void AutoWireOptionalReferences()
    {
        if (_maxPlayersInput != null) return;

        TMP_InputField[] inputs = GetComponentsInChildren<TMP_InputField>(true);
        for (int i = 0; i < inputs.Length; i++)
        {
            TMP_InputField input = inputs[i];
            if (input == null || input == _roomNameInput) continue;

            if (ContainsAny(input.gameObject.name, "PlayerCount", "MaxPlayers", "MaxPlayer", "인원", "플레이어"))
            {
                _maxPlayersInput = input;
                return;
            }
        }
    }

    private bool TryGetMaxPlayers(out int maxPlayers)
    {
        if (_maxPlayersInput == null)
        {
            // 입력칸이 없으면 기본 인원 수 사용
            maxPlayers = Mathf.Clamp(_defaultMaxPlayers, _minMaxPlayers, _maxMaxPlayers);
            return true;
        }

        string rawValue = _maxPlayersInput.text?.Trim();
        if (!int.TryParse(rawValue, out maxPlayers))
        {
            return false;
        }

        if (maxPlayers < _minMaxPlayers || maxPlayers > _maxMaxPlayers)
        {
            return false;
        }

        return true;
    }

    private static string ExtractDigits(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        StringBuilder builder = new StringBuilder(value.Length);
        for (int i = 0; i < value.Length; i++)
        {
            if (char.IsDigit(value[i]))
            {
                builder.Append(value[i]);
            }
        }

        return builder.ToString();
    }

    private static bool ContainsAny(string value, params string[] keywords)
    {
        if (string.IsNullOrEmpty(value)) return false;

        for (int i = 0; i < keywords.Length; i++)
        {
            if (!string.IsNullOrEmpty(keywords[i]) && value.Contains(keywords[i]))
            {
                return true;
            }
        }

        return false;
    }
}
