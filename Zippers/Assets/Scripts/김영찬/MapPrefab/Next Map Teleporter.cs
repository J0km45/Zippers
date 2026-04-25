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
    [SerializeField] private TeleportBallotBox _ballotBox_Sp;
    
    bool _nextMapAvailable_UP;
    bool _nextMapAvailable_Down;
    bool _nextMapAvailable_Left;
    bool _nextMapAvailable_Right;
    bool _nextMapAvailable_Sp;
    
    int _voteUp = 0;
    int _voteDown = 0;
    int _voteLeft = 0;
    int _voteRight = 0;
    int _voteSp = 0;
    
    /// <summary>
    /// 연결된 다음 맵의 정보를 받아 비콘 사용 가능 여부 체크
    /// </summary>
    public void SetBeaconLocation()
    {
        _nextMapAvailable_UP = _controller.Data.NextMap_Up != null;
        _nextMapAvailable_Down = _controller.Data.NextMap_Down != null;
        _nextMapAvailable_Left = _controller.Data.NextMap_Left != null;
        _nextMapAvailable_Right = _controller.Data.NextMap_Right != null;
        _nextMapAvailable_Sp = _controller.Data.NextMap_Sp != null;
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
        if(_nextMapAvailable_Sp) _controller.Data.SetBeaconDisable(_controller.Data.TeleportBeacon_Sp);
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
        _controller.Data.SetBeaconDisable(_controller.Data.TeleportBeacon_Sp);
    }

    
}
