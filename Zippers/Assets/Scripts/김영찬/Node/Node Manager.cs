using System;
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

    /// <summary>
    /// InitializeAsHost 가 1회 실행되어 시드/Path 가 확정되면 true.
    /// 중복 초기화 방지 + 외부에서 준비 상태 조회용.
    /// </summary>
    public bool IsInitialized { get; private set; }

    /// <summary>
    /// 노드 시스템 전용 격리 RNG. Phase C 에서 도입.
    /// UnityEngine.Random (전역 상태) 대신 사용 — 다른 시스템(이펙트/UI/사운드 등)이
    /// UnityEngine.Random 을 호출해도 노드 시드가 흔들리지 않도록 격리.
    /// 모든 클라가 동일 seed 로 InitRng 호출 → 동일 호출 순서로 사용 → 동일 결과.
    /// </summary>
    public System.Random Rng { get; private set; }

    private void Awake()
    {
        Init();
    }

    /// <summary>
    /// 노드 시스템의 정식 부트스트랩 진입점.
    /// 호스트(GameSpawnController) 가 GameScene 의 OnLoadEventCompleted 시점에 호출.
    /// 난이도 + 시드를 NetworkNodeData 에 기록 → ClientRpc 로 모든 클라가 동일 시드로 Path 생성.
    /// 가드:
    ///   - 호스트가 아니면 무시
    ///   - NetworkNodeData 미할당 / 미스폰 시 에러 + 무시 (호출 시점이 너무 빠른 경우)
    ///   - 정의되지 않은 NodeDifficulty 값 거부
    ///   - 중복 호출 무시 (IsInitialized 플래그)
    /// 이전엔 Start() 안에서 NodeDifficulty.Test 로 자동 호출됐으나, Phase B 에서 외부 트리거로 분리.
    /// </summary>
    public void InitializeAsHost(NodeDifficulty difficulty)
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
        {
            DebugTool.Warning("InitializeAsHost 거부: 호스트가 아님", DebugType.Node, this);
            return;
        }
        if (_networkNodeData == null)
        {
            DebugTool.Error("InitializeAsHost 거부: NetworkNodeData 미할당 (인스펙터 확인)", DebugType.Node, this);
            return;
        }
        if (!_networkNodeData.IsSpawned)
        {
            DebugTool.Error("InitializeAsHost 거부: NetworkNodeData 가 spawn 되지 않음 (호출 시점이 너무 빠름)", DebugType.Node, this);
            return;
        }
        if (!Enum.IsDefined(typeof(NodeDifficulty), difficulty))
        {
            DebugTool.Error($"InitializeAsHost 거부: 정의되지 않은 NodeDifficulty 값 {difficulty}", DebugType.Node, this);
            return;
        }
        if (IsInitialized)
        {
            DebugTool.Warning($"InitializeAsHost 중복 호출 무시 (이미 초기화됨, current={_networkNodeData.Difficulty.Value})", DebugType.Node, this);
            return;
        }

        _networkNodeData.SetDifficultyAndGenerateMap(difficulty);
        IsInitialized = true;
        DebugTool.Log($"노드 시스템 초기화 완료: difficulty={difficulty}", DebugType.Node, this);
    }

    /// <summary>
    /// 노드 전용 격리 RNG 를 시드로 초기화.
    /// NetworkNodeData.GenerateMapClientRpc 가 모든 클라(호스트 포함)에서 호출.
    /// 동일 seed → 동일 RNG 시퀀스 → 모든 클라가 동일 Path / 동일 GetRandomMap 결과.
    /// 이전(Phase B 이전) 의 UnityEngine.Random.InitState 를 대체.
    /// </summary>
    public void InitRng(int seed)
    {
        if (Rng != null)
        {
            DebugTool.Warning($"InitRng 재호출 — 기존 RNG 교체 (new seed={seed}). 일반적으론 새 게임 진입 시에만 발생", DebugType.Node, this);
        }
        Rng = new System.Random(seed);
        DebugTool.Log($"Node RNG 초기화 완료: seed={seed}", DebugType.Node, this);
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
