using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Difficulty Data SO", menuName = "Node Data/Difficulty Data SO")]
public class DifficultyDataSO : ScriptableObject
{
    [Header("난이도 지정")] 
    [SerializeField] private NodeDifficulty _difficulty;

    [Header("이번 게임에서 몇개의 노드를 지날 지 지정")] 
    [SerializeField] private int _pathNodeCount;

    [Header("방향 별 등장 확률 지정")]
    [SerializeField] [Range(0f, 1f)] private float _upsideChance;
    [SerializeField] [Range(0f, 1f)] private float _leftSideChance;
    [SerializeField] [Range(0f, 1f)] private float _rightSideChance;
    
    [Header("게임에서 중복 등장하는 노드 가중치 지정\n 가중치 데이터가 없을 경우 중복 등장 노드 = Battle Node")] 
    [SerializeField] private NodeTypeWeight[] _weightData;

    /// <summary>
    /// 이번 게임의 난이도
    /// </summary>
    public NodeDifficulty Difficulty => _difficulty;
    
    /// <summary>
    /// 이번 게임에서 몇개의 노드를 지날 지
    /// </summary>
    public int PathNodeCount => _pathNodeCount;
    
    /// <summary>
    /// 위쪽 방향으로 이동 가능 한 노드가 연결 될 확률
    /// </summary>
    public float UpsideChance => _upsideChance;
    
    /// <summary>
    /// 왼쪽 방향으로 이동 가능 한 노드가 연결 될 확률
    /// </summary>
    public float LeftSideChance => _leftSideChance;
    
    /// <summary>
    /// 오른쪽 방향으로 이동 가능 한 노드가 연결 될 확률
    /// </summary>
    public float RightSideChance => _rightSideChance;
    
    /// <summary>
    /// 게임에서 중복 등장하는 노드 가중치<br/>
    /// 가중치 데이터가 없을 경우 Battle 노드 지정
    /// </summary>
    public NodeTypeWeight[] WeightData => _weightData;
}
