using System;
using UnityEngine;

/// <summary>
/// 노드(맵) 간 이동을 도와주는 서포터<br/>
/// 실제 이동은 Node Manager에서 실행
/// </summary>
public class TeleportSupporter : MonoBehaviour
{
    [SerializeField] private MapController _controller;
    [SerializeField] private TeleportBallotBox _ballotBox_UP;
    [SerializeField] private TeleportBallotBox _ballotBox_Down;
    [SerializeField] private TeleportBallotBox _ballotBox_Left;
    [SerializeField] private TeleportBallotBox _ballotBox_Right;

    public float MinVoteWin { get; private set; }

    bool _nextMapAvailable_UP;
    bool _nextMapAvailable_Down;
    bool _nextMapAvailable_Left;
    bool _nextMapAvailable_Right;
    
    int _voteUp;
    int _voteDown;
    int _voteLeft;
    int _voteRight;
    
    private event Action OnVoteChange;

    /// <summary>
    /// 투표 수가 바뀌면 UI에서 결과를 가져갈 수 있도록 하는 이벤트<br/>
    /// 각각의 int변수는 순서대로 상,하,좌,우 투표수
    /// </summary>
    public event Action<int, int, int, int> OnSendVoteResult;
    
    /// <summary>
    /// 지정된 방향으로 텔레포트를 시작하도록 명령을 보내는 이벤트
    /// </summary>
    public event Action<NodeStartDir> OnTeleportStart; 

    private void Awake()
    {
        Init();
    }

    private void OnDisable()
    {
        DisableEvent();
    }

    private void Init()
    {
        _voteUp = 0;
        _voteDown = 0;
        _voteLeft = 0;
        _voteRight = 0;
    }
    
    /// <summary>
    /// MapController에서 이벤트 일괄 활성화 하기 위함
    /// </summary>
    public void EnableEvent()
    {
        _controller.Data.OnChangeNextMaps += SetBeaconLocation;
        _controller.Data.NetworkMapData.AlivePlayerCount.OnValueChanged += SetMinVoteWin;
        _ballotBox_UP.OnVoteChange += CountVoteUp;
        _ballotBox_Down.OnVoteChange += CountVoteDown;
        _ballotBox_Left.OnVoteChange += CountVoteLeft;
        _ballotBox_Right.OnVoteChange += CountVoteRight;
        OnVoteChange += CulVoteResult;
    }

    private void DisableEvent()
    {
        _controller.Data.OnChangeNextMaps -= SetBeaconLocation;
        _controller.Data.NetworkMapData.AlivePlayerCount.OnValueChanged -= SetMinVoteWin;
        _ballotBox_UP.OnVoteChange -= CountVoteUp;
        _ballotBox_Down.OnVoteChange -= CountVoteDown;
        _ballotBox_Left.OnVoteChange -= CountVoteLeft;
        _ballotBox_Right.OnVoteChange -= CountVoteRight;
        OnVoteChange -= CulVoteResult;
    }
    
    /// <summary>
    /// 연결된 다음 맵의 정보를 받아 비콘 사용 가능 여부 체크
    /// </summary>
    private void SetBeaconLocation()
    {
        _nextMapAvailable_UP = _controller.Data.NextMap_Up != null;
        _nextMapAvailable_Down = _controller.Data.NextMap_Down != null;
        _nextMapAvailable_Left = _controller.Data.NextMap_Left != null;
        _nextMapAvailable_Right = _controller.Data.NextMap_Right != null;
    }
    
    /// <summary>
    /// 사용 가능한 비콘을 활성화
    /// </summary>
    public void EnableBeaconAvailable()
    {
        if(_nextMapAvailable_UP) _controller.Data.SetBeaconEnable(_controller.Data.TeleportBeacon_Up);
        if(_nextMapAvailable_Down) _controller.Data.SetBeaconEnable(_controller.Data.TeleportBeacon_Down);
        if(_nextMapAvailable_Left) _controller.Data.SetBeaconEnable(_controller.Data.TeleportBeacon_Left);
        if(_nextMapAvailable_Right) _controller.Data.SetBeaconEnable(_controller.Data.TeleportBeacon_Right);
        DebugTool.Log($"{_controller.gameObject.name} Enable Beacon", DebugType.Node, this);
    }
    
    /// <summary>
    /// 모든 비콘 비활성화
    /// </summary>
    public void DisableBeaconAll()
    {
        _controller.Data.SetBeaconDisable(_controller.Data.TeleportBeacon_Up);
        _controller.Data.SetBeaconDisable(_controller.Data.TeleportBeacon_Down);
        _controller.Data.SetBeaconDisable(_controller.Data.TeleportBeacon_Left);
        _controller.Data.SetBeaconDisable(_controller.Data.TeleportBeacon_Right);
        DebugTool.Log($"{_controller.gameObject.name} Disable Beacon", DebugType.Node, this);
    }

    private void CountVoteUp(int count)
    {
        _voteUp = count;
        OnVoteChange?.Invoke();
    }

    private void CountVoteDown(int count)
    {
        _voteDown = count;
        OnVoteChange?.Invoke();
    }

    private void CountVoteLeft(int count)
    {
        _voteLeft = count;
        OnVoteChange?.Invoke();
    }

    private void CountVoteRight(int count)
    {
        _voteRight = count;
        OnVoteChange?.Invoke();
    }
    
    private void CulVoteResult()
    {
        DebugTool.Log($"{_controller.gameObject.name} Current Vote Result\n" +
                      $"Up : {_voteUp}, Down : {_voteDown}, Left : {_voteLeft}, Right : {_voteRight}", DebugType.Node, this);
        
        if (_voteUp > MinVoteWin)
        {
            DebugTool.Log($"{_controller.gameObject.name} Teleport To Upper Map", DebugType.Node, this);
            OnTeleportStart?.Invoke(NodeStartDir.Up);
            return;
        }
        
        if (_voteRight > MinVoteWin)
        {
            DebugTool.Log($"{_controller.gameObject.name} Teleport To Right Map", DebugType.Node, this);
            OnTeleportStart?.Invoke(NodeStartDir.Right);
            return;
        }
        
        if (_voteLeft > MinVoteWin)
        {
            DebugTool.Log($"{_controller.gameObject.name} Teleport To Left Map", DebugType.Node, this);
            OnTeleportStart?.Invoke(NodeStartDir.Left);
            return;
        }
        
        if (_voteDown > MinVoteWin)
        {
            DebugTool.Log($"{_controller.gameObject.name} Teleport To Lower Map", DebugType.Node, this);
            OnTeleportStart?.Invoke(NodeStartDir.Down);
            return;
        }

        OnSendVoteResult?.Invoke(_voteUp, _voteDown, _voteLeft, _voteRight);
    }

    public void SetMinVoteWin(int preCount, int curCount)
    {
        MinVoteWin = curCount / 2f;
    }
}
