using UnityEngine;
using Unity.Netcode;
using Zippers.Network.Contracts;
using System;

public class PlayerCombatNetState : NetworkBehaviour, IPlayerStatusReader,IPlayerCombatCommands
{
    private const float MinHealth = 0f; // 최소체력 0 아래로 안내려가도록
    private const float DefaultMaxHealth = 100f; // 기본 채력(나중에 IPlayerStatProvider에서 받아올꺼임)


    private readonly NetworkVariable<float> _currentHealth = new NetworkVariable<float>
        (
            DefaultMaxHealth,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
    private readonly NetworkVariable<float> _maxHealth = new NetworkVariable<float>
        (
            DefaultMaxHealth,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
    private readonly NetworkVariable<bool> _isDead = new NetworkVariable<bool>
        (
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
    public float CurrentHealth => throw new NotImplementedException();

    public float MaxHealth => throw new NotImplementedException();

    public bool IsDead => throw new NotImplementedException();

    public float CurrentStamina => throw new NotImplementedException();

    public float CurrentAmmo => throw new NotImplementedException();

    public bool IsReloading => throw new NotImplementedException();

    public event Action<float, float> OnHealthChanged;
    public event Action OnPlayerDied;
    public event Action OnPlayerRevived;

    public override void OnNetworkSpawn()
    {
       base.OnNetworkSpawn();

        DebugTool.Log("Spawn", DebugType.CombatNet, this);
    }

    public override void OnNetworkDespawn()
    {
        
        DebugTool.Log("Despawn", DebugType.CombatNet, this);
        base.OnNetworkDespawn();
    }

    public void ServerApplyHeal(float amount, string source)
    {
        throw new NotImplementedException();
    }

    public void ServerApplyDamage(float damage, ulong attackerClientId, string source)
    {
        throw new NotImplementedException();
    }

    public void ServerKill(string reason)
    {
        throw new NotImplementedException();
    }

    public void ServerRevive(float healthRatio)
    {
        throw new NotImplementedException();
    }
}
