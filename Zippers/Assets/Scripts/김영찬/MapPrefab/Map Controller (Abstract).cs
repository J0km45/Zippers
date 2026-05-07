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
        ReadyForUse();
        AutoDeactivate();
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

    private void ReadyForUse()
    {
        Data.SetNodeState(NodeState.Ready);
        EventController.SetDefaultEvent();
        TeleportSupporter.DisableBeaconAll();
    }

    private void AutoDeactivate()
    {
        if(NodeType == NodeType.Start) return;
        DebugTool.Log($"{gameObject.name} Map Auto Deactivate", DebugType.Node, this);
        gameObject.SetActive(false);
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
        TeleportSupporter.OnTeleportStart += TeleportNextMap;
        TeleportSupporter.EnableEvent();
    }

    private void EventDisable()
    {
        Data.OnChangeState -= ActionController.ChangeState;
        EventController.Machine.OnEventChangeSendPostEvent -= DestroyEventData;
        TeleportSupporter.OnTeleportStart -= TeleportNextMap;
    }

    private void DestroyEventData(EventSO postEvent)
    {
        Destroy(postEvent);
    }
    
    private IEnumerator WaitCoroutine()
    {
        while (!Manager.DataContainer.IsDictReady)
        {
            yield return _wait;
        }
        
        while (EventController.Machine == null)
        {
            yield return _wait;
        }
        EventEnable();
    }

    private void TeleportNextMap(NodeStartDir dir)
    {
        Transform[] nextMapStartPos = null;
        MapData nextMapData = null;
        
        switch (dir)
        {
            case NodeStartDir.Up:
                nextMapStartPos = Data.PlayerSpawnPoint_Up;
                nextMapData = Data.NextMap_Up.GetComponent<MapData>();
                break;
            case NodeStartDir.Down:
                nextMapStartPos = Data.PlayerSpawnPoint_Down;
                nextMapData = Data.NextMap_Down.GetComponent<MapData>();
                break;
            case NodeStartDir.Left:
                nextMapStartPos = Data.PlayerSpawnPoint_Left;
                nextMapData = Data.NextMap_Left.GetComponent<MapData>();
                break;
            case NodeStartDir.Right:
                nextMapStartPos = Data.PlayerSpawnPoint_Right;
                nextMapData = Data.NextMap_Right.GetComponent<MapData>();
                break;
        }

        if (nextMapData == null || nextMapStartPos == null)
        {
            DebugTool.Warning($"{gameObject.name} Wrong Position Teleport : {dir}", DebugType.Node, this);
            return;
        }
        
        // ToDo : 플레이어 텔레포트 구현 (NetworkTransform 컴포넌트 삽입되어야 함)
        
        nextMapData.SetNodeStartDir(dir);
        
        DebugTool.Log($"{gameObject.name} Teleport Complete. Next Map : {nextMapData.gameObject.name}", DebugType.Node, nextMapData);
    }
}
