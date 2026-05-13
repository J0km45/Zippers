using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

public class LobbyStartGameButtonController : MonoBehaviour
{
    [SerializeField] private Button _startButton;
    [SerializeField] private TMP_Text _buttonText;

    private bool _isStarting;

    public static LobbyStartGameButtonController EnsureInScene()
    {
        LobbyStartGameButtonController existing = FindObjectOfType<LobbyStartGameButtonController>(true);
        if (existing != null) return existing;

        Button startButton = FindStartButton();
        if (startButton == null) return null;

        GameObject controllerHost = FindControllerHost(startButton);
        LobbyStartGameButtonController controller = controllerHost.AddComponent<LobbyStartGameButtonController>();
        controller._startButton = startButton;
        controller._buttonText = startButton.GetComponentInChildren<TMP_Text>(true);
        return controller;
    }

    private void Awake()
    {
        if (_startButton == null)
        {
            _startButton = FindStartButton();
        }

        if (_buttonText == null && _startButton != null)
        {
            _buttonText = _startButton.GetComponentInChildren<TMP_Text>(true);
        }
    }

    private void OnEnable()
    {
        if (_startButton != null)
        {
            _startButton.onClick.AddListener(OnStartClicked);
        }

        SubscribeLobbyEvents();
        Refresh();
    }

    private void OnDisable()
    {
        if (_startButton != null)
        {
            _startButton.onClick.RemoveListener(OnStartClicked);
        }

        UnsubscribeLobbyEvents();
    }

    private void SubscribeLobbyEvents()
    {
        if (LobbyManager.Instance == null) return;

        LobbyManager.Instance.OnSessionUpdated += HandleSessionUpdated;
        LobbyManager.Instance.OnSessionLeft += Refresh;
        LobbyManager.Instance.OnRestartCooldownEnded += Refresh;
    }

    private void UnsubscribeLobbyEvents()
    {
        if (LobbyManager.Instance == null) return;

        LobbyManager.Instance.OnSessionUpdated -= HandleSessionUpdated;
        LobbyManager.Instance.OnSessionLeft -= Refresh;
        LobbyManager.Instance.OnRestartCooldownEnded -= Refresh;
    }

    private void HandleSessionUpdated(ISession _)
    {
        Refresh();
    }

    private async void OnStartClicked()
    {
        if (_isStarting || LobbyManager.Instance == null) return;

        _isStarting = true;
        Refresh();

        bool success = await LobbyManager.Instance.TryStartGameAsHostAsync();
        if (!success)
        {
            _isStarting = false;
            Refresh();
        }
    }

    private void Refresh()
    {
        if (_startButton == null) return;

        ISession session = LobbyManager.Instance != null ? LobbyManager.Instance.CurrentSession : null;
        bool isHostStartButton = session != null && LobbyManager.Instance.IsHost;
        _startButton.gameObject.SetActive(isHostStartButton);

        if (!isHostStartButton || session.CurrentPlayer == null) return;

        PlayerInfo me = LobbyManager.Instance.GetPlayerInfo(session.CurrentPlayer);
        _startButton.interactable = !_isStarting && me.HasClass && LobbyManager.Instance.CanHostStartGame;

        if (_buttonText != null)
        {
            _buttonText.text = "게임 시작";
        }
    }

    private static Button FindStartButton()
    {
        Button[] buttons = FindObjectsOfType<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null || !button.gameObject.scene.IsValid()) continue;

            if (button.gameObject.name.Contains("Start")
                || button.gameObject.name.Contains("GameStart"))
            {
                return button;
            }
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null || !button.gameObject.scene.IsValid()) continue;
            if (button.gameObject.name.Contains("Ready")) continue;

            if (HasButtonText(button.transform, "게임 시작", "Start"))
            {
                return button;
            }
        }

        return null;
    }

    private static GameObject FindControllerHost(Button button)
    {
        Transform current = button.transform;
        while (current != null)
        {
            if (current.name.Contains("GameStartPanel"))
            {
                return current.gameObject;
            }

            current = current.parent;
        }

        return button.transform.parent != null ? button.transform.parent.gameObject : button.gameObject;
    }

    private static bool HasButtonText(Transform buttonRoot, params string[] keywords)
    {
        TMP_Text[] texts = buttonRoot.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            string text = texts[i].text;
            if (string.IsNullOrEmpty(text)) continue;

            for (int j = 0; j < keywords.Length; j++)
            {
                if (text.Contains(keywords[j])) return true;
            }
        }

        return false;
    }
}
