using System;
using UnityEngine;

public class MapData : MonoBehaviour
{
    [SerializeField] MapController _controller;
    
    #region 맵 데이터 변수

    /// <summary>
    /// UP에서 시작 될때 플레이어의 스폰 포인트
    /// </summary>
    [field:SerializeField]public Transform[] PlayerSpawnPoint_Up    {get; private set;}
    
    /// <summary>
    /// Down에서 시작 될때 플레이어의 스폰 포인트
    /// </summary>
    [field:SerializeField]public Transform[] PlayerSpawnPoint_Down  {get; private set;}
    
    /// <summary>
    /// Left에서 시작 될때 플레이어의 스폰 포인트
    /// </summary>
    [field:SerializeField]public Transform[] PlayerSpawnPoint_Left  {get; private set;}
    
    /// <summary>
    /// Right에서 시작 될때 플레이어의 스폰 포인트
    /// </summary>
    [field:SerializeField]public Transform[] PlayerSpawnPoint_Right {get; private set;}
    
    /// <summary>
    /// Up 방향으로 연결 된 맵
    /// </summary>
    public MapController NextMap_Up {get; private set;}
    
    /// <summary>
    /// Down 방향으로 연결 된 맵
    /// </summary>
    public MapController NextMap_Down {get; private set;}
    
    /// <summary>
    /// Left 방향으로 연결 된 맵
    /// </summary>
    public MapController NextMap_Left {get; private set;}
    
    /// <summary>
    /// Right 방향으로 연결 된 맵
    /// </summary>
    public MapController NextMap_Right {get; private set;}

    /// <summary>
    /// Up 방향 노드로 넘어 갈 때 플레이어의 합류 지점
    /// </summary>
    [field:SerializeField]public GameObject TeleportBeacon_Up    {get; private set;}
    
    /// <summary>
    /// Down 방향 노드로 넘어 갈 때 플레이어의 합류 지점
    /// </summary>
    [field:SerializeField]public GameObject TeleportBeacon_Down  {get; private set;}
    
    /// <summary>
    /// Left 방향 노드로 넘어 갈 때 플레이어의 합류 지점
    /// </summary>
    [field:SerializeField]public GameObject TeleportBeacon_Left  {get; private set;}
    
    /// <summary>
    /// Right 방향 노드로 넘어 갈 때 플레이어의 합류 지점
    /// </summary>
    [field:SerializeField]public GameObject TeleportBeacon_Right  {get; private set;}
    
    /// <summary>
    /// 노드의 현재 진행 상황
    /// </summary>
    [field:SerializeField]public NodeState NodeState {get; private set;}
    
    /// <summary>
    /// 이번 노드가 시작 시 어느 방향에서 시작할 지 표기
    /// </summary>
    [field:SerializeField]public NodeStartDir StartDir {get; private set;}
    
    /// <summary>
    /// 몬스터의 스폰 포인트<br/>
    /// 0번 인덱스 = 플레이어 진입 방향의 왼쪽, 시계 방향으로 구성
    /// </summary>
    [field:SerializeField]public Transform[] MonsterSpawnPoints { get; private set;}

    /// <summary>
    /// 맵에 생존한 플레이어 수
    /// </summary>
    public int AlivePlayerCount { get; private set; }

    #endregion
    
    #region 이벤트

    /// <summary>
    /// 연결 된 다음 맵이 변경 되면 전파
    /// </summary>
    public event Action OnChangeNextMaps;
    
    /// <summary>
    /// 현재 노드의 상태가 변경 되면 전파
    /// </summary>
    public event Action<NodeState> OnChangeState;
    
    /// <summary>
    /// 맵에 생존한 플레이어의 수가 변경되면 전파
    /// </summary>
    public event Action<int> OnChangeAlivePlayerCount;

    #endregion

    #region 맵 데이터 설정

    private void Awake()
    {
        AlivePlayerCount = 0;
        DebugTool.Log($"{gameObject.name} Map Data Awake", DebugType.Node, this);
    }

    /// <summary>
    /// 현재 맵의 시작 지점을 지정<br/>
    /// 임의 변경 금지
    /// </summary>
    /// <param name="dir">변경할 노드의 시작 지점</param>
    public void SetNodeStartDir(NodeStartDir dir)
    {
        StartDir = dir;
        DebugTool.Log($"{gameObject.name} Map Node Start Dir Set {StartDir}", DebugType.Node, this);
    }
    
    /// <summary>
    /// 다음 연결 될 맵을 지정
    /// </summary>
    /// <param name="dir">연결 될 방향</param>
    /// <param name="nextMap">다음 맵 프리팹</param>
    public void SetNextMap(NodeStartDir dir, MapController nextMap)
    {
        switch (dir)
        {
            case NodeStartDir.Up:
                NextMap_Up = nextMap;
                break;
            case NodeStartDir.Down:
                NextMap_Down = nextMap;
                break;
            case NodeStartDir.Left:
                NextMap_Left = nextMap;
                break;
            case NodeStartDir.Right:
                NextMap_Right = nextMap;
                break;
        }
        OnChangeNextMaps?.Invoke();
        DebugTool.Log($"{gameObject.name} Map Next Map Set\n dir : {dir}", DebugType.Node, this);
    }
    
    /// <summary>
    /// 텔레포트 비콘 활성화
    /// </summary>
    /// <param name="beacon"></param>
    public void SetBeaconEnable(GameObject beacon)
    {
        beacon.SetActive(true);
    }

    /// <summary>
    /// 텔레포트 비콘 비활성화
    /// </summary>
    /// <param name="beacon"></param>
    public void SetBeaconDisable(GameObject beacon)
    {
        beacon.SetActive(false);
    }

    /// <summary>
    /// 현재 맵의 Node State를 지정
    /// </summary>
    /// <param name="state">지정할 Node State</param>
    public void SetNodeState(NodeState state)
    {
        NodeState = state;
        OnChangeState?.Invoke(state);
        DebugTool.Log($"{gameObject.name} Map Node State Change : {NodeState}", DebugType.Node, this);
    }
    
    /// <summary>
    /// 현재 맵에서 살아남은 플레이어 숫자 증가
    /// </summary>
    public void PlusAlivePlayerCount()
    {
        if(AlivePlayerCount >= 4) return;
        AlivePlayerCount++;
        OnChangeAlivePlayerCount?.Invoke(AlivePlayerCount);
        DebugTool.Log($"{gameObject.name} Player Income, Current Player : {AlivePlayerCount}", DebugType.Node, this);
    }
    
    /// <summary>
    /// 현재 맵에서 살아남은 플레이어 숫자 감소
    /// </summary>
    public void MinusAlivePlayerCount()
    {
        if(AlivePlayerCount <= 0) return;
        AlivePlayerCount--;
        OnChangeAlivePlayerCount?.Invoke(AlivePlayerCount);
        DebugTool.Log($"{gameObject.name} Player Out, Current Player : {AlivePlayerCount}", DebugType.Node, this);
    }
    
    #endregion
}
