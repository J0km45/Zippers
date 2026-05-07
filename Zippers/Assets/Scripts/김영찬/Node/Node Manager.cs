using System;
using UnityEngine;

/// <summary>
/// 노드 현황을 관리 및 제어
/// </summary>
public class NodeManager : MonoBehaviour
{
    [SerializeField] NodeDifficulty _currentDifficulty;
    
    [SerializeField] private NodeDataContainer _dataContainer;

    /// <summary>
    /// 이번 게임에서 전투 총 회수(Boss, Battle)
    /// </summary>
    public int BattleCount { get; private set; }

    /// <summary>
    /// NodeManager에서 사용하는 NodeDataContainer변수
    /// </summary>
    public NodeDataContainer DataContainer  => _dataContainer;

    /// <summary>
    /// NodeManager에서 사용하는 NodePathMaker변수
    /// </summary>
    public NodePathMaker NodePathMaker { get; private set; }
    
    /// <summary>
    /// 진행 단계가 변경 되었을 때 전파
    /// </summary>
    public event Action<NodeDifficulty> OnDifficultyChanged;
    
    /// <summary>
    /// 전투회수가 변경 될 때 전파
    /// </summary>
    public event Action<int> OnBattleCountChanged; 

    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        EventEnable();
    }

    private void OnDisable()
    {
        EventDisable();
    }

    private void Start()
    {
        // ToDo : 테스트 코드임으로 나중에 GameManager 등에서 다음 코드를 실행 하도록 할 것
        SetDifficulty(NodeDifficulty.Test);
    }

    private void Init()
    {
        ResetBattleCount();
        NodePathMaker = new NodePathMaker(this);
    }
    
    private void EventEnable()
    {
        OnDifficultyChanged += NodePathMaker.MakePath;
    }
    private void EventDisable()
    {
        OnDifficultyChanged -= NodePathMaker.MakePath;
    }

    /// <summary>
    /// 난이도 변경
    /// </summary>
    /// <param name="difficulty">게임 난이도</param>
    public void SetDifficulty(NodeDifficulty difficulty)
    {
        _currentDifficulty = difficulty;
        DebugTool.Log("ChangeDifficulty : " + _currentDifficulty, DebugType.Node, this);
        OnDifficultyChanged?.Invoke(_currentDifficulty);
    }
    
    /// <summary>
    /// Battle Count 증가
    /// </summary>
    public void AddBattleCount()
    {
        BattleCount++;
        DebugTool.Log("AddBattleCount, Current Count : " + BattleCount, DebugType.Node, this);
        OnBattleCountChanged?.Invoke(BattleCount);
    }

    /// <summary>
    /// Battle Count 감소
    /// </summary>
    public void RemoveBattleCount()
    {
        BattleCount--;
        DebugTool.Log("RemoveBattleCount, Current Count : " + BattleCount, DebugType.Node, this);
    }

    /// <summary>
    /// Battle Count 리셋
    /// </summary>
    public void ResetBattleCount()
    {
        BattleCount = 0;
        DebugTool.Log("ResetBattleCount", DebugType.Node, this);
    }
}
