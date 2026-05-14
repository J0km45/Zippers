using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 노드(맵) 간 이동을 도와주는 서포터.
///
/// Phase E 변경:
/// - MonoBehaviour → NetworkBehaviour 로 변환. 같은 GameObject 의 NetworkObject (NetworkMapData 와 공유) 사용.
/// - 4개 vote 카운트가 NetworkVariable&lt;int&gt; 로 동기화 (모든 클라가 동일 값 관찰).
/// - 호스트만 HashSet&lt;ulong&gt; 4개로 어느 플레이어가 어느 박스에 있는지 추적.
/// - 투표함 진입/이탈은 TeleportBallotBox 가 호스트 측에서 OnPlayerEnterBoxServer/ExitBoxServer 호출.
/// - 과반수 판정(CulVoteResult) 은 모든 클라에서 NetworkVariable.OnValueChanged 콜백으로 실행 →
///   동일 NetworkVariable 값 → 동일 결정 → 동일 OnTeleportStart 이벤트 발화 → 모든 클라가 텔레포트.
/// - 실제 NetworkTransform.Teleport 는 MapController.TeleportNextMap 안에서 호스트만 호출.
/// </summary>
public class TeleportSupporter : NetworkBehaviour
{
    [Header("Refs")]
    [SerializeField] private MapController _controller;
    [SerializeField] private TeleportBallotBox _ballotBox_UP;
    [SerializeField] private TeleportBallotBox _ballotBox_Down;
    [SerializeField] private TeleportBallotBox _ballotBox_Left;
    [SerializeField] private TeleportBallotBox _ballotBox_Right;

    // ─────────────────────────────────────────────────────────────────
    // 네트워크 상태 (모두 읽기, 호스트만 쓰기)
    // ─────────────────────────────────────────────────────────────────

    private readonly NetworkVariable<int> _voteUp =
        new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private readonly NetworkVariable<int> _voteDown =
        new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private readonly NetworkVariable<int> _voteLeft =
        new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private readonly NetworkVariable<int> _voteRight =
        new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    /// <summary>UI 가 OnValueChanged 구독해서 표시. 호스트만 갱신, 모두 읽기.</summary>
    public NetworkVariable<int> VoteUp    => _voteUp;
    public NetworkVariable<int> VoteDown  => _voteDown;
    public NetworkVariable<int> VoteLeft  => _voteLeft;
    public NetworkVariable<int> VoteRight => _voteRight;

    /// <summary>과반수 임계값. AlivePlayerCount / 2f. NetworkVariable OnValueChanged 로 동기화.</summary>
    public float MinVoteWin { get; private set; }

    // ─────────────────────────────────────────────────────────────────
    // 호스트 전용 상태 (서버에서만 채워짐)
    // ─────────────────────────────────────────────────────────────────

    private readonly HashSet<ulong> _playersInUp    = new HashSet<ulong>();
    private readonly HashSet<ulong> _playersInDown  = new HashSet<ulong>();
    private readonly HashSet<ulong> _playersInLeft  = new HashSet<ulong>();
    private readonly HashSet<ulong> _playersInRight = new HashSet<ulong>();

    // 비콘 사용 가능 여부 (기존 로직 그대로 — 다음 맵이 연결돼있을 때만 비콘 활성)
    bool _nextMapAvailable_UP;
    bool _nextMapAvailable_Down;
    bool _nextMapAvailable_Left;
    bool _nextMapAvailable_Right;

    // 같은 맵 활성 사이클에서 텔레포트가 1회만 발화하도록 가드.
    // 맵 비활성 → 다음 활성 시점에 리셋 (OnDisable / OnEnable 흐름).
    private bool _teleportTriggered;

    /// <summary>지정된 방향으로 텔레포트를 시작하도록 명령을 보내는 이벤트.</summary>
    public event Action<NodeStartDir> OnTeleportStart;

    // ─────────────────────────────────────────────────────────────────
    // Lifecycle
    // ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        Init();
    }

    public override void OnNetworkSpawn()
    {
        // 모든 클라(호스트 포함) 가 NetworkVariable 변경을 듣고 CulVoteResult 실행 →
        // 동일 값 → 동일 결정 → 동일 OnTeleportStart 발화.
        _voteUp.OnValueChanged    += OnVoteUpChanged;
        _voteDown.OnValueChanged  += OnVoteDownChanged;
        _voteLeft.OnValueChanged  += OnVoteLeftChanged;
        _voteRight.OnValueChanged += OnVoteRightChanged;
    }

    public override void OnNetworkDespawn()
    {
        _voteUp.OnValueChanged    -= OnVoteUpChanged;
        _voteDown.OnValueChanged  -= OnVoteDownChanged;
        _voteLeft.OnValueChanged  -= OnVoteLeftChanged;
        _voteRight.OnValueChanged -= OnVoteRightChanged;
    }

    private void OnDisable()
    {
        DisableEvent();
        // 맵 비활성화 시 호스트 측 상태 리셋. 다음 진입을 위한 준비.
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            _playersInUp.Clear();
            _playersInDown.Clear();
            _playersInLeft.Clear();
            _playersInRight.Clear();
            // NetworkVariable 도 0 으로 리셋. (Spawn 안 됐을 수도 있어 try/catch)
            try
            {
                _voteUp.Value    = 0;
                _voteDown.Value  = 0;
                _voteLeft.Value  = 0;
                _voteRight.Value = 0;
            }
            catch { /* despawn 된 상태면 무시 */ }
        }
        _teleportTriggered = false;
    }

    private void Init()
    {
        // (기존 _voteUp/Down/Left/Right int 4개 0 초기화 자리 — NetworkVariable 은 기본값 0 으로 시작하므로 별도 init 불필요)
    }

    // ─────────────────────────────────────────────────────────────────
    // 이벤트 구독 — MapController.EnableEvent / EventDisable 와 연동
    // ─────────────────────────────────────────────────────────────────

    /// <summary>MapController 가 EnableEvent 시점에 호출.</summary>
    public void EnableEvent()
    {
        _controller.Data.OnChangeNextMaps += SetBeaconLocation;
        _controller.Data.NetworkMapData.AlivePlayerCount.OnValueChanged += SetMinVoteWin;
    }

    private void DisableEvent()
    {
        if (_controller == null) return;
        if (_controller.Data != null)
        {
            _controller.Data.OnChangeNextMaps -= SetBeaconLocation;
            if (_controller.Data.NetworkMapData != null)
            {
                _controller.Data.NetworkMapData.AlivePlayerCount.OnValueChanged -= SetMinVoteWin;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // BallotBox 가 호출하는 진입점 (호스트 전용)
    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// 호스트 측 TeleportBallotBox.OnTriggerEnter 에서 호출. clientId 가 방향 박스에 들어옴.
    /// </summary>
    public void OnPlayerEnterBoxServer(ulong clientId, NodeStartDir dir)
    {
        if (!IsServer) return;
        if (_teleportTriggered) return;   // 이미 텔레포트 시작 — 더 받지 않음

        HashSet<ulong> set = GetSet(dir);
        if (set.Add(clientId))
        {
            ApplyCountToNetVar(dir, set.Count);
        }
    }

    /// <summary>
    /// 호스트 측 TeleportBallotBox.OnTriggerExit 에서 호출. clientId 가 방향 박스에서 나감.
    /// </summary>
    public void OnPlayerExitBoxServer(ulong clientId, NodeStartDir dir)
    {
        if (!IsServer) return;
        if (_teleportTriggered) return;

        HashSet<ulong> set = GetSet(dir);
        if (set.Remove(clientId))
        {
            ApplyCountToNetVar(dir, set.Count);
        }
    }

    private HashSet<ulong> GetSet(NodeStartDir dir)
    {
        switch (dir)
        {
            case NodeStartDir.Up:    return _playersInUp;
            case NodeStartDir.Down:  return _playersInDown;
            case NodeStartDir.Left:  return _playersInLeft;
            case NodeStartDir.Right: return _playersInRight;
            default: return _playersInUp;
        }
    }

    private void ApplyCountToNetVar(NodeStartDir dir, int count)
    {
        switch (dir)
        {
            case NodeStartDir.Up:    _voteUp.Value    = count; break;
            case NodeStartDir.Down:  _voteDown.Value  = count; break;
            case NodeStartDir.Left:  _voteLeft.Value  = count; break;
            case NodeStartDir.Right: _voteRight.Value = count; break;
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // NetworkVariable 변경 콜백 — 모든 클라에서 발화 (host 포함)
    // ─────────────────────────────────────────────────────────────────

    private void OnVoteUpChanged(int prev, int curr)    { CulVoteResult(); }
    private void OnVoteDownChanged(int prev, int curr)  { CulVoteResult(); }
    private void OnVoteLeftChanged(int prev, int curr)  { CulVoteResult(); }
    private void OnVoteRightChanged(int prev, int curr) { CulVoteResult(); }

    /// <summary>
    /// 과반수 판정. 모든 클라에서 동일한 NetworkVariable 값 → 동일 결정.
    /// 동일 OnTeleportStart 발화 → 동일 MapController.TeleportNextMap 실행 →
    /// 호스트만 NetworkTransform.Teleport (NGO 권위 규칙).
    /// </summary>
    private void CulVoteResult()
    {
        if (_teleportTriggered) return;

        DebugTool.Log($"{(_controller != null ? _controller.gameObject.name : "?")} Current Vote Result\n" +
                      $"Up : {_voteUp.Value}, Down : {_voteDown.Value}, Left : {_voteLeft.Value}, Right : {_voteRight.Value}",
                      DebugType.Node, this);

        if (_voteUp.Value > MinVoteWin)
        {
            FireTeleport(NodeStartDir.Up);
            return;
        }
        if (_voteRight.Value > MinVoteWin)
        {
            FireTeleport(NodeStartDir.Right);
            return;
        }
        if (_voteLeft.Value > MinVoteWin)
        {
            FireTeleport(NodeStartDir.Left);
            return;
        }
        if (_voteDown.Value > MinVoteWin)
        {
            FireTeleport(NodeStartDir.Down);
            return;
        }
    }

    private void FireTeleport(NodeStartDir dir)
    {
        _teleportTriggered = true;   // 모든 클라에서 동일 시점에 true 됨 (동일 NetworkVariable 값 기반)
        DebugTool.Log($"{(_controller != null ? _controller.gameObject.name : "?")} Teleport To {dir}",
                      DebugType.Node, this);
        OnTeleportStart?.Invoke(dir);
    }

    // ─────────────────────────────────────────────────────────────────
    // 비콘 (기존 로직 유지)
    // ─────────────────────────────────────────────────────────────────

    private void SetBeaconLocation()
    {
        _nextMapAvailable_UP    = _controller.Data.NextMap_Up    != null;
        _nextMapAvailable_Down  = _controller.Data.NextMap_Down  != null;
        _nextMapAvailable_Left  = _controller.Data.NextMap_Left  != null;
        _nextMapAvailable_Right = _controller.Data.NextMap_Right != null;
    }

    public void EnableBeaconAvailable()
    {
        if (_nextMapAvailable_UP)    _controller.Data.SetBeaconEnable(_controller.Data.TeleportBeacon_Up);
        if (_nextMapAvailable_Down)  _controller.Data.SetBeaconEnable(_controller.Data.TeleportBeacon_Down);
        if (_nextMapAvailable_Left)  _controller.Data.SetBeaconEnable(_controller.Data.TeleportBeacon_Left);
        if (_nextMapAvailable_Right) _controller.Data.SetBeaconEnable(_controller.Data.TeleportBeacon_Right);
        DebugTool.Log($"{_controller.gameObject.name} Enable Beacon", DebugType.Node, this);
    }

    public void DisableBeaconAll()
    {
        _controller.Data.SetBeaconDisable(_controller.Data.TeleportBeacon_Up);
        _controller.Data.SetBeaconDisable(_controller.Data.TeleportBeacon_Down);
        _controller.Data.SetBeaconDisable(_controller.Data.TeleportBeacon_Left);
        _controller.Data.SetBeaconDisable(_controller.Data.TeleportBeacon_Right);
        DebugTool.Log($"{_controller.gameObject.name} Disable Beacon", DebugType.Node, this);
    }

    // ─────────────────────────────────────────────────────────────────
    // 과반수 임계값
    // ─────────────────────────────────────────────────────────────────

    public void SetMinVoteWin(int preCount, int curCount)
    {
        MinVoteWin = curCount / 2f;
    }
}
