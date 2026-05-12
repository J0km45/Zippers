using System;
using TMPro;
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

    private void Awake()
    {
        BindEvents();

        // 처음 켜질 때 UI 상태를 한 번 갱신
        RefreshAllItems();

        // 상세 설명 패널은 처음에는 닫아두기
        SetDetailVisible(false);
    }

    private void OnDestroy()
    {
        UnbindEvents();
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
        for (int i = 0; i < _items.Length; i++)
        {
            // 람다 안에서 i 값이 꼬이지 않게 index로 복사
            int index = i;

            if (_items[index].Button != null)
                _items[index].Button.onClick.AddListener(() => SelectClass(index));
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
            if (_items[i].Button != null)
                _items[i].Button.onClick.RemoveAllListeners();
        }
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

    private void SelectClass(int index)
    {
        // 잘못된 인덱스 방지
        if (_items == null || index < 0 || index >= _items.Length) return;

        ClassItemBinding item = _items[index];

        // 잠긴 클래스는 선택 x
        if (item.IsLocked) return;

        // 선택된 클래스를 저장
        _selectedClass = item.ClassType;

        
        RefreshAllItems();

        OnClassSelected?.Invoke(_selectedClass);
    }

    private void RefreshAllItems()
    {
        if (_items == null) return;

        // 모든 클래스 아이템 UI를 갱신
        for (int i = 0; i < _items.Length; i++)
            RefreshItem(_items[i]);
    }

    private void RefreshItem(ClassItemBinding item)
    {
        bool isSelected = item.ClassType == _selectedClass;
        bool isLocked = item.IsLocked;

        // 잠긴 클래스는 버튼 클릭 x 
        if (item.Button != null)
            item.Button.interactable = !isLocked;

        // 클래스 이름 표시
        if (item.NameText != null)
            item.NameText.text = item.DisplayName;

        // 상태 텍스트 표시
        if (item.StateText != null)
            item.StateText.text = isLocked ? "선택 불가" : isSelected ? "선택 중" : "선택 가능";

        // 잠금 아이콘 표시/숨김
        if (item.LockIcon != null)
            item.LockIcon.SetActive(isLocked);
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