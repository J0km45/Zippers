using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;

public class LobbyTopInfoBarController : MonoBehaviour
{
    [SerializeField] private TMP_Text _roomNameText;
    [SerializeField] private TMP_Text _inviteCodeText;
    [SerializeField] private TMP_Text _currentPlayersText;
    [SerializeField] private TMP_Text _readyPlayersText;

    public static LobbyTopInfoBarController EnsureInScene()
    {
        LobbyTopInfoBarController existing = FindFirstObjectByType<LobbyTopInfoBarController>(FindObjectsInactive.Include);
        if (existing != null) return existing;

        GameObject topInfoBar = GameObject.Find("LobbyTopInfoBar");
        if (topInfoBar == null) return null;

        LobbyTopInfoBarController controller = topInfoBar.AddComponent<LobbyTopInfoBarController>();
        controller.AutoWireMissingReferences();
        controller.Refresh();
        return controller;
    }

    private void Awake()
    {
        AutoWireMissingReferences();
    }

    private void OnEnable()
    {
        SubscribeLobbyEvents();
        Refresh();
    }

    private void OnDisable()
    {
        UnsubscribeLobbyEvents();
    }

    private void SubscribeLobbyEvents()
    {
        if (LobbyManager.Instance == null) return;

        LobbyManager.Instance.OnSessionUpdated += HandleSessionUpdated;
        LobbyManager.Instance.OnSessionLeft += Refresh;
    }

    private void UnsubscribeLobbyEvents()
    {
        if (LobbyManager.Instance == null) return;

        LobbyManager.Instance.OnSessionUpdated -= HandleSessionUpdated;
        LobbyManager.Instance.OnSessionLeft -= Refresh;
    }

    private void HandleSessionUpdated(ISession _)
    {
        Refresh();
    }

    private void Refresh()
    {
        ISession session = LobbyManager.Instance != null ? LobbyManager.Instance.CurrentSession : null;

        if (_roomNameText != null)
        {
            _roomNameText.text = session != null && !string.IsNullOrWhiteSpace(session.Name)
                ? session.Name
                : "-";
        }

        if (_inviteCodeText != null)
        {
            _inviteCodeText.text = session != null && !string.IsNullOrWhiteSpace(session.Code)
                ? session.Code
                : "-";
        }

        int currentPlayers = session != null ? session.PlayerCount : 0;
        int maxPlayers = session != null ? session.MaxPlayers : 0;
        string playerCountText = maxPlayers > 0 ? $"{currentPlayers} / {maxPlayers}" : currentPlayers.ToString();

        if (_currentPlayersText != null)
        {
            _currentPlayersText.text = playerCountText;
        }

        if (_readyPlayersText != null)
        {
            int readyPlayers = CountReadyPlayers(session);
            _readyPlayersText.text = maxPlayers > 0 ? $"{readyPlayers} / {maxPlayers}" : readyPlayers.ToString();
        }
    }

    private static int CountReadyPlayers(ISession session)
    {
        if (session == null || session.Players == null || LobbyManager.Instance == null) return 0;

        int count = 0;
        for (int i = 0; i < session.Players.Count; i++)
        {
            if (session.Players[i].Id == session.Host)
            {
                count++;
                continue;
            }

            PlayerInfo info = LobbyManager.Instance.GetPlayerInfo(session.Players[i]);
            if (info.IsReady) count++;
        }

        return count;
    }

    private void AutoWireMissingReferences()
    {
        if (_roomNameText == null)
        {
            _roomNameText = FindTextByObjectName(transform, "RoomNameText");
        }

        if (_inviteCodeText == null)
        {
            _inviteCodeText = FindTextByObjectName(transform, "InviteCodeText");
        }

        if (_currentPlayersText == null)
        {
            Transform currentPlayersSegment = FindChildByName(transform, "CurrentPlayersSegment");
            _currentPlayersText = FindTextByObjectName(currentPlayersSegment, "ValueText");
        }

        if (_readyPlayersText == null)
        {
            Transform readyPlayersSegment = FindChildByName(transform, "ReadyPlayersSegment");
            _readyPlayersText = FindTextByObjectName(readyPlayersSegment, "ValueText");
        }
    }

    private static TMP_Text FindTextByObjectName(Transform root, string objectName)
    {
        if (root == null) return null;

        TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null && texts[i].gameObject.name == objectName)
            {
                return texts[i];
            }
        }

        return null;
    }

    private static Transform FindChildByName(Transform root, string childName)
    {
        if (root == null) return null;

        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] != null && children[i].name == childName)
            {
                return children[i];
            }
        }

        return null;
    }
}
