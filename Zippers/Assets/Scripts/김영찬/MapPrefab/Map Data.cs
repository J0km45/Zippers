using System;
using UnityEngine;

public class MapData : MonoBehaviour
{
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
    /// Special Point에서 시작 될때 플레이어의 스폰 포인트
    /// </summary>
    [field:SerializeField]public Transform[] PlayerSpawnPoint_Sp {get; private set;}
    
    
    /// <summary>
    /// Up 방향으로 연결 된 맵
    /// </summary>
    [Header("연결 된 다음 맵")]
    [field:SerializeField]public GameObject NextMap_Up {get; private set;}
    
    /// <summary>
    /// Down 방향으로 연결 된 맵
    /// </summary>
    [field:SerializeField]public GameObject NextMap_Down {get; private set;}
    
    /// <summary>
    /// Left 방향으로 연결 된 맵
    /// </summary>
    [field:SerializeField]public GameObject NextMap_Left {get; private set;}
    
    /// <summary>
    /// Right 방향으로 연결 된 맵
    /// </summary>
    [field:SerializeField]public GameObject NextMap_Right {get; private set;}

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
    /// 이번 게임 루프 중 노드 트리에서 몇번째에 위치되어있는지 표기
    /// </summary>
    [field:SerializeField]public double NodeTreeIndex {get; private set;} // (MVP 후순위지만 일단 변수는 들고 있도록)
    
    /// <summary>
    /// 이번 노드가 시작 시 어느 방향에서 시작할 지 표기<br/>
    /// 0 = 왼쪽, 1 = 위쪽, 2 = 오른쪽, 3 = 아래쪽
    /// </summary>
    [field:SerializeField]public int StartDir {get; private set;}
    
    /// <summary>
    /// NodeSO에서 이 노드의 노드 타입을 불러옴
    /// </summary>
    [field:SerializeField]public NodeType NodeType {get; private set;}

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
    }

    /// <summary>
    /// 현재 맵의 Node Tree Index를 지정<br/>
    /// NodeTreeMaker.cs에서만 사용함<br/>
    /// 임의 변경 금지
    /// </summary>
    /// <param name="nodeTreeIndex">규칙에 따라 지정되는 Index<br/>자세한 내용은 NodeTreeMaker.cs 참조</param>
    public void SetNodeTreeIndex(double nodeTreeIndex)
    {
        NodeTreeIndex = nodeTreeIndex;
    }

    /// <summary>
    /// 현재 맵의 시작 지점을 지정<br/>
    /// NodeTreeMaker.cs에서만 사용함<br/>
    /// 임의 변경 금지
    /// </summary>
    /// <param name="dir">변경할 노드의 시작 지점<br/>
    /// 0 = 왼쪽, 1 = 위쪽, 2 = 오른쪽, 3 = 아래쪽</param>
    public void SetNodeStartDir(int dir)
    {
        StartDir = dir;
    }
    
    /// <summary>
    /// 연결 된 다음 맵을 지정<br/>
    /// NodeTreeMaker.cs에서만 사용함<br/>
    /// NodeStartDir의 반대 방향으로 붙여야 됨으로 오른쪽부터 시게방향으로 지정<br/>
    /// 임의 변경 금지
    /// </summary>
    /// <param name="rightMap">오른쪽으로 향하는 맵<br/>부재 시 null 입력</param>
    /// <param name="lowerMap">아래쪽으로 향하는 맵<br/>부재 시 null 입력</param>
    /// <param name="leftMap">왼쪽으로 향하는 맵<br/>부재 시 null 입력</param>
    /// <param name="upperMap">위쪽으로 향하는 맵<br/>부재 시 null 입력</param>
    public void SetNextMaps(GameObject rightMap, GameObject lowerMap, GameObject leftMap, GameObject upperMap)
    {
        NextMap_Right = rightMap;
        NextMap_Down = lowerMap;
        NextMap_Left = leftMap;
        NextMap_Up = upperMap;
        OnChangeNextMaps?.Invoke();
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
    }

    /// <summary>
    /// 현재 맵의 Node Type를 지정<br/>
    /// Node SO에서만 사용함<br/>
    /// 임의 변경 금지
    /// </summary>
    /// <param name="nodeType">노드 타입 지정</param>
    public void SetNodeType(NodeType nodeType)
    {
        NodeType = nodeType;
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
    }
    
    /// <summary>
    /// 현재 맵에서 살아남은 플레이어 숫자 증가
    /// </summary>
    public void PlusAlivePlayerCount()
    {
        if(AlivePlayerCount >= 4) return;
        AlivePlayerCount++;
        OnChangeAlivePlayerCount?.Invoke(AlivePlayerCount);
    }
    
    /// <summary>
    /// 현재 맵에서 살아남은 플레이어 숫자 감소
    /// </summary>
    public void MinusAlivePlayerCount()
    {
        if(AlivePlayerCount <= 0) return;
        AlivePlayerCount--;
        OnChangeAlivePlayerCount?.Invoke(AlivePlayerCount);
    }

    /// <summary>
    /// 현재 맵에서 살아남은 몬스터 숫자 증가
    /// </summary>
    public void PlusAliveMonsterCount()
    {
        _aliveMonsterCount++;
        OnChangeAliveMonsterCount?.Invoke(_aliveMonsterCount);
    }
    
    /// <summary>
    /// 현재 맵에서 살아남은 몬스터 숫자 감소
    /// </summary>
    public void MinusAliveMonsterCount()
    {
        if(_aliveMonsterCount <= 0) return;
        _aliveMonsterCount--;
        OnChangeAliveMonsterCount?.Invoke(_aliveMonsterCount);
    }

    /// <summary>
    /// 웨이브 1회 클리어 시
    /// </summary>
    public void ClearOneWave()
    {
        if(_remainingWaveCount <= 0) return;
        _remainingWaveCount--;
        OnChangeRemainingWaveCount?.Invoke(_remainingWaveCount);
    }

    #endregion
}
