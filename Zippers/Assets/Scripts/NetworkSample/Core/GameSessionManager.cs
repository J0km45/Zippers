using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Unity.Netcode;

/// <summary>
/// 게임 씬에서 NGO NetworkSceneManager의 빌트인 이벤트로 합류 판정 및 게임 시작/종료를 처리.
/// 모든 세션 멤버는 이미 Relay+NGO에 연결되어 있는 상태로 게임 씬에 진입함
/// </summary>
public class GameSessionManager : NetworkBehaviour
{
    public static GameSessionManager Instance { get; private set; }

    [SerializeField] private InputActionReference _endGameAction;

    private readonly NetworkVariable<bool> _isGameStartedNet = new NetworkVariable<bool>();
    private readonly NetworkVariable<int> _currentJoinedCountNet = new NetworkVariable<int>();
    private readonly NetworkVariable<int> _expectedPlayerCountNet = new NetworkVariable<int>();

    private bool _gameEnded;

    /// <summary>
    /// 현재 게임 씬 로드까지 완료한 플레이어 수 (호스트 포함)
    /// </summary>
    public int CurrentJoinedCount => _currentJoinedCountNet.Value;

    /// <summary>
    /// 전원 합류 판정의 기준이 되는 예상 인원수
    /// </summary>
    public int ExpectedPlayerCount => _expectedPlayerCountNet.Value;

    /// <summary>
    /// 게임 시작 여부
    /// </summary>
    public bool IsGameStarted => _isGameStartedNet.Value;

    /// <summary>
    /// 합류 상태 변화 알림 (current, expected)
    /// </summary>
    public event Action<int, int> OnWaitingStatusChanged;

    /// <summary>
    /// 전원 합류 후 게임 시작 알림
    /// </summary>
    public event Action OnGameStarted;

    private void Awake()
    {
        SetSingleton();
    }

    public override void OnDestroy()
    {
        if (Instance == this) Instance = null;
        base.OnDestroy();
    }

    public override void OnNetworkSpawn()
    {
        BindNetworkVariableEvents();
        if (IsServer)
        {
            InitServerSide();
            BindSceneManagerEvents();
            _endGameAction.action.Enable();
        }
    }

    public override void OnNetworkDespawn()
    {
        UnbindNetworkVariableEvents();
        if (IsServer)
        {
            UnbindSceneManagerEvents();
            _endGameAction.action.Disable();
        }
    }

    private void BindNetworkVariableEvents()
    {
        _currentJoinedCountNet.OnValueChanged += OnCountChanged;
        _expectedPlayerCountNet.OnValueChanged += OnCountChanged;
        _isGameStartedNet.OnValueChanged += OnGameStartedChanged;
    }

    private void UnbindNetworkVariableEvents()
    {
        _currentJoinedCountNet.OnValueChanged -= OnCountChanged;
        _expectedPlayerCountNet.OnValueChanged -= OnCountChanged;
        _isGameStartedNet.OnValueChanged -= OnGameStartedChanged;
    }

    private void BindSceneManagerEvents()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnClientSceneLoadComplete;
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnAllClientsSceneLoaded;
    }

    private void UnbindSceneManagerEvents()
    {
        // NGO 종료 race로 Singleton/SceneManager가 null일 수 있음
        if (NetworkManager.Singleton == null || NetworkManager.Singleton.SceneManager == null) return;
        NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnClientSceneLoadComplete;
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnAllClientsSceneLoaded;
    }

    // TODO: EndGame 입력 트리거는 임시. 실제 종료 조건(승패/시간 등) 확정 시 교체
    private void Update()
    {
        if (!IsServer || !IsSpawned || !_isGameStartedNet.Value || _gameEnded) return;
        if (_endGameAction.action.WasPressedThisFrame())
        {
            _gameEnded = true;
            EndGameClientRpc();
        }
    }

    private void InitServerSide()
    {
        _expectedPlayerCountNet.Value = LobbyManager.Instance.ExpectedPlayerCount;
        _currentJoinedCountNet.Value = 0;
    }

    private void OnClientSceneLoadComplete(ulong clientId, string sceneName, LoadSceneMode mode)
    {
        if (sceneName != SceneId.Game.GetName()) return;
        _currentJoinedCountNet.Value = _currentJoinedCountNet.Value + 1;
    }

    private void OnAllClientsSceneLoaded(string sceneName, LoadSceneMode mode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (sceneName != SceneId.Game.GetName()) return;
        if (clientsTimedOut != null && clientsTimedOut.Count > 0)
        {
            Debug.LogWarning($"GameSessionManager: {clientsTimedOut.Count}명 씬 로드 timeout - 룸으로 복귀");
            _gameEnded = true;
            EndGameClientRpc();
            return;
        }
        _isGameStartedNet.Value = true;
    }

    // 두 카운트 NetworkVariable 모두 이 핸들러 하나에 구독. 변경된 쪽이 어디든 최신 값을 함께 전파
    private void OnCountChanged(int previous, int current)
    {
        OnWaitingStatusChanged?.Invoke(_currentJoinedCountNet.Value, _expectedPlayerCountNet.Value);
    }

    private void OnGameStartedChanged(bool previous, bool current)
    {
        if (current && !previous)
        {
            OnGameStarted?.Invoke();
        }
    }

    [ClientRpc]
    private void EndGameClientRpc()
    {
        _ = LobbyManager.Instance.ReturnToRoomAsync();
    }

    private void SetSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
}
