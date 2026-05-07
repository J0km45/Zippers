using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// 게임 씬 진입 후 다른 플레이어 합류를 기다리는 동안 표시되는 오버레이 UI
/// </summary>
public class GameWaitingUI : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TMP_Text _statusText;

    private bool _isBound;

    private void OnEnable()
    {
        ShowWaiting();
        StartCoroutine(BindWhenManagerReady());
    }

    private void OnDisable()
    {
        UnbindSessionManagerEvents();
    }

    private IEnumerator BindWhenManagerReady()
    {
        // NOTE: GameSessionManager는 NetworkBehaviour라 OnNetworkSpawn 이후에 이벤트 구독 가능
        while (GameSessionManager.Instance == null)
        {
            yield return null;
        }
        BindSessionManagerEvents();
    }

    private void BindSessionManagerEvents()
    {
        if (_isBound || GameSessionManager.Instance == null) return;
        GameSessionManager session = GameSessionManager.Instance;
        session.OnWaitingStatusChanged += HandleWaitingStatusChanged;
        session.OnGameStarted += HandleGameStarted;
        _isBound = true;

        // 구독 전에 이미 NetworkVariable이 값 변경을 완료했을 수 있으므로 현재 상태를 즉시 반영
        HandleWaitingStatusChanged(session.CurrentJoinedCount, session.ExpectedPlayerCount);
        if (session.IsGameStarted)
        {
            HandleGameStarted();
        }
    }

    private void UnbindSessionManagerEvents()
    {
        if (!_isBound || GameSessionManager.Instance == null)
        {
            _isBound = false;
            return;
        }
        GameSessionManager.Instance.OnWaitingStatusChanged -= HandleWaitingStatusChanged;
        GameSessionManager.Instance.OnGameStarted -= HandleGameStarted;
        _isBound = false;
    }

    private void HandleWaitingStatusChanged(int current, int expected)
    {
        _statusText.text = $"플레이어 합류 대기 중... ({current}/{expected})";
    }

    private void HandleGameStarted()
    {
        _panel.SetActive(false);
    }

    private void ShowWaiting()
    {
        _panel.SetActive(true);
        _statusText.text = "플레이어 합류 대기 중...";
    }
}
