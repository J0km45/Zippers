using System;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

public class RoomEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _playerCountText;
    [SerializeField] private TMP_Text _statusText;
    [SerializeField] private Button _joinButton;
    [SerializeField] private TMP_Text _selectButtonText;
    [SerializeField] private Image _backgroundImage;

    private static readonly Color NormalBackgroundColor = new Color(1f, 1f, 1f, 1f);
    private static readonly Color SelectedBackgroundColor = new Color(0.80f, 0.93f, 1f, 1f);

    private ISessionInfo _sessionInfo;
    private Action<RoomEntryUI> _onSelectClicked;

    public ISessionInfo SessionInfo => _sessionInfo;

    private void Awake()
    {
        EnsureTextReferences();

        if (_joinButton == null || _joinButton.gameObject == gameObject)
        {
            // 루트 버튼이 아닌 실제 선택 버튼을 우선 사용
            Button selectButton = FindChildButtonByName("SelectButtonSlot", "SelectButton", "Main Button");
            if (selectButton != null)
            {
                _joinButton = selectButton;
            }
        }

        if (_selectButtonText == null)
        {
            // 방 이름 텍스트가 선택 버튼 텍스트로 잡히지 않도록 선택 영역 안에서만 탐색
            _selectButtonText = FindTextUnderNamedChild("SelectButtonSlot", "SelectButton", "Main Button");
            if (_selectButtonText == null && _joinButton != null && _joinButton.gameObject != gameObject)
            {
                _selectButtonText = _joinButton.GetComponentInChildren<TMP_Text>(true);
            }
        }

        if (_backgroundImage == null)
        {
            _backgroundImage = GetComponent<Image>();
        }

        if (_statusText == null)
        {
            _statusText = FindChildTextByName("StatusText", "StateText");
        }
    }

    private void OnEnable()
    {
        if (_joinButton != null) _joinButton.onClick.AddListener(InvokeSelect);
    }

    private void OnDisable()
    {
        if (_joinButton != null) _joinButton.onClick.RemoveListener(InvokeSelect);
    }

    public void Setup(ISessionInfo sessionInfo, Action<RoomEntryUI> onSelectClicked)
    {
        _sessionInfo = sessionInfo;
        _onSelectClicked = onSelectClicked;
        EnsureTextReferences();

        // 세션 정보로 방 제목과 인원 수를 갱신
        if (_nameText != null) _nameText.text = string.IsNullOrWhiteSpace(sessionInfo.Name) ? "Room" : sessionInfo.Name;
        if (_playerCountText != null)
        {
            int currentPlayers = sessionInfo.MaxPlayers - sessionInfo.AvailableSlots;
            _playerCountText.text = $"{currentPlayers} / {sessionInfo.MaxPlayers}";
        }

        if (_statusText != null)
        {
            _statusText.text = GetRoomStateText(sessionInfo);
        }

        SetSelected(false);
    }

    public void Setup(ISessionInfo sessionInfo, Action<ISessionInfo> onJoinClicked)
    {
        Setup(sessionInfo, (Action<RoomEntryUI>)(_ => onJoinClicked?.Invoke(sessionInfo)));
    }

    public void SetSelected(bool selected)
    {
        if (_selectButtonText != null)
        {
            // 선택 상태는 선택 버튼 텍스트와 배경색으로 표시
            _selectButtonText.text = selected ? "선택됨" : "선택";
        }

        if (_backgroundImage != null)
        {
            _backgroundImage.color = selected ? SelectedBackgroundColor : NormalBackgroundColor;
        }
    }

    public void SetInteractable(bool interactable)
    {
        if (_joinButton != null) _joinButton.interactable = interactable;
    }

    private void InvokeSelect()
    {
        if (_sessionInfo == null) return;
        _onSelectClicked?.Invoke(this);
    }

    private void EnsureTextReferences()
    {
        if (_nameText == null)
        {
            _nameText = FindChildTextByName("RoomNameText", "NameText", "RoomName");
        }

        if (_playerCountText == null)
        {
            _playerCountText = FindChildTextByName("PlayerCountText", "CountText", "PlayerCount");
        }

        if (_statusText == null)
        {
            _statusText = FindChildTextByName("StatusText", "StateText");
        }
    }

    private TMP_Text FindChildTextByName(params string[] names)
    {
        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text text = texts[i];
            if (text == null) continue;

            for (int j = 0; j < names.Length; j++)
            {
                if (text.gameObject.name == names[j])
                {
                    return text;
                }
            }
        }

        return null;
    }

    private Button FindChildButtonByName(params string[] names)
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null || button.gameObject == gameObject) continue;

            for (int j = 0; j < names.Length; j++)
            {
                if (button.gameObject.name == names[j] || HasParentNamed(button.transform, names[j]))
                {
                    return button;
                }
            }
        }

        return null;
    }

    private TMP_Text FindTextUnderNamedChild(params string[] names)
    {
        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text text = texts[i];
            if (text == null) continue;

            for (int j = 0; j < names.Length; j++)
            {
                if (HasParentNamed(text.transform, names[j]))
                {
                    return text;
                }
            }
        }

        return null;
    }

    private static bool HasParentNamed(Transform child, string parentName)
    {
        Transform current = child;
        while (current != null)
        {
            if (current.name == parentName)
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private static string GetRoomStateText(ISessionInfo sessionInfo)
    {
        return sessionInfo.IsLocked ? "진행" : "모집";
    }
}
