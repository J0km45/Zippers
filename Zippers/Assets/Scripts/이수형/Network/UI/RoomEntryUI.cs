using System;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 방 목록의 한 줄을 표현하는 패시브 뷰 컴포넌트.
/// RoomListController 가 prefab 으로 instantiate 후 Setup 호출.
/// </summary>
public class RoomEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _playerCountText;
    [SerializeField] private Button _joinButton;

    private ISessionInfo _sessionInfo;
    private Action<ISessionInfo> _onJoinClicked;

    private void OnEnable()
    {
        if (_joinButton != null) _joinButton.onClick.AddListener(InvokeJoin);
    }

    private void OnDisable()
    {
        if (_joinButton != null) _joinButton.onClick.RemoveListener(InvokeJoin);
    }

    /// <summary>UI 에 세션 정보를 채우고 참여 콜백 연결.</summary>
    public void Setup(ISessionInfo sessionInfo, Action<ISessionInfo> onJoinClicked)
    {
        _sessionInfo = sessionInfo;
        _onJoinClicked = onJoinClicked;

        if (_nameText != null) _nameText.text = sessionInfo.Name;
        if (_playerCountText != null)
        {
            int currentPlayers = sessionInfo.MaxPlayers - sessionInfo.AvailableSlots;
            _playerCountText.text = $"{currentPlayers} / {sessionInfo.MaxPlayers}";
        }
    }

    /// <summary>다른 비동기 작업 진행 중일 때 일괄 비활성용.</summary>
    public void SetInteractable(bool interactable)
    {
        if (_joinButton != null) _joinButton.interactable = interactable;
    }

    private void InvokeJoin()
    {
        if (_sessionInfo == null) return;
        _onJoinClicked?.Invoke(_sessionInfo);
    }
}
