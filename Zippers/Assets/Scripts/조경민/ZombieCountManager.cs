using System;
using Unity.Netcode;
using UnityEngine;

public class ZombieCountManager : NetworkBehaviour
{
    // 현재 웨이브
    public NetworkVariable<int> AliveCount = new(); // 현재 살아있는 좀비 수
    public NetworkVariable<int> SpawnedCount = new(); // 현재 웨이브에서 스폰된 좀비 수
    public bool IsWaveCleared => SpawnedCount.Value > 0 && AliveCount.Value <= 0; // 웨이브 클리어 여부

    // 전체
    public NetworkVariable<int> TotalAliveCount = new(); // 전체 살아있는 좀비 수
    public NetworkVariable<int> TotalSpawnCount = new(); // 전체 웨이브에서 스폰된 좀비 수

    public bool IsNodeCleared => TotalSpawnCount.Value > 0 && TotalAliveCount.Value <= 0; // 노드 클리어 여부

    public event Action<int> OnZombieCountChanged;

    public override void OnNetworkSpawn()
    {
        TotalAliveCount.OnValueChanged += OnTotalAliveCountChanged;

        OnTotalAliveCountChanged(0, TotalAliveCount.Value);
    }

    public override void OnNetworkDespawn()
    {
        TotalAliveCount.OnValueChanged -= OnTotalAliveCountChanged;
    }

    private void OnTotalAliveCountChanged(int previous, int current)
    {
        OnZombieCountChanged?.Invoke(current);
    }

    public void ResetCount()
    {
        if (!IsServer) return;

        AliveCount.Value = 0;
        SpawnedCount.Value = 0;
    }

    public void ResetTotalCount()
    {
        if (!IsServer) return;

        TotalAliveCount.Value = 0;
        TotalSpawnCount.Value = 0;
    }

    public void AddCount()
    {
        if (!IsServer) return;

        SpawnedCount.Value++;
        AliveCount.Value++;

        TotalSpawnCount.Value++;
        TotalAliveCount.Value++;
    }

    public void RemoveCount()
    {
        if (!IsServer) return;

        AliveCount.Value = Mathf.Max(0, AliveCount.Value - 1);
        TotalAliveCount.Value = Mathf.Max(0, TotalAliveCount.Value - 1);
    }
}
