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
    /// NodeManager에서 사용하는 NodeDataContainer변수
    /// </summary>
    public NodeDataContainer DataContainer  => _dataContainer;

    /// <summary>
    /// NodeManager에서 사용하는 NodePathMaker변수
    /// </summary>
    public NodePathMaker NodePathMaker { get; private set; }
    
    public event Action<NodeDifficulty> OnDifficultyChanged;

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
        DebugTool.Log("ChangeDifficulty: " + difficulty, DebugType.Node, this);
        OnDifficultyChanged?.Invoke(difficulty);
    }
}
