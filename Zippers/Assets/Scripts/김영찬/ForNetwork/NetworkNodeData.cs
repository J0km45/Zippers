using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkNodeData : NetworkBehaviour
{
    [SerializeField] NodeManager _manager;
    [SerializeField] NetworkVariable<NodeDifficulty> _difficulty;
    [SerializeField] NetworkVariable<int> _battleCount;
    [SerializeField] NetworkVariable<int> _mapSeed;
    
    /// <summary>
    /// 이번 게임의 난이도
    /// </summary>
    public NetworkVariable<NodeDifficulty> Difficulty => _difficulty;
    
    /// <summary>
    /// 이번 게임에서 전투 총 회수(Boss, Battle)
    /// </summary>
    public NetworkVariable<int> BattleCount => _battleCount;
    
    /// <summary>
    /// 이번 게임의 맵 시드
    /// </summary>
    public NetworkVariable<int> MapSeed => _mapSeed;

    private void Awake()
    {
        DebugTool.Log("Network Node Data Awake", DebugType.Node, this);
    }
    
    /// <summary>
    /// 방장(Server)이 호출하여 난이도와 시드를 결정
    /// </summary>
    public void SetDifficultyAndGenerateMap(NodeDifficulty difficulty)
    {
        if(!IsServer) return;
        
        _difficulty.Value = difficulty;
        _mapSeed.Value = (int)DateTime.Now.Ticks; 
        
        GenerateMapClientRpc(difficulty, _mapSeed.Value);
        
        DebugTool.Log($"[Server] 난이도: {difficulty}, 시드: {_mapSeed.Value} 설정 완료", DebugType.Node, this);
    }
    
    /// <summary>
    /// Battle Count 증가
    /// </summary>
    public void AddBattleCount()
    {
        if(!IsServer) return;
        _battleCount.Value++;
        DebugTool.Log("AddBattleCount, Current Count : " + BattleCount, DebugType.Node, this);
    }

    /// <summary>
    /// Battle Count 감소
    /// </summary>
    public void RemoveBattleCount()
    {
        if(!IsServer) return;
        _battleCount.Value--;
        DebugTool.Log("RemoveBattleCount, Current Count : " + BattleCount, DebugType.Node, this);
    }

    /// <summary>
    /// Battle Count 리셋
    /// </summary>
    public void ResetBattleCount()
    {
        if(!IsServer) return;
        _battleCount.Value = 0;
        DebugTool.Log("ResetBattleCount", DebugType.Node, this);
    }

    /// <summary>
    /// 서버가 명령을 내리면 모든 클라이언트가 동시에 실행하는 맵 생성 로직
    /// </summary>
    [ClientRpc]
    private void GenerateMapClientRpc(NodeDifficulty difficulty, int seed)
    {
        UnityEngine.Random.InitState(seed);
        _manager.NodePathMaker.MakePath(difficulty);
    }
}
