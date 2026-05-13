using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

public class LobbyReadyButtonController : MonoBehaviour
{
    [SerializeField] private Button _readyButton;
    [SerializeField] private TMP_Text _buttonText;

    private bool _isUpdating;

    public static LobbyReadyButtonController EnsureInScene()
    {
        LobbyReadyButtonController existing = FindObjectOfType<LobbyReadyButtonController>(true);
        if (existing != null) return existing;

        Button readyButton = FindReadyButton();
        if (readyButton == null) return null;

        GameObject controllerHost = FindControllerHost(readyButton);
        LobbyReadyButtonController controller = controllerHost.AddComponent<LobbyReadyButtonController>();
        controller._readyButton = readyButton;
        controller._buttonText = readyButton.GetComponentInChildren<TMP_Text>(true);
        return controller;
    }

    private void Awake()
    {
        if (_readyButton == null)
        {
            _readyButton = FindReadyButton();
        }

        if (_buttonText == null && _readyButton != null)
        {
            _buttonText = _readyButton.GetComponentInChildren<TMP_Text>(true);
        }
    }

    private void OnEnable()
    {
        if (_readyButton != null)
        {
            _readyButton.onClick.AddListener(OnReadyClicked);
        }

        SubscribeLobbyEvents();
        Refresh();
    }

    private void OnDisable()
    {
        if (_readyButton != null)
        {
            _readyButton.onClick.RemoveListener(OnReadyClicked);
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

    private async void OnReadyClicked()
    {
        if (_isUpdating || LobbyManager.Instance == null) return;

        ISession session = LobbyManager.Instance.CurrentSession;
        if (session?.CurrentPlayer == null) return;

        PlayerInfo me = LobbyManager.Instance.GetPlayerInfo(session.CurrentPlayer);
        if (!me.HasClass) return;

        _isUpdating = true;
        Refresh();

        await LobbyManager.Instance.SetReadyAsync(!me.IsReady);

        _isUpdating = false;
        Refresh();
    }

    private void Refresh()
    {
        if (_readyButton == null) return;

        ISession session = LobbyManager.Instance != null ? LobbyManager.Instance.CurrentSession : null;
        bool isClientReadyButton = session != null && !LobbyManager.Instance.IsHost;
        _readyButton.gameObject.SetActive(isClientReadyButton);

        if (!isClientReadyButton || session.CurrentPlayer == null) return;

        PlayerInfo me = LobbyManager.Instance.GetPlayerInfo(session.CurrentPlayer);
        _readyButton.interactable = !_isUpdating && me.HasClass;

        if (_buttonText != null)
        {
            _buttonText.text = me.IsReady ? "준비 취소" : "게임 준비";
        }
    }

    private static Button FindReadyButton()
    {
        Button[] buttons = FindObjectsOfType<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null || !button.gameObject.scene.IsValid()) continue;

            if (button.gameObject.name.Contains("Ready"))
            {
                return button;
            }
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null || !button.gameObject.scene.IsValid()) continue;
            if (button.gameObject.name.Contains("Start") || button.gameObject.name.Contains("GameStart")) continue;

            if (HasButtonText(button.transform, "게임 준비", "Ready"))
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
