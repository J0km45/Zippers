using System.Collections;
using UnityEngine;

/// <summary>
/// 노드 현황을 관리 하고, Node Tree Maker와 Map Maker의 동작을 제어
/// </summary>
public class NodeManager : MonoBehaviour
{
    /// <summary>
    /// NodeManager에서 NodeGrid를 호출하는 변수
    /// </summary>
    [field:SerializeField] public NodeGrid NodeGrid { get; private set; }
    
    /// <summary>
    /// NodeManager에서 NodeTreeMaker를 호출하는 변수
    /// </summary>
    public NodeTreeMaker TreeMaker { get; private set; }
    
    /// <summary>
    /// NodeManager에서 MapMaker를 호출하는 변수
    /// </summary>
    public MapMaker MapMaker { get; private set; }

    // ToDo : 차후에 난이도 조절 가능하게 되면 그쪽으로 난이도 설정 넘김
    [SerializeField] NodeDifficulty _curDifficulty;
    
    private WaitForEndOfFrame _wait = new();

    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        EnableEvents();
    }

    private void OnDisable()
    {
        DisableEvents();
    }

    private void Start()
    {
        StartCoroutine(WaitDictionary());
    }

    private void Init()
    {
        TreeMaker = new NodeTreeMaker(this);
        MapMaker = new MapMaker(this);
    }

    private void EnableEvents()
    {
        TreeMaker.OnTreeMakingComplete += MapMaker.SetMap;
    }

    private void DisableEvents()
    {
        TreeMaker.OnTreeMakingComplete -= MapMaker.SetMap;
    }

    private IEnumerator WaitDictionary()
    {
        while (NodeDictionary.Instance == null)
        {
            yield return _wait;
        }

        while (TreeDictionary.Instance == null)
        {
            yield return _wait;
        }
        
        while (!NodeDictionary.Instance.IsDictionaryReady)
        {
            yield return _wait;
        }
        
        while (!TreeDictionary.Instance.IsDictionaryReady)
        {
            yield return _wait;
        }
        
        TreeMaker.SetNodeTree(_curDifficulty);
    }
}
