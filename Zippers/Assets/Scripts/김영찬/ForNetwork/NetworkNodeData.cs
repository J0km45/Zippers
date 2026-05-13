using Unity.Netcode;
using UnityEngine;

public class NetworkNodeData : NetworkBehaviour
{
    [SerializeField] NetworkVariable<NodeDifficulty> _difficulty;
    [SerializeField] NetworkVariable<int> _battleCount;
    
    private NodeDifficulty _preDifficulty;
    private int _preBattleCount;
    
    /// <summary>
    /// 이번 게임의 난이도
    /// </summary>
    public NetworkVariable<NodeDifficulty> Difficulty => _difficulty;
    
    /// <summary>
    /// 이번 게임에서 전투 총 회수(Boss, Battle)
    /// </summary>
    public NetworkVariable<int> BattleCount => _battleCount;
    
    /// <summary>
    /// 난이도 변경
    /// </summary>
    /// <param name="difficulty">게임 난이도</param>
    public void SetDifficulty(NodeDifficulty difficulty)
    {
        _preDifficulty = Difficulty.Value;
        _difficulty.Value = difficulty;
        DebugTool.Log("ChangeDifficulty : " + Difficulty, DebugType.Node, this);
        Difficulty.OnValueChanged?.Invoke(_preDifficulty, Difficulty.Value);
    }
    
    /// <summary>
    /// Battle Count 증가
    /// </summary>
    public void AddBattleCount()
    {
        _preBattleCount = BattleCount.Value;
        _battleCount.Value++;
        DebugTool.Log("AddBattleCount, Current Count : " + BattleCount, DebugType.Node, this);
        BattleCount.OnValueChanged?.Invoke(_preBattleCount, BattleCount.Value);
    }

    /// <summary>
    /// Battle Count 감소
    /// </summary>
    public void RemoveBattleCount()
    {
        _battleCount.Value--;
        DebugTool.Log("RemoveBattleCount, Current Count : " + BattleCount, DebugType.Node, this);
    }

    /// <summary>
    /// Battle Count 리셋
    /// </summary>
    public void ResetBattleCount()
    {
        _battleCount.Value = 0;
        DebugTool.Log("ResetBattleCount", DebugType.Node, this);
    }

    
}
