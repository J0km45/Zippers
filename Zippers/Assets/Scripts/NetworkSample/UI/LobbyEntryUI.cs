using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Multiplayer;
using TMPro;

/// <summary>
/// 세션 목록의 한 줄(방 하나)을 표현하는 UI
/// </summary>
public class LobbyEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _playerCountText;
    [SerializeField] private Button _joinButton;

    private ISessionInfo _sessionInfo;
    private Action<ISessionInfo> _onJoinClicked;

    private void OnEnable()
    {
        BindButtonEvents();
    }

    private void OnDisable()
    {
        UnbindButtonEvents();
    }

    /// <summary>
    /// UI에 세션 정보를 채우고 참여 콜백 연결
    /// </summary>
    /// <param name="sessionInfo">표시할 세션</param>
    /// <param name="onJoinClicked">참여 버튼 클릭 시 호출될 콜백</param>
    public void Setup(ISessionInfo sessionInfo, Action<ISessionInfo> onJoinClicked)
    {
        _sessionInfo = sessionInfo;
        _onJoinClicked = onJoinClicked;
        _nameText.text = sessionInfo.Name;
        int currentPlayers = sessionInfo.MaxPlayers - sessionInfo.AvailableSlots;
        _playerCountText.text = $"{currentPlayers} / {sessionInfo.MaxPlayers}";
    }

    /// <summary>
    /// 항목의 참여 버튼 활성/비활성. 다른 비동기 작업 진행 중일 때 일괄 비활성용
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        _joinButton.interactable = interactable;
    }

    private void BindButtonEvents()
    {
        _joinButton.onClick.AddListener(InvokeJoin);
    }

    private void UnbindButtonEvents()
    {
        _joinButton.onClick.RemoveListener(InvokeJoin);
    }

    private void InvokeJoin()
    {
        if (_sessionInfo == null) return;
        _onJoinClicked?.Invoke(_sessionInfo);
    }
}
