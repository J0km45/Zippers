using System;
using Unity.Netcode;
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
        EnableEvent();
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
    
    private void EnableEvent()
    {
        _controller.Data.OnChangeNextMaps += SetBeaconLocation;
        _ballotBox_UP.OnVoteChange += CountVoteUp;
        _ballotBox_Down.OnVoteChange += CountVoteDown;
        _ballotBox_Left.OnVoteChange += CountVoteLeft;
        _ballotBox_Right.OnVoteChange += CountVoteRight;
        OnVoteChange += CulVoteResult;
    }

    private void DisableEvent()
    {
        _controller.Data.OnChangeNextMaps -= SetBeaconLocation;
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
        DebugTool.Log($"Enable Beacon", DebugType.Node, this);
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
        DebugTool.Log($"Disable Beacon", DebugType.Node, this);
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
        DebugTool.Log($"Current Vote Result\n" +
                      $"Up : {_voteUp}, Down : {_voteDown}, Left : {_voteLeft}, Right : {_voteRight}", DebugType.Node, this);
        
        float minVoteWin = _controller.Data.AlivePlayerCount / 2f;
        
        if (_voteUp > minVoteWin)
        {
            //Teleport(NodeStartDir.Up);
            DebugTool.Log($"Teleport To Upper Map", DebugType.Node, this);
            return;
        }
        
        if (_voteRight > minVoteWin)
        {
            //Teleport(NodeStartDir.Right);
            DebugTool.Log($"Teleport To Right Map", DebugType.Node, this);
            return;
        }
        
        if (_voteLeft > minVoteWin)
        {
            //Teleport(NodeStartDir.Left);
            DebugTool.Log($"Teleport To Left Map", DebugType.Node, this);
            return;
        }
        
        if (_voteDown > minVoteWin)
        {
            //Teleport(NodeStartDir.Down);
            DebugTool.Log($"Teleport To Lower Map", DebugType.Node, this);
            return;
        }
        
        
    }

    private void Teleport(NodeStartDir dir)
    {
        switch (dir)
        {
            case NodeStartDir.Up:
                GameObject nextMap = _controller.Data.NextMap_Up;
                MapData nextMapData = nextMap.GetComponent<MapData>();
                Transform[] nextMapPlayerSpawnPoint = nextMapData.PlayerSpawnPoint_Down;
                int playerIndex = 0;
                foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    Transform sp = nextMapPlayerSpawnPoint[playerIndex % nextMapPlayerSpawnPoint.Length];

                    // ToDo : 네트워크 파트와 협의 후 코드 작성
                    
                    playerIndex++;
                }
                
                break;
        }
    }
}
