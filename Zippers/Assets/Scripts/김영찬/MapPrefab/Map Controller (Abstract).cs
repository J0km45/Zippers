using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
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

    protected virtual void Awake()
    {
        Init();
        Register();
    }
    
    protected virtual void OnEnable()
    {
        StartCoroutine(WaitCoroutine());
    }

    protected virtual void OnDisable()
    {
        EventDisable();
        ReadyForUse();
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
        Data.NetworkMapData.SetNodeState(NodeState.Ready);
        Data.NetworkMapData.ResetAlivePlayerCount();
        Data.ResetNextMaps();
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
        Data.NetworkMapData.NodeState.OnValueChanged += ActionController.ChangeState;
        EventController.Machine.OnEventChangeSendPostEvent += DestroyEventData;
        TeleportSupporter.OnTeleportStart += TeleportNextMap;
        TeleportSupporter.EnableEvent();
    }

    private void EventDisable()
    {
        Data.NetworkMapData.NodeState.OnValueChanged -= ActionController.ChangeState;
        EventController.Machine.OnEventChangeSendPostEvent -= DestroyEventData;
        TeleportSupporter.OnTeleportStart -= TeleportNextMap;
    }

    private void DestroyEventData(EventSO postEvent)
    {
        Destroy(postEvent);
    }

    private void TeleportNextMap(NodeStartDir dir)
    {
        Transform[] nextMapStartPos = null;
        MapData nextMapData = null;

        // Phase E 수정:
        //   기존 코드는 Data.PlayerSpawnPoint_X (현재 맵의 spawn point) 를 사용했음 — 변수명상
        //   nextMapStartPos 인데 의미가 어긋났음 (텔레포트 대상은 다음 맵이어야 함).
        //   다음 맵의 PlayerSpawnPoint_X 로 교정. nextMapData.SetNodeStartDir(dir) 와 일관성.
        switch (dir)
        {
            case NodeStartDir.Up:
                nextMapData = Data.NextMap_Up.GetComponent<MapData>();
                if (nextMapData != null) nextMapStartPos = nextMapData.PlayerSpawnPoint_Up;
                if(Data.NextMap_Down != null) Data.NextMap_Down.gameObject.SetActive(false);
                if(Data.NextMap_Left != null) Data.NextMap_Left.gameObject.SetActive(false);
                if(Data.NextMap_Right != null) Data.NextMap_Right.gameObject.SetActive(false);
                break;
            case NodeStartDir.Down:
                nextMapData = Data.NextMap_Down.GetComponent<MapData>();
                if (nextMapData != null) nextMapStartPos = nextMapData.PlayerSpawnPoint_Down;
                if(Data.NextMap_Up != null) Data.NextMap_Up.gameObject.SetActive(false);
                if(Data.NextMap_Left != null) Data.NextMap_Left.gameObject.SetActive(false);
                if(Data.NextMap_Right != null) Data.NextMap_Right.gameObject.SetActive(false);
                break;
            case NodeStartDir.Left:
                nextMapData = Data.NextMap_Left.GetComponent<MapData>();
                if (nextMapData != null) nextMapStartPos = nextMapData.PlayerSpawnPoint_Left;
                if(Data.NextMap_Down != null) Data.NextMap_Down.gameObject.SetActive(false);
                if(Data.NextMap_Up != null) Data.NextMap_Up.gameObject.SetActive(false);
                if(Data.NextMap_Right != null) Data.NextMap_Right.gameObject.SetActive(false);
                break;
            case NodeStartDir.Right:
                nextMapData = Data.NextMap_Right.GetComponent<MapData>();
                if (nextMapData != null) nextMapStartPos = nextMapData.PlayerSpawnPoint_Right;
                if(Data.NextMap_Down != null) Data.NextMap_Down.gameObject.SetActive(false);
                if(Data.NextMap_Left != null) Data.NextMap_Left.gameObject.SetActive(false);
                if(Data.NextMap_Up != null) Data.NextMap_Up.gameObject.SetActive(false);
                break;
        }

        if (nextMapData == null || nextMapStartPos == null)
        {
            DebugTool.Warning($"{gameObject.name} Wrong Position Teleport : {dir}", DebugType.Node, this);
            return;
        }

        // Phase E: NetworkTransform.Teleport 로 호스트가 모든 PlayerObject 위치 동기화.
        // 모든 클라가 이 메서드를 실행하지만, IsServer 인 호스트만 실제 텔레포트 호출.
        TeleportAllPlayersAsHost(nextMapStartPos);

        nextMapData.SetNodeStartDir(dir);

        DebugTool.Log($"{gameObject.name} Teleport Complete. Next Map : {nextMapData.gameObject.name}", DebugType.Node, nextMapData);
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Phase E: 호스트가 모든 PlayerObject 의 NetworkTransform 을 다음 맵의 spawn point 로 이동.
    /// 각 플레이어의 SlotIndex (PlayerSessionBridge → PlayerInfo) 에 따라 좌표 배정.
    /// NetworkTransform.Teleport 는 호스트만 호출 가능하며, 모든 클라에 즉시 동기화됨.
    /// 비호스트는 이 메서드 안에서 즉시 return → 안전.
    /// </summary>
    private void TeleportAllPlayersAsHost(Transform[] spawnPoints)
    {
        NetworkManager nm = NetworkManager.Singleton;
        if (nm == null || !nm.IsServer) return;

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            DebugTool.Error($"{gameObject.name} TeleportAllPlayers: spawn point 배열 없음", DebugType.Node, this);
            return;
        }

        if (PlayerSessionBridge.Instance == null)
        {
            DebugTool.Error("PlayerSessionBridge.Instance 가 null - SlotIndex 조회 불가, 텔레포트 중단", DebugType.Node, this);
            return;
        }

        // ConnectedClientsIds 순회 중 컬렉션 변경 방지를 위해 복사.
        List<ulong> clientIds = new List<ulong>(nm.ConnectedClientsIds);

        int successCount = 0;
        for (int i = 0; i < clientIds.Count; i++)
        {
            ulong clientId = clientIds[i];

            if (!PlayerSessionBridge.Instance.TryGetPlayerInfo(clientId, out PlayerInfo info))
            {
                DebugTool.Warning($"clientId={clientId} PlayerInfo 조회 실패 - skip", DebugType.Node, this);
                continue;
            }
            if (info.SlotIndex < 0 || info.SlotIndex >= spawnPoints.Length)
            {
                DebugTool.Warning($"clientId={clientId} SlotIndex={info.SlotIndex} 범위 외 (spawnPoints 길이 {spawnPoints.Length}) - skip", DebugType.Node, this);
                continue;
            }

            if (!nm.ConnectedClients.TryGetValue(clientId, out NetworkClient client) || client.PlayerObject == null)
            {
                DebugTool.Warning($"clientId={clientId} PlayerObject 없음 - skip", DebugType.Node, this);
                continue;
            }

            NetworkTransform nt = client.PlayerObject.GetComponent<NetworkTransform>();
            if (nt == null)
            {
                DebugTool.Error($"clientId={clientId} PlayerObject 에 NetworkTransform 없음 - skip", DebugType.Node, this);
                continue;
            }

            Transform spawn = spawnPoints[info.SlotIndex];
            if (spawn == null)
            {
                DebugTool.Warning($"clientId={clientId} SlotIndex={info.SlotIndex} 의 spawn Transform 이 null - skip", DebugType.Node, this);
                continue;
            }

            try
            {
                nt.Teleport(spawn.position, spawn.rotation, spawn.localScale);
                successCount++;
            }
            catch (Exception e)
            {
                DebugTool.Error($"clientId={clientId} NetworkTransform.Teleport 예외: {e.Message}", DebugType.Node, this);
            }
        }

        DebugTool.Log($"{gameObject.name} TeleportAllPlayers: {successCount}/{clientIds.Count} 텔레포트 완료", DebugType.Node, this);
    }
    
    private IEnumerator WaitCoroutine()
    {
        while (!Manager.DataContainer.IsDictReady)
        {
            yield return YieldContainer.EndOfFrame();
        }
        
        while (EventController.Machine == null)
        {
            yield return YieldContainer.EndOfFrame();
        }
        EventEnable();
    }
}
