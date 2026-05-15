using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GoToShopTest : MonoBehaviour
{
    [SerializeField] float _waitTime = 3f;
    private ShopTypeMapController _shop;

    private void Awake()
    {
        _shop = FindFirstObjectByType<ShopTypeMapController>();
    }

    public void OnClickGoToShopButton()
    {
        StartCoroutine(TestTeleportRoutine());
    }
    
    private IEnumerator TestTeleportRoutine()
    {
        _shop.gameObject.SetActive(true);
        
        yield return YieldContainer.Seconds(_waitTime); 
        
        Transform[] nextMapStartPos = _shop.Data.PlayerSpawnPoint_Down;
        TeleportAllPlayersAsHost(nextMapStartPos);
    }
    
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

            PlayerTeleporter teleporter = client.PlayerObject.GetComponent<PlayerTeleporter>();
            if (teleporter == null)
            {
                DebugTool.Error($"clientId={clientId} PlayerObject 에 PlayerTeleporter 컴포넌트 없음 - skip (프리팹 인스펙터 확인 필요)", DebugType.Node, this);
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
                teleporter.TeleportFromServer(spawn.position, spawn.rotation, spawn.localScale);
                successCount++;
            }
            catch (Exception e)
            {
                DebugTool.Error($"clientId={clientId} TeleportFromServer 예외: {e.Message}", DebugType.Node, this);
            }
        }

        DebugTool.Log($"{gameObject.name} TeleportAllPlayers: {successCount}/{clientIds.Count} ClientRpc 전송 완료 (owner 측에서 실제 텔레포트)", DebugType.Node, this);
    }
}
