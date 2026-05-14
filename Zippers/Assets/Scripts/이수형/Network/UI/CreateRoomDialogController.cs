using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomDialogController : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TMP_InputField _roomNameInput;
    [SerializeField] private TMP_InputField _maxPlayersInput;
    [SerializeField] private TMP_Text _warningText;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private int _defaultMaxPlayers = 4;
    [SerializeField] private int _minMaxPlayers = 1;
    [SerializeField] private int _maxMaxPlayers = 4;

    private bool _isProcessing;

    private void Awake()
    {
        // 인원 수 입력칸이 연결되지 않은 경우 자식 오브젝트에서 자동 탐색
        AutoWireOptionalReferences();
    }

    private void OnEnable()
    {
        if (_confirmButton != null) _confirmButton.onClick.AddListener(OnConfirmClicked);
        if (_cancelButton != null) _cancelButton.onClick.AddListener(Close);
        if (_roomNameInput != null) _roomNameInput.onValueChanged.AddListener(OnNameInputChanged);
        if (_maxPlayersInput != null) _maxPlayersInput.onValueChanged.AddListener(OnMaxPlayersInputChanged);
    }

    private void OnDisable()
    {
        if (_confirmButton != null) _confirmButton.onClick.RemoveListener(OnConfirmClicked);
        if (_cancelButton != null) _cancelButton.onClick.RemoveListener(Close);
        if (_roomNameInput != null) _roomNameInput.onValueChanged.RemoveListener(OnNameInputChanged);
        if (_maxPlayersInput != null) _maxPlayersInput.onValueChanged.RemoveListener(OnMaxPlayersInputChanged);
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
        if (_roomNameInput != null) _roomNameInput.text = string.Empty;
        if (_maxPlayersInput != null) _maxPlayersInput.text = _defaultMaxPlayers.ToString();

        _isProcessing = false;
        SetWarning("방 이름을 입력해 주세요.");

        if (_confirmButton != null) _confirmButton.interactable = false;
    }

    private void OnNameInputChanged(string value)
    {
        if (_isProcessing) return;

        bool hasName = !string.IsNullOrWhiteSpace(value);
        if (_confirmButton != null) _confirmButton.interactable = hasName;
        SetWarning(hasName ? string.Empty : "방 이름을 입력해 주세요.");
    }

    private void OnMaxPlayersInputChanged(string value)
    {
        if (_isProcessing || _maxPlayersInput == null) return;

        // 인원 수 입력은 숫자만 허용
        string digitsOnly = ExtractDigits(value);
        if (_maxPlayersInput.text != digitsOnly)
        {
            _maxPlayersInput.SetTextWithoutNotify(digitsOnly);
        }
    }

    private async void OnConfirmClicked()
    {
        if (_isProcessing) return;

        string roomName = _roomNameInput != null ? _roomNameInput.text?.Trim() : null;
        if (string.IsNullOrWhiteSpace(roomName))
        {
            SetWarning("방 이름을 입력해 주세요.");
            return;
        }

        if (!TryGetMaxPlayers(out int maxPlayers))
        {
            // 입력한 최대 인원 수를 세션 생성 옵션으로 전달하기 전에 검증
            SetWarning($"인원 수는 {_minMaxPlayers}~{_maxMaxPlayers} 사이 숫자로 입력해 주세요.");
            return;
        }

        if (LobbyManager.Instance == null)
        {
            DebugTool.Error("LobbyManager.Instance is null.", DebugType.Network, this);
            SetWarning("로비 매니저를 찾을 수 없습니다.");
            return;
        }

        _isProcessing = true;
        if (_confirmButton != null) _confirmButton.interactable = false;
        SetWarning("방을 생성하는 중입니다...");

        bool success = await LobbyManager.Instance.CreateSessionAsync(roomName, maxPlayers);

        _isProcessing = false;
        if (success)
        {
            Close();
            return;
        }

        bool hasName = _roomNameInput != null && !string.IsNullOrWhiteSpace(_roomNameInput.text);
        if (_confirmButton != null) _confirmButton.interactable = hasName;
        SetWarning("방 생성에 실패했습니다. 다시 시도해 주세요.");
    }

    private void SetWarning(string message)
    {
        if (_warningText != null) _warningText.text = message;
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
