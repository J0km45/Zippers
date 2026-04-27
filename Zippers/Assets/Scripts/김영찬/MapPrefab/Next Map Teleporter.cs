using System;
using UnityEngine;

/// <summary>
/// 노드(맵)가 클리어 되었을 때, 다음 노드(맵)으로 이동 시켜주는 역할
/// </summary>
public class NextMapTeleporter : MonoBehaviour
{
    [SerializeField] private MapController _controller;
    [SerializeField] private TeleportBallotBox _ballotBox_UP;
    [SerializeField] private TeleportBallotBox _ballotBox_Down;
    [SerializeField] private TeleportBallotBox _ballotBox_Left;
    [SerializeField] private TeleportBallotBox _ballotBox_Right;
    
    bool _nextMapAvailable_UP;
    bool _nextMapAvailable_Down;
    bool _nextMapAvailable_Left;
    bool _nextMapAvailable_Right;
    
    int _voteUp;
    int _voteDown;
    int _voteLeft;
    int _voteRight;
    
    private event Action OnVoteChange;

    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        EnableVoteEvent();
    }

    private void OnDisable()
    {
        DisableVoteEvent();
    }

    private void Init()
    {
        _voteUp = 0;
        _voteDown = 0;
        _voteLeft = 0;
        _voteRight = 0;
    }
    
    private void EnableVoteEvent()
    {
        _ballotBox_UP.OnVoteChange += CountVoteUp;
        _ballotBox_Down.OnVoteChange += CountVoteDown;
        _ballotBox_Left.OnVoteChange += CountVoteLeft;
        _ballotBox_Right.OnVoteChange += CountVoteRight;
        OnVoteChange += CulVoteResult;
    }

    private void DisableVoteEvent()
    {
        _ballotBox_UP.OnVoteChange -= CountVoteUp;
        _ballotBox_Down.OnVoteChange -= CountVoteDown;
        _ballotBox_Left.OnVoteChange -= CountVoteLeft;
        _ballotBox_Right.OnVoteChange -= CountVoteRight;
        OnVoteChange -= CulVoteResult;
    }
    
    /// <summary>
    /// 연결된 다음 맵의 정보를 받아 비콘 사용 가능 여부 체크
    /// </summary>
    public void SetBeaconLocation()
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
        float minVoteWin = _controller.Data.AlivePlayerCount / 2f;
        
        if (_voteLeft > minVoteWin)
        {
            Teleport();
            return;
        }
        if (_voteUp > minVoteWin)
        {
            Teleport();
            return;
        }
        if (_voteRight > minVoteWin)
        {
            Teleport();
            return;
        }
        if (_voteDown > minVoteWin)
        {
            Teleport();
        }
    }

    private void Teleport()
    {
        // 텔레포트 로직
        // 모든 플레이어 이동
    }
}
