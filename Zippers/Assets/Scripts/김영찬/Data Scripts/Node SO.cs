using UnityEngine;

/// <summary>
/// 노드의 Data를 저장하기 위한 SO<br/>
/// 맵의 프리팹과 연결되어 있다.
/// </summary>
[CreateAssetMenu(fileName = "NodeSO", menuName = "NodeData/NodeSO")]
public class NodeSO : ScriptableObject
{
    /// <summary>
    /// 노드 타입
    /// </summary>
    [field:SerializeField]public NodeType NodeType                  {get; private set;}
    
    /// <summary>
    /// 노드의 현재 진행 상황
    /// </summary>
    [field:SerializeField]public NodeState NodeState                {get; private set;}
    
    /// <summary>
    /// 노드의 식별 번호
    /// </summary>
    [field:SerializeField]public int NodeID                         {get; private set;}
    
    /// <summary>
    /// 이 노드에 해당 되는 Map 프리팹
    /// </summary>
    [field:SerializeField]public GameObject Map                     {get; private set;}
    
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
    /// Special Point에서 시작 될때 플레이어의 스폰 포인트
    /// </summary>
    [field:SerializeField]public Transform[] PlayerSpawnPoint_Sp    {get; private set;}
    
    /// <summary>
    /// 이번 게임 루프 중 노드 트리에서 몇번째에 위치되어있는지 표기<br/>
    /// 임의 조작 금지
    /// </summary>
    public double NodeTreeIndex; // (MVP 후순위지만 일단 변수는 들고 있도록)
    
    /// <summary>
    /// 연결된 다음 노드를 표기<br/>
    /// 임의 조작 금지
    /// </summary>
    public NodeSO[] NextNodes;
    
    /// <summary>
    /// 이번 노드가 시작 시 어느 방향에서 시작할 지 표기<br/>
    /// 임의 조작 금지
    /// </summary>
    public NodeStartDir StartDir;
}
