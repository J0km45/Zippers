using System;
using UnityEngine;

public class MapData : MonoBehaviour
{
    [SerializeField] MapController _controller;
    
    #region 맵 데이터 변수

    /// <summary>
    /// UP에서 시작 될때 플레이어의 스폰 포인트
    /// </summary>
    [Header("시작 지점")]
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
    [Header("연결 된 다음 맵")]
    [field:SerializeField]public MapController NextMap_Up {get; private set;}
    
    /// <summary>
    /// Down 방향으로 연결 된 맵
    /// </summary>
    [field:SerializeField]public MapController NextMap_Down {get; private set;}
    
    /// <summary>
    /// Left 방향으로 연결 된 맵
    /// </summary>
    [field:SerializeField]public MapController NextMap_Left {get; private set;}
    
    /// <summary>
    /// Right 방향으로 연결 된 맵
    /// </summary>
    [field:SerializeField]public MapController NextMap_Right {get; private set;}

    /// <summary>
    /// Up 방향 노드로 넘어 갈 때 플레이어의 합류 지점
    /// </summary>
    [Header("다음 맵으로 넘어가는 지점")]
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
    [Header("For Debug")]
    [field:SerializeField]public NodeState NodeState {get; private set;}
    
    /// <summary>
    /// 이번 노드가 시작 시 어느 방향에서 시작할 지 표기
    /// </summary>
    [field:SerializeField]public NodeStartDir StartDir {get; private set;}
    
    /// <summary>
    /// 몬스터의 스폰 포인트<br/>
    /// 0번 인덱스 = 플레이어 진입 방향의 왼쪽, 시계 방향으로 구성
    /// </summary>
    public Transform[] MonsterSpawnPoints { get; private set;}

    /// <summary>
    /// 맵에 생존한 플레이어 수
    /// </summary>
    public int AlivePlayerCount { get; private set; }

    /// <summary>
    /// 맵에 생존한 몬스터 수
    /// </summary>
    private int _aliveMonsterCount;

    /// <summary>
    /// 맵에 남은 웨이브 횟수
    /// </summary>
    private int _remainingWaveCount;

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
    
    /// <summary>
    /// 맵에 생존한 몬스터의 수가 변경되면 전파
    /// </summary>
    public event Action<int> OnChangeAliveMonsterCount;
    
    /// <summary>
    /// 맵에 잔여 웨이브가 변경되면 전파
    /// </summary>
    public event Action<int> OnChangeRemainingWaveCount;

    #endregion

    #region 맵 데이터 설정

    private void Awake()
    {
        AlivePlayerCount = 0;
        _aliveMonsterCount = 0;
        _remainingWaveCount = 0;
        DebugTool.Log($"{_controller.gameObject.name} Map Data Awake", DebugType.Node, this);
    }

    /// <summary>
    /// 현재 맵의 시작 지점을 지정<br/>
    /// 임의 변경 금지
    /// </summary>
    /// <param name="dir">변경할 노드의 시작 지점</param>
    public void SetNodeStartDir(NodeStartDir dir)
    {
        StartDir = dir;
        SetSpawnPoint(dir);
        DebugTool.Log($"{_controller.gameObject.name} Map Node Start Dir Set {StartDir}", DebugType.Node, this);
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
        DebugTool.Log($"{_controller.gameObject.name} Map Next Map Set\n dir : {dir}", DebugType.Node, this);
    }
    
    /// <summary>
    /// 현재 맵의 최대 Wave 횟수 지정<br/>
    /// NodeTreeMaker.cs에서만 사용함<br/>
    /// 임의 변경 금지
    /// </summary>
    /// <param name="count">지정할 현재 맵의 최대 Wave</param>
    public void SetRemainingWaveCount(int count)
    {
        _remainingWaveCount = count;
        OnChangeRemainingWaveCount?.Invoke(count);
        DebugTool.Log($"{_controller.gameObject.name} Map Wave Count Set : {count}", DebugType.Node, this);
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
        DebugTool.Log($"{_controller.gameObject.name} Map Node State Change : {NodeState}", DebugType.Node, this);
    }
    
    /// <summary>
    /// 현재 맵에서 살아남은 플레이어 숫자 증가
    /// </summary>
    public void PlusAlivePlayerCount()
    {
        if(AlivePlayerCount >= 4) return;
        AlivePlayerCount++;
        OnChangeAlivePlayerCount?.Invoke(AlivePlayerCount);
        DebugTool.Log($"{_controller.gameObject.name} Player Income, Current Player : {AlivePlayerCount}", DebugType.Node, this);
    }
    
    /// <summary>
    /// 현재 맵에서 살아남은 플레이어 숫자 감소
    /// </summary>
    public void MinusAlivePlayerCount()
    {
        if(AlivePlayerCount <= 0) return;
        AlivePlayerCount--;
        OnChangeAlivePlayerCount?.Invoke(AlivePlayerCount);
        DebugTool.Log($"{_controller.gameObject.name} Player Out, Current Player : {AlivePlayerCount}", DebugType.Node, this);
    }

    /// <summary>
    /// 현재 맵에서 살아남은 몬스터 숫자 증가
    /// </summary>
    public void PlusAliveMonsterCount()
    {
        _aliveMonsterCount++;
        OnChangeAliveMonsterCount?.Invoke(_aliveMonsterCount);
        DebugTool.Log($"{_controller.gameObject.name} Monster Income, Current Monster : {_aliveMonsterCount}", DebugType.Node, this);
    }
    
    /// <summary>
    /// 현재 맵에서 살아남은 몬스터 숫자 감소
    /// </summary>
    public void MinusAliveMonsterCount()
    {
        if(_aliveMonsterCount <= 0) return;
        _aliveMonsterCount--;
        OnChangeAliveMonsterCount?.Invoke(_aliveMonsterCount);
        DebugTool.Log($"{_controller.gameObject.name} Monster Out, Current Monster : {_aliveMonsterCount}", DebugType.Node, this);
    }

    /// <summary>
    /// 웨이브 1회 클리어 시
    /// </summary>
    public void ClearOneWave()
    {
        if(_remainingWaveCount <= 0) return;
        _remainingWaveCount--;
        OnChangeRemainingWaveCount?.Invoke(_remainingWaveCount);
        DebugTool.Log($"{_controller.gameObject.name} Wave Clear, Remain Wave : {_remainingWaveCount}", DebugType.Node, this);
    }
    
    /// <summary>
    /// 시작 위치에 따른 스폰 포인트 지정
    /// </summary>
    /// <param name="dir">플레이어 입장 위치(노드 시작 지점)</param>
    private void SetSpawnPoint(NodeStartDir dir)
    {
        MonsterSpawnPoint tempUp = TeleportBeacon_Up.GetComponent<MonsterSpawnPoint>();
        MonsterSpawnPoint tempDown = TeleportBeacon_Down.GetComponent<MonsterSpawnPoint>();
        MonsterSpawnPoint tempLeft = TeleportBeacon_Left.GetComponent<MonsterSpawnPoint>();
        MonsterSpawnPoint tempRight = TeleportBeacon_Right.GetComponent<MonsterSpawnPoint>();
        
        switch (dir)
        {
            case NodeStartDir.Down:
                MonsterSpawnPoints[0] = tempLeft.SpawnPoint;
                MonsterSpawnPoints[1] = tempUp.SpawnPoint;
                MonsterSpawnPoints[2] = tempRight.SpawnPoint;
                MonsterSpawnPoints[3] = tempDown.SpawnPoint;
                break;
            case NodeStartDir.Left:
                MonsterSpawnPoints[0] = tempUp.SpawnPoint;
                MonsterSpawnPoints[1] = tempRight.SpawnPoint;
                MonsterSpawnPoints[2] = tempDown.SpawnPoint;
                MonsterSpawnPoints[3] = tempLeft.SpawnPoint;
                break;
            case NodeStartDir.Up:
                MonsterSpawnPoints[0] = tempRight.SpawnPoint;
                MonsterSpawnPoints[1] = tempDown.SpawnPoint;
                MonsterSpawnPoints[2] = tempLeft.SpawnPoint;
                MonsterSpawnPoints[3] = tempUp.SpawnPoint;
                break;
            case NodeStartDir.Right:
                MonsterSpawnPoints[0] = tempDown.SpawnPoint;
                MonsterSpawnPoints[1] = tempLeft.SpawnPoint;
                MonsterSpawnPoints[2] = tempUp.SpawnPoint;
                MonsterSpawnPoints[3] = tempRight.SpawnPoint;
                break;
        }
    }
    
    #endregion
}
