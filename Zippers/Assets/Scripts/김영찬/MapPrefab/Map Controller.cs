using System.Collections;
using UnityEngine;

/// <summary>
/// 맵이 어떻게 동작하는지 제어하는 역할
/// </summary>
public class MapController : MonoBehaviour
{
    /// <summary>
    /// MapController에서 사용하는 MapData 변수
    /// </summary>
    public MapData Data { get; private set; }
    
    /// <summary>
    /// MapController에서 사용하는 MapActionController 변수
    /// </summary>
    public MapActionController ActionController { get; private set; }
    
    /// <summary>
    /// MapController에서 사용하는 MapEventController 변수
    /// </summary>
    public MapEventController EventController { get; private set; }
    
    /// <summary>
    /// MapController에서 사용하는 NextMapTeleporter 변수
    /// </summary>
    public NextMapTeleporter Teleporter { get; private set; }
    
    /// <summary>
    /// MapController에서 사용하는 NodeManager 변수
    /// </summary>
    public NodeManager Manager { get; private set; }
    
    private WaitForEndOfFrame _wait = new WaitForEndOfFrame();

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
        InitController();
        Data.SetNodeState(NodeState.Ready);
        StartCoroutine(WaitDictionaryReady());
        Teleporter.DisableBeaconAll();
    }

    private void Update()
    {
        UpdateController();
    }

    private void Init()
    {
        Data = GetComponent<MapData>();
        Teleporter = GetComponent<NextMapTeleporter>();
        Manager = FindFirstObjectByType<NodeManager>();
        ActionController = new MapActionController(this);
        EventController = new MapEventController(this);
        DebugTool.Log($"{Data.NodeTreeIndex}Map <color.yellow>Main Controller Ready</color>", DebugType.Node, this);
    }

    private void InitController()
    {
        ActionController.InitActionController();
        EventController.InitEventController();
    }

    private void UpdateController()
    {
        ActionController.Update();
        EventController.Update();
    }
    
    private void EventEnable()
    {
        Data.OnChangeState += ActionController.ChangeState;
    }

    private void EventDisable()
    {
        Data.OnChangeState -= ActionController.ChangeState;
    }

    private IEnumerator WaitDictionaryReady()
    {
        while (EventDictionary.Instance == null)
        {
            yield return _wait;
        }

        while (!EventDictionary.Instance.IsDictionaryReady)
        {
            yield return _wait;
        }
        
        EventController.SetCurrentEvent(NodeEventType.NoEvent, 0);
    }
}
