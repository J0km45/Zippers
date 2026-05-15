using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

public class LobbyAvatarNameplateManager : MonoBehaviour
{
    private const string CenterSlotsName = "LobbyCenterPlayerSlots";
    private const string ClassTextName = "ClassText";
    private const string StateTextName = "StateText";
    private const string NumberTextName = "NumberText";

    private static LobbyAvatarNameplateManager _instance;

    [SerializeField] private Vector3 _localOffset = new Vector3(0f, 2.2f, 0f);

    private readonly Dictionary<string, NameplateBinding> _nameplates = new Dictionary<string, NameplateBinding>();
    private RectTransform _slotTemplate;
    private readonly RectTransform[] _slotTemplates = new RectTransform[4];
    private Canvas _screenCanvas;
    private RectTransform _screenCanvasRect;
    private RectTransform _nameplateRoot;
    private bool _isDestroyed;

    public static LobbyAvatarNameplateManager EnsureInScene()
    {
        if (_instance != null) return _instance;

        LobbyAvatarNameplateManager existing = FindFirstObjectByType<LobbyAvatarNameplateManager>();
        if (existing != null)
        {
            _instance = existing;
            return _instance;
        }

        GameObject root = new GameObject("LobbyAvatarNameplateManager");
        _instance = root.AddComponent<LobbyAvatarNameplateManager>();
        return _instance;
    }

    public static void AttachNameplate(NetworkObject avatar, string playerId)
    {
        if (avatar == null || string.IsNullOrEmpty(playerId)) return;

        EnsureInScene().AttachInternal(avatar, playerId);
    }

    public static void DetachNameplate(string playerId)
    {
        if (_instance == null || string.IsNullOrEmpty(playerId)) return;

        _instance.DetachInternal(playerId);
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        CacheTemplateAndHideCenterSlots();
    }

    private void OnEnable()
    {
        SubscribeLobbyEvents();
        RefreshAll();
    }

    private void OnDisable()
    {
        UnsubscribeLobbyEvents();
    }

    private void OnDestroy()
    {
        _isDestroyed = true;
        UnsubscribeLobbyEvents();
        ClearAll();

        if (_instance == this) _instance = null;
    }

    private void LateUpdate()
    {
        UpdateNameplatePositions();
    }

    private void UpdateNameplatePositions()
    {
        Camera camera = Camera.main;
        if (camera == null || _screenCanvasRect == null) return;

        // 캐릭터 머리 위 월드 좌표를 화면 Canvas 좌표로 변환해서 UI 배치
        List<NameplateBinding> bindings = new List<NameplateBinding>(_nameplates.Values);
        for (int i = 0; i < bindings.Count; i++)
        {
            NameplateBinding binding = bindings[i];
            if (binding == null || binding.Root == null || binding.AvatarRoot == null) continue;

            Vector3 screenPosition = camera.WorldToScreenPoint(binding.AvatarRoot.position + _localOffset);
            bool isVisible = screenPosition.z > 0f;
            binding.Root.gameObject.SetActive(isVisible);
            if (!isVisible) continue;

            Camera uiCamera = _screenCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _screenCanvas.worldCamera;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _screenCanvasRect,
                    screenPosition,
                    uiCamera,
                    out Vector2 localPoint))
            {
                binding.Root.anchoredPosition = localPoint;
            }
        }
    }

    private void SubscribeLobbyEvents()
    {
        if (LobbyManager.Instance == null) return;

        LobbyManager.Instance.OnSessionUpdated += RefreshFromSession;
        LobbyManager.Instance.OnSessionLeft += ClearAll;
        LobbyManager.Instance.OnRestartCooldownEnded += RefreshAll;
    }

    private void UnsubscribeLobbyEvents()
    {
        if (LobbyManager.Instance == null) return;

        LobbyManager.Instance.OnSessionUpdated -= RefreshFromSession;
        LobbyManager.Instance.OnSessionLeft -= ClearAll;
        LobbyManager.Instance.OnRestartCooldownEnded -= RefreshAll;
    }

    private void CacheTemplateAndHideCenterSlots()
    {
        GameObject centerSlots = GameObject.Find(CenterSlotsName);
        if (centerSlots == null) return;

        // 기존 중앙 슬롯 UI의 디자인만 템플릿으로 사용하고, 실제 중앙 표시는 숨긴다.
        if (_screenCanvas == null)
        {
            _screenCanvas = centerSlots.GetComponentInParent<Canvas>();
            if (_screenCanvas != null)
            {
                _screenCanvasRect = _screenCanvas.transform as RectTransform;
            }
        }

        if (_slotTemplate == null)
        {
            for (int i = 0; i < centerSlots.transform.childCount; i++)
            {
                RectTransform child = centerSlots.transform.GetChild(i) as RectTransform;
                if (child == null) continue;

                _slotTemplate = child;
                break;
            }
        }

        CacheSlotTemplates(centerSlots.transform);

        // 원본 LobbyCenterPlayerSlots는 복제용으로만 남겨두고 클릭/표시는 막는다.
        CanvasGroup canvasGroup = centerSlots.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = centerSlots.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        EnsureNameplateRoot();
    }

    private void CacheSlotTemplates(Transform centerSlots)
    {
        // 슬롯별 원본을 따로 저장해서 방장 슬롯은 HostIcon, 일반 슬롯은 NumberText 디자인을 유지한다.
        for (int i = 0; i < centerSlots.childCount; i++)
        {
            RectTransform child = centerSlots.GetChild(i) as RectTransform;
            if (child == null) continue;

            int slotIndex = ResolveSlotIndex(child, i);
            if (slotIndex < 0 || slotIndex >= _slotTemplates.Length) continue;

            _slotTemplates[slotIndex] = child;
        }
    }

    private void EnsureNameplateRoot()
    {
        if (_nameplateRoot != null || _screenCanvasRect == null) return;

        // 머리 위 UI들은 기존 화면 Canvas 아래에서 일반 UI처럼 렌더링한다.
        GameObject root = new GameObject("LobbyAvatarNameplates", typeof(RectTransform));
        _nameplateRoot = root.transform as RectTransform;
        _nameplateRoot.SetParent(_screenCanvasRect, false);
        _nameplateRoot.anchorMin = Vector2.zero;
        _nameplateRoot.anchorMax = Vector2.one;
        _nameplateRoot.offsetMin = Vector2.zero;
        _nameplateRoot.offsetMax = Vector2.zero;
        _nameplateRoot.pivot = new Vector2(0.5f, 0.5f);
    }

    private void AttachInternal(NetworkObject avatar, string playerId)
    {
        CacheTemplateAndHideCenterSlots();
        if (_slotTemplate == null) return;
        if (_screenCanvasRect == null || _nameplateRoot == null) return;

        // 플레이어의 SlotIndex에 맞는 기존 슬롯 UI를 복제해야 아이콘/번호 디자인이 섞이지 않는다.
        int slotIndex = FindPlayerSlotIndex(playerId);
        RectTransform slotTemplate = GetSlotTemplate(slotIndex);

        if (_nameplates.TryGetValue(playerId, out NameplateBinding existing) &&
            existing != null &&
            existing.Root != null &&
            existing.AvatarRoot == avatar.transform &&
            existing.SlotIndex == slotIndex)
        {
            Refresh(existing);
            return;
        }

        DetachInternal(playerId);

        RectTransform slot = Instantiate(slotTemplate, _nameplateRoot);
        slot.name = "LobbyAvatarNameplate";
        slot.gameObject.SetActive(true);
        slot.anchorMin = new Vector2(0.5f, 0.5f);
        slot.anchorMax = new Vector2(0.5f, 0.5f);
        slot.pivot = new Vector2(0.5f, 0.5f);
        slot.anchoredPosition = Vector2.zero;
        slot.localScale = Vector3.one;

        LayoutElement layoutElement = slot.GetComponent<LayoutElement>();
        if (layoutElement != null) layoutElement.ignoreLayout = true;

        NameplateBinding binding = new NameplateBinding
        {
            PlayerId = playerId,
            SlotIndex = slotIndex,
            AvatarRoot = avatar.transform,
            Root = slot,
            ClassText = FindChildComponent<TMP_Text>(slot.transform, ClassTextName),
            StateText = FindChildComponent<TMP_Text>(slot.transform, StateTextName),
            NumberText = FindChildComponent<TMP_Text>(slot.transform, NumberTextName)
        };

        _nameplates[playerId] = binding;
        Refresh(binding);
        UpdateNameplatePositions();
    }

    private RectTransform GetSlotTemplate(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < _slotTemplates.Length && _slotTemplates[slotIndex] != null)
        {
            return _slotTemplates[slotIndex];
        }

        return _slotTemplate;
    }

    private void DetachInternal(string playerId)
    {
        if (!_nameplates.TryGetValue(playerId, out NameplateBinding binding)) return;

        if (binding != null && binding.Root != null)
        {
            Destroy(binding.Root.gameObject);
        }

        _nameplates.Remove(playerId);
    }

    private void RefreshAll()
    {
        RefreshFromSession(LobbyManager.Instance != null ? LobbyManager.Instance.CurrentSession : null);
    }

    private void RefreshFromSession(ISession session)
    {
        List<NameplateBinding> bindings = new List<NameplateBinding>(_nameplates.Values);
        for (int i = 0; i < bindings.Count; i++)
        {
            Refresh(bindings[i], session);
        }
    }

    private void Refresh(NameplateBinding binding)
    {
        Refresh(binding, LobbyManager.Instance != null ? LobbyManager.Instance.CurrentSession : null);
    }

    private void Refresh(NameplateBinding binding, ISession session)
    {
        if (binding == null || session == null || LobbyManager.Instance == null) return;

        IReadOnlyPlayer player = FindPlayer(session, binding.PlayerId);
        if (player == null)
        {
            DetachInternal(binding.PlayerId);
            return;
        }

        PlayerInfo info = LobbyManager.Instance.GetPlayerInfo(player);
        if (!info.HasClass)
        {
            DetachInternal(binding.PlayerId);
            return;
        }

        bool isHost = player.Id == session.Host;
        string stateText = isHost || info.IsReady ? "준비 완료" : "미준비";

        if (binding.ClassText != null) binding.ClassText.text = GetClassDisplayName(info.PlayerClass);
        if (binding.StateText != null) binding.StateText.text = stateText;
        if (binding.NumberText != null && info.SlotIndex >= 0) binding.NumberText.text = (info.SlotIndex + 1).ToString();
    }

    private int FindPlayerSlotIndex(string playerId)
    {
        ISession session = LobbyManager.Instance != null ? LobbyManager.Instance.CurrentSession : null;
        IReadOnlyPlayer player = FindPlayer(session, playerId);
        if (player == null || LobbyManager.Instance == null) return -1;

        return LobbyManager.Instance.GetPlayerInfo(player).SlotIndex;
    }

    private int ResolveSlotIndex(Transform slotRoot, int fallbackIndex)
    {
        TMP_Text numberText = FindChildComponent<TMP_Text>(slotRoot, NumberTextName);
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

    private IReadOnlyPlayer FindPlayer(ISession session, string playerId)
    {
        if (session == null || string.IsNullOrEmpty(playerId)) return null;

        for (int i = 0; i < session.Players.Count; i++)
        {
            IReadOnlyPlayer player = session.Players[i];
            if (player.Id == playerId) return player;
        }

        return null;
    }

    private void ClearAll()
    {
        List<string> playerIds = new List<string>(_nameplates.Keys);
        for (int i = 0; i < playerIds.Count; i++)
        {
            DetachInternal(playerIds[i]);
        }
    }

    public static IEnumerator WaitAndAttachNameplate(ulong networkObjectId, string playerId)
    {
        LobbyAvatarNameplateManager manager = EnsureInScene();

        // 클라이언트에서 NetworkObject spawn 동기화가 끝난 뒤 화면 UI를 연결한다.
        while (!manager._isDestroyed)
        {
            NetworkManager networkManager = NetworkManager.Singleton;
            if (networkManager != null &&
                networkManager.SpawnManager != null &&
                networkManager.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject avatar))
            {
                AttachNameplate(avatar, playerId);
                yield break;
            }

            yield return null;
        }
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
                return "권총";
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

    private class NameplateBinding
    {
        public string PlayerId;
        public int SlotIndex;
        public Transform AvatarRoot;
        public RectTransform Root;
        public TMP_Text ClassText;
        public TMP_Text StateText;
        public TMP_Text NumberText;
    }
}
