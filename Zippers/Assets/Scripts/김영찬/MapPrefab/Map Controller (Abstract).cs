using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 맵이 어떻게 동작하는지 제어하는 역할<br/>
/// 추상 클래스
/// </summary>
public abstract class MapController : MonoBehaviour
{
    /// <summary>
    /// 내 타입이 무엇인지 자식이 무조건 대답해야 함
    /// </summary>
    public abstract NodeType NodeType { get; }
    
    /// <summary>
    /// MapController에서 사용하는 NodeManager 변수
    /// </summary>
    public NodeManager Manager { get; private set; }

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
    /// MapController에서 사용하는 TeleportSupporter 변수
    /// </summary>
    public TeleportSupporter TeleportSupporter { get; private set; }

    private WaitForEndOfFrame _wait;

    protected virtual void Awake()
    {
        Init();
        Register();
        _wait = new WaitForEndOfFrame();
    }
    
    protected virtual void OnEnable()
    {
        StartCoroutine(WaitCoroutine());
    }

    protected virtual void OnDisable()
    {
        EventDisable();
    }

    private void Register()
    {
        Manager.DataContainer.RegisterMap(this);
    }
    
    protected virtual void Start()
    {
        InitController();
        Data.SetNodeState(NodeState.Ready);
        EventController.SetDefaultEvent();
        TeleportSupporter.DisableBeaconAll();
    }

    protected virtual void Update()
    {
        UpdateController();
    }

    private void Init()
    {
        ActionController = new MapActionController(this);
        EventController = new MapEventController(this);
        Manager = FindFirstObjectByType<NodeManager>();
        Data = GetComponent<MapData>();
        TeleportSupporter = GetComponent<TeleportSupporter>();
        DebugTool.Log($"{gameObject.name} Map Controller Ready", DebugType.Node, this);
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
        EventController.Machine.OnEventChangeSendPostEvent += DestroyEventData;
        TeleportSupporter.EnableEvent();
    }

    private void EventDisable()
    {
        Data.OnChangeState -= ActionController.ChangeState;
        EventController.Machine.OnEventChangeSendPostEvent -= DestroyEventData;
    }

    private void DestroyEventData(EventSO postEvent)
    {
        Destroy(postEvent);
    }
    
    private IEnumerator WaitCoroutine()
    {
        while (EventController.Machine == null)
        {
            yield return _wait;
        }
        EventEnable();
    }
}
