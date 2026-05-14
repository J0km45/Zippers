using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 노드 현황을 관리 및 제어
/// </summary>
public class NodeManager : MonoBehaviour
{
    [SerializeField] private NodeDataContainer _dataContainer;
    [SerializeField] private NetworkNodeData _networkNodeData;

    /// <summary>
    /// NetworkNodeData에서 사용하는 BattleCount를 밖으로 연결
    /// </summary>
    public int BattleCount => NetworkNodeData.BattleCount.Value;
    
    /// <summary>
    /// NodeManager에서 사용하는 NetworkNodeData변수
    /// </summary>
    public NetworkNodeData NetworkNodeData => _networkNodeData;
    
    /// <summary>
    /// NodeManager에서 사용하는 NodeDataContainer변수
    /// </summary>
    public NodeDataContainer DataContainer => _dataContainer;

    /// <summary>
    /// NodeManager에서 사용하는 NodePathMaker변수
    /// </summary>
    public NodePathMaker NodePathMaker { get; private set; }
    
    private void Awake()
    {
        Init();
    }
    
    private void Start()
    {
        // ToDo : 테스트 코드임으로 나중에 GameManager 등에서 다음 코드를 실행 하도록 할 것

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            NetworkNodeData.SetDifficultyAndGenerateMap(NodeDifficulty.Test);
        }
    }

    private void Init()
    {
        NodePathMaker = new NodePathMaker(this);
        DebugTool.Log($"Node Manager Ready", DebugType.Node, this);
    }
    
    public Transform[] GetStartSpawnPoints()
    {
        var startMap = FindFirstObjectByType<StartTypeMapController>();
        if (startMap == null || startMap.Data.PlayerSpawnPoint_Down.Length <= 0)
        {
            DebugTool.Warning("Start Map Not Found", DebugType.Node, this);
            return null;
        }
        
        return startMap.Data.PlayerSpawnPoint_Down;
    }
}
