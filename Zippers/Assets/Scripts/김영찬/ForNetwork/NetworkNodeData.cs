using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkNodeData : NetworkBehaviour
{
    [SerializeField] NetworkVariable<NodeDifficulty> _difficulty;
    [SerializeField] NetworkVariable<int> _battleCount;
    
    /// <summary>
    /// 이번 게임의 난이도
    /// </summary>
    public NetworkVariable<NodeDifficulty> Difficulty => _difficulty;
    
    /// <summary>
    /// 이번 게임에서 전투 총 회수(Boss, Battle)
    /// </summary>
    public NetworkVariable<int> BattleCount => _battleCount;

    private void Awake()
    {
        DebugTool.Log("Network Node Data Awake", DebugType.Node, this);
    }

    /// <summary>
    /// 난이도 변경
    /// </summary>
    /// <param name="difficulty">게임 난이도</param>
    public void SetDifficulty(NodeDifficulty difficulty)
    {
        if(!IsServer) return;
        _difficulty.Value = difficulty;
        DebugTool.Log("ChangeDifficulty : " + Difficulty, DebugType.Node, this);
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

    
}
