using System;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;

public class LobbyPlayerSlotsView : MonoBehaviour
{
    [SerializeField] private Transform _playerSlotRoot;
    [SerializeField] private LobbyPlayerSlotBinding[] _playerSlots;

    private bool _isDestroyed;

    public static LobbyPlayerSlotsView EnsureInScene()
    {
        GameObject slotRoot = GameObject.Find("LobbyCenterPlayerSlots");
        if (slotRoot == null) return null;

        LobbyPlayerSlotsView view = slotRoot.GetComponent<LobbyPlayerSlotsView>();
        if (view == null)
        {
            view = slotRoot.AddComponent<LobbyPlayerSlotsView>();
        }

        return view;
    }

    private void Awake()
    {
        AutoWirePlayerSlots();
    }

    private void OnEnable()
    {
        SubscribeLobbyEvents();
        RefreshFromLobbySession();
    }

    private async void Start()
    {
        if (LobbyManager.Instance == null) return;

        await LobbyManager.Instance.WaitForOwnSlotAsync(5f);
        if (_isDestroyed) return;

        RefreshFromLobbySession();
    }

    private void OnDisable()
    {
        UnsubscribeLobbyEvents();
    }

    private void OnDestroy()
    {
        _isDestroyed = true;
        UnsubscribeLobbyEvents();
    }

    private void SubscribeLobbyEvents()
    {
        if (LobbyManager.Instance == null) return;

        LobbyManager.Instance.OnSessionUpdated += RefreshFromLobbySession;
        LobbyManager.Instance.OnSessionLeft += HandleSessionLeft;
        LobbyManager.Instance.OnRestartCooldownEnded += RefreshFromLobbySession;
    }

    private void UnsubscribeLobbyEvents()
    {
        if (LobbyManager.Instance == null) return;

        LobbyManager.Instance.OnSessionUpdated -= RefreshFromLobbySession;
        LobbyManager.Instance.OnSessionLeft -= HandleSessionLeft;
        LobbyManager.Instance.OnRestartCooldownEnded -= RefreshFromLobbySession;
    }

    private void HandleSessionLeft()
    {
        RefreshPlayerSlots(null);
    }

    private void RefreshFromLobbySession()
    {
        RefreshFromLobbySession(LobbyManager.Instance != null ? LobbyManager.Instance.CurrentSession : null);
    }

    private void RefreshFromLobbySession(ISession session)
    {
        RefreshPlayerSlots(session);
    }

    private void AutoWirePlayerSlots()
    {
        if (_playerSlotRoot == null)
        {
            GameObject slotRoot = GameObject.Find("LobbyCenterPlayerSlots");
            if (slotRoot != null) _playerSlotRoot = slotRoot.transform;
        }

        if (_playerSlotRoot == null) return;

        if (_playerSlots == null || _playerSlots.Length < 4)
        {
            _playerSlots = new LobbyPlayerSlotBinding[4];
        }

        for (int i = 0; i < _playerSlots.Length; i++)
        {
            if (_playerSlots[i] == null)
            {
                _playerSlots[i] = new LobbyPlayerSlotBinding();
            }
        }

        for (int i = 0; i < _playerSlotRoot.childCount; i++)
        {
            Transform slotRoot = _playerSlotRoot.GetChild(i);
            int slotIndex = ResolveSlotIndex(slotRoot, i);
            if (slotIndex < 0 || slotIndex >= _playerSlots.Length) continue;

            LobbyPlayerSlotBinding slot = _playerSlots[slotIndex];
            slot.Root = slotRoot;
            if (slot.ClassText == null)
                slot.ClassText = FindChildComponent<TMP_Text>(slotRoot, "ClassText");
            if (slot.StateText == null)
                slot.StateText = FindChildComponent<TMP_Text>(slotRoot, "StateText");
        }
    }

    private int ResolveSlotIndex(Transform slotRoot, int fallbackIndex)
    {
        TMP_Text numberText = FindChildComponent<TMP_Text>(slotRoot, "NumberText");
        if (numberText != null && int.TryParse(numberText.text, out int displayNumber))
        {
            return displayNumber - 1;
        }

        string slotName = slotRoot.name;
        for (int i = slotName.Length - 1; i >= 0; i--)
        {
            if (!char.IsDigit(slotName[i])) continue;
            return (slotName[i] - '0') - 1;
        }

        return fallbackIndex;
    }

    private void RefreshPlayerSlots(ISession session)
    {
        AutoWirePlayerSlots();
        if (_playerSlots == null) return;

        int visibleSlotCount = session != null && session.MaxPlayers > 0
            ? Mathf.Min(session.MaxPlayers, _playerSlots.Length)
            : _playerSlots.Length;

        for (int slotIndex = 0; slotIndex < _playerSlots.Length; slotIndex++)
        {
            bool isVisibleSlot = slotIndex < visibleSlotCount;
            SetPlayerSlotVisible(slotIndex, isVisibleSlot);
            if (!isVisibleSlot) continue;

            IReadOnlyPlayer player = FindPlayerInSlot(session, slotIndex);
            if (player == null)
            {
                SetPlayerSlot(slotIndex, "빈 슬롯", "참가 대기중");
                continue;
            }

            PlayerInfo info = LobbyManager.Instance.GetPlayerInfo(player);
            if (!info.HasClass)
            {
                SetPlayerSlot(slotIndex, "빈 슬롯", "미준비");
                continue;
            }

            bool isHost = player.Id == session.Host;
            string stateText = isHost || info.IsReady ? "준비 완료" : "미준비";
            SetPlayerSlot(slotIndex, GetClassDisplayName(info.PlayerClass), stateText);
        }
    }

    private void SetPlayerSlotVisible(int slotIndex, bool isVisible)
    {
        if (_playerSlots == null || slotIndex < 0 || slotIndex >= _playerSlots.Length) return;

        LobbyPlayerSlotBinding slot = _playerSlots[slotIndex];
        if (slot?.Root != null)
        {
            slot.Root.gameObject.SetActive(isVisible);
        }
    }

    private IReadOnlyPlayer FindPlayerInSlot(ISession session, int targetSlot)
    {
        if (session == null || LobbyManager.Instance == null) return null;

        for (int i = 0; i < session.Players.Count; i++)
        {
            IReadOnlyPlayer player = session.Players[i];
            PlayerInfo info = LobbyManager.Instance.GetPlayerInfo(player);
            if (info.SlotIndex == targetSlot) return player;
        }

        return null;
    }

    private void SetPlayerSlot(int slotIndex, string classText, string stateText)
    {
        if (_playerSlots == null || slotIndex < 0 || slotIndex >= _playerSlots.Length) return;

        LobbyPlayerSlotBinding slot = _playerSlots[slotIndex];
        if (slot == null) return;

        if (slot.ClassText != null) slot.ClassText.text = classText;
        if (slot.StateText != null) slot.StateText.text = stateText;
    }

    private static string GetClassDisplayName(PlayerClass playerClass)
    {
        switch (playerClass)
        {
            case PlayerClass.Melee:
                return "근접";
            case PlayerClass.Rifle:
                return "돌격소총";
            case PlayerClass.Shotgun:
                return "샷건";
            case PlayerClass.Pistol:
                return "유틸";
            default:
                return "빈 슬롯";
        }
    }

    private static T FindChildComponent<T>(Transform root, string childName) where T : Component
    {
        GameObject child = FindChildGameObject(root, childName);
        return child != null ? child.GetComponent<T>() : null;
    }

    private static GameObject FindChildGameObject(Transform root, string childName)
    {
        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].name == childName) return children[i].gameObject;
        }

        return null;
    }
}

[Serializable]
public class LobbyPlayerSlotBinding
{
    public Transform Root;
    public TMP_Text ClassText;
    public TMP_Text StateText;
}
