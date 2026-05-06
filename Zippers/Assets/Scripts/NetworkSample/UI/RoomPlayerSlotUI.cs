using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 룸 내 플레이어 1명을 표시하는 슬롯 UI
/// </summary>
public class RoomPlayerSlotUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _playerNameText;
    [SerializeField] private TMP_Text _readyText;
    [SerializeField] private Image _readyIndicator;
    [SerializeField] private GameObject _hostBadge;
    [SerializeField] private GameObject _emptyLabel;
    [SerializeField] private GameObject _filledContent;
    [SerializeField] private Color _readyColor;
    [SerializeField] private Color _notReadyColor;
    [SerializeField] private Color _emptyColor;
    [SerializeField] private Color _hostColor;

    /// <summary>
    /// 빈 슬롯 상태로 표시
    /// </summary>
    public void SetEmpty()
    {
        _filledContent.SetActive(false);
        _emptyLabel.SetActive(true);
        _hostBadge.SetActive(false);
        _readyIndicator.color = _emptyColor;
        _playerNameText.text = "-";
        _readyText.text = "빈 자리";
    }

    /// <summary>
    /// 실제 플레이어 정보로 슬롯 채움
    /// </summary>
    /// <param name="playerName">플레이어 이름</param>
    /// <param name="isReady">레디 상태</param>
    /// <param name="isHost">호스트 여부</param>
    public void SetPlayer(string playerName, bool isReady, bool isHost)
    {
        _filledContent.SetActive(true);
        _emptyLabel.SetActive(false);
        _playerNameText.text = playerName;
        _hostBadge.SetActive(isHost);

        if (isHost)
        {
            _readyText.text = "방장";
            _readyIndicator.color = _hostColor;
        }
        else
        {
            _readyText.text = isReady ? "준비 완료" : "대기중";
            _readyIndicator.color = isReady ? _readyColor : _notReadyColor;
        }
    }
}
