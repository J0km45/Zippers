using System;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

public class LobbyClassSelectView : MonoBehaviour
{
    // 외부 스크립트가 클래스 선택 결과를 받을 때 사용
    public event Action<LobbyClassType> OnClassSelected;

    [Header("Panel")] [SerializeField] private GameObject _panel; // 클래스 선택 패널 전체
    [SerializeField] private Button _closeButton; // X 닫기 버튼
    [SerializeField] private Button _detailButton; // 상세 설명 열기 버튼
    [SerializeField] private GameObject _detailPanel; // 상세 설명 패널

    [Header("Class Items")] [SerializeField]
    private ClassItemBinding[] _items; // 클래스 버튼 목록

    // 현재 선택된 클래스
    private LobbyClassType _selectedClass = LobbyClassType.None;
    private Action[] _classClickHandlers;
    private bool _isRequestingClass;
    private bool _isDestroyed;

    private void Awake()
    {
        EnsureClassItems();
        AutoWireMissingItems();
        // 중앙 고정 슬롯 UI 대신 캐릭터 머리 위 nameplate UI를 사용한다.
        LobbyAvatarNameplateManager.EnsureInScene();
        LobbyLeaveButtonController.EnsureInScene();
        LobbyReadyButtonController.EnsureInScene();
        LobbyStartGameButtonController.EnsureInScene();
        BindEvents();

        // 처음 켜질 때 UI 상태를 한 번 갱신
        RefreshAllItems();

        // 상세 설명 패널은 처음에는 닫아두기
        SetDetailVisible(false);
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
        UnbindEvents();
        UnsubscribeLobbyEvents();
    }

    private void BindEvents()
    {
        // 닫기 버튼 연결
        if (_closeButton != null)
            _closeButton.onClick.AddListener(Close);

        // 상세 설명 버튼 연결
        if (_detailButton != null)
            _detailButton.onClick.AddListener(ToggleDetail);

        if (_items == null) return;

        // 클래스 버튼들을 순서대로 연결
        _classClickHandlers = new Action[_items.Length];
        for (int i = 0; i < _items.Length; i++)
        {
            // 람다 안에서 i 값이 꼬이지 않게 index로 복사
            int index = i;
            _classClickHandlers[index] = () => SelectClass(index);

            if (_items[index].Button != null)
                _items[index].Button.onClick.AddListener(_classClickHandlers[index].Invoke);
        }
    }

    private void UnbindEvents()
    {
        // 닫기 버튼 이벤트 해제
        if (_closeButton != null)
            _closeButton.onClick.RemoveListener(Close);

        // 상세 설명 버튼 이벤트 해제
        if (_detailButton != null)
            _detailButton.onClick.RemoveListener(ToggleDetail);

        if (_items == null) return;

        // 클래스 버튼 이벤트 해제
        for (int i = 0; i < _items.Length; i++)
        {
            if (_items[i].Button != null && _classClickHandlers != null && i < _classClickHandlers.Length)
                _items[i].Button.onClick.RemoveListener(_classClickHandlers[i].Invoke);
        }
    }

    private void SubscribeLobbyEvents()
    {
        if (LobbyManager.Instance == null) return;

        LobbyManager.Instance.OnSessionUpdated += HandleSessionUpdated;
        LobbyManager.Instance.OnSessionLeft += HandleSessionLeft;
        LobbyManager.Instance.OnRestartCooldownEnded += RefreshFromLobbySession;
    }

    private void UnsubscribeLobbyEvents()
    {
        if (LobbyManager.Instance == null) return;

        LobbyManager.Instance.OnSessionUpdated -= HandleSessionUpdated;
        LobbyManager.Instance.OnSessionLeft -= HandleSessionLeft;
        LobbyManager.Instance.OnRestartCooldownEnded -= RefreshFromLobbySession;
    }

    private void HandleSessionUpdated(ISession session)
    {
        RefreshFromLobbySession(session);
    }

    private void HandleSessionLeft()
    {
        _selectedClass = LobbyClassType.None;
        RefreshAllItems();
    }

    public void Open()
    {
        // 패널 열기
        if (_panel != null)
            _panel.SetActive(true);
    }

    public void Close()
    {
        // 패널 닫기
        if (_panel != null)
            _panel.SetActive(false);

        SetDetailVisible(false);
    }

    private void ToggleDetail()
    {
        if (_detailPanel == null) return;

        SetDetailVisible(!_detailPanel.activeSelf);
    }

    private void SetDetailVisible(bool visible)
    {
        if (_detailPanel != null)
            _detailPanel.SetActive(visible);
    }

    private async void SelectClass(int index)
    {
        // 잘못된 인덱스 방지
        if (_isRequestingClass || _items == null || index < 0 || index >= _items.Length) return;

        ClassItemBinding item = _items[index];

        // 잠긴 클래스는 선택 x
        if (item.IsLocked || IsClassTakenByOther(item.ClassType)) return;

        LobbyClassType requestedClass = item.ClassType;
        LobbyClassType nextClass = _selectedClass == requestedClass ? LobbyClassType.None : requestedClass;

        if (LobbyManager.Instance == null || LobbyManager.Instance.CurrentSession == null)
        {
            _selectedClass = nextClass;
            RefreshAllItems();
            OnClassSelected?.Invoke(_selectedClass);
            return;
        }

        _isRequestingClass = true;
        RefreshAllItems();

        bool success = false;
        try
        {
            success = await LobbyManager.Instance.SetClassAsync(ToPlayerClass(nextClass));
        }
        finally
        {
            if (!_isDestroyed)
            {
                _isRequestingClass = false;
            }
        }

        if (_isDestroyed) return;

        if (success)
        {
            _selectedClass = nextClass;
            OnClassSelected?.Invoke(_selectedClass);
        }

        RefreshFromLobbySession();
    }

    private void RefreshAllItems()
    {
        if (_items == null) return;

        // 모든 클래스 아이템 UI를 갱신
        for (int i = 0; i < _items.Length; i++)
            RefreshItem(_items[i]);
    }

    private void EnsureClassItems()
    {
        if (_items == null || _items.Length < 4)
        {
            _items = new ClassItemBinding[4];
        }

        EnsureItem(0, LobbyClassType.Melee, "근접");
        EnsureItem(1, LobbyClassType.Rifle, "돌격소총");
        EnsureItem(2, LobbyClassType.Shotgun, "샷건");
        EnsureItem(3, LobbyClassType.Pistol, "피스톨");
    }

    private void EnsureItem(int index, LobbyClassType classType, string displayName)
    {
        if (_items[index] == null)
        {
            _items[index] = new ClassItemBinding();
        }

        if (_items[index].ClassType == LobbyClassType.None || _items[index].ClassType != classType)
        {
            _items[index].ClassType = classType;
        }

        if (string.IsNullOrEmpty(_items[index].DisplayName))
        {
            _items[index].DisplayName = displayName;
        }
    }

    private void AutoWireMissingItems()
    {
        if (_items == null) return;

        for (int i = 0; i < _items.Length; i++)
        {
            ClassItemBinding item = _items[i];
            if (item == null) continue;

            Transform itemRoot = FindClassItemRoot(item.ClassType);
            if (itemRoot == null) continue;

            if (item.Button == null)
                item.Button = itemRoot.GetComponent<Button>() ?? itemRoot.GetComponentInChildren<Button>(true);

            if (item.NameText == null)
                item.NameText = FindChildComponent<TMP_Text>(itemRoot, "NameText");

            if (item.StateText == null)
                item.StateText = FindChildComponent<TMP_Text>(itemRoot, "StateText");

            if (item.LockIcon == null)
                item.LockIcon = FindChildGameObject(itemRoot, "LockIcon");
        }
    }

    private Transform FindClassItemRoot(LobbyClassType classType)
    {
        string primaryToken = GetClassSearchToken(classType);

        Transform[] children = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            string childName = children[i].name;
            if (!childName.Contains("ClassItem")) continue;
            if (childName.Contains(primaryToken))
                return children[i];
        }

        return null;
    }

    private static string GetClassSearchToken(LobbyClassType classType)
    {
        switch (classType)
        {
            case LobbyClassType.Melee:
                return "Melee";
            case LobbyClassType.Rifle:
                return "Rifle";
            case LobbyClassType.Shotgun:
                return "Shotgun";
            case LobbyClassType.Pistol:
                return "Pistol";
            default:
                return string.Empty;
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

    private void RefreshItem(ClassItemBinding item)
    {
        bool isSelected = item.ClassType == _selectedClass;
        bool isLocked = item.IsLocked || IsClassTakenByOther(item.ClassType) || _isRequestingClass;

        // 잠긴 클래스는 버튼 클릭 x 
        if (item.Button != null)
            item.Button.interactable = !isLocked || isSelected;

        // 클래스 이름 표시
        if (item.NameText != null)
            item.NameText.text = item.DisplayName;

        // 상태 텍스트 표시
        if (item.StateText != null)
            item.StateText.text = GetStateText(item, isSelected, isLocked);

        // 잠금 아이콘 표시/숨김
        if (item.LockIcon != null)
            item.LockIcon.SetActive(isLocked && !isSelected);
    }

    private string GetStateText(ClassItemBinding item, bool isSelected, bool isLocked)
    {
        if (_isRequestingClass) return "처리 중";
        if (isSelected) return "선택 중";
        if (item.IsLocked) return "선택 불가";
        if (IsClassTakenByOther(item.ClassType)) return "다른 플레이어 선택 중";
        if (isLocked) return "선택 불가";
        return "선택 가능";
    }

    private void RefreshFromLobbySession()
    {
        RefreshFromLobbySession(LobbyManager.Instance != null ? LobbyManager.Instance.CurrentSession : null);
    }

    private void RefreshFromLobbySession(ISession session)
    {
        if (session?.CurrentPlayer != null && LobbyManager.Instance != null)
        {
            PlayerInfo myInfo = LobbyManager.Instance.GetPlayerInfo(session.CurrentPlayer);
            _selectedClass = ToLobbyClassType(myInfo.PlayerClass);
        }

        RefreshAllItems();
    }

    private bool IsClassTakenByOther(LobbyClassType classType)
    {
        if (classType == LobbyClassType.None || LobbyManager.Instance == null) return false;

        ISession session = LobbyManager.Instance.CurrentSession;
        if (session == null) return false;

        PlayerClass playerClass = ToPlayerClass(classType);
        string ownId = session.CurrentPlayer?.Id;

        for (int i = 0; i < session.Players.Count; i++)
        {
            IReadOnlyPlayer player = session.Players[i];
            if (player.Id == ownId) continue;

            PlayerInfo info = LobbyManager.Instance.GetPlayerInfo(player);
            if (info.PlayerClass == playerClass) return true;
        }

        return false;
    }

    private static PlayerClass ToPlayerClass(LobbyClassType classType)
    {
        switch (classType)
        {
            case LobbyClassType.Melee:
                return PlayerClass.Melee;
            
            case LobbyClassType.Rifle:
                return PlayerClass.Rifle;
            
            case LobbyClassType.Shotgun:
                return PlayerClass.Shotgun;
            
            case LobbyClassType.Pistol:
                return PlayerClass.Pistol;
            default:
                return PlayerClass.None;
        }
    }

    private static LobbyClassType ToLobbyClassType(PlayerClass playerClass)
    {
        switch (playerClass)
        {
            case PlayerClass.Melee:
                return LobbyClassType.Melee;
            case PlayerClass.Rifle:
                return LobbyClassType.Rifle;
            case PlayerClass.Shotgun:
                return LobbyClassType.Shotgun;
            case PlayerClass.Pistol:
                return LobbyClassType.Pistol;
            default:
                return LobbyClassType.None;
        }
    }

    public LobbyClassType GetSelectedClass()
    {
        return _selectedClass;
    }

    public void SetLocked(LobbyClassType classType, bool locked)
    {
        if (_items == null) return;

        // 특정 클래스의 잠금 상태 바꾸기
        for (int i = 0; i < _items.Length; i++)
        {
            if (_items[i].ClassType != classType) continue;

            _items[i].IsLocked = locked;
            RefreshItem(_items[i]);
            return;
        }
    }
}

[Serializable]
public class ClassItemBinding
{
    public LobbyClassType ClassType; // 이 버튼이 의미하는 클래스
    public string DisplayName; // 화면에 표시할 클래스 이름
    public bool IsLocked; // 선택 불가능 여부

    [Header("UI")] public Button Button; // 클래스 버튼
    public TMP_Text NameText; // 클래스 이름 텍스트
    public TMP_Text StateText; // 선택 가능/불가/선택 중 텍스트
    public GameObject LockIcon; // 잠금 아이콘 오브젝트
}
