using UnityEngine;
using Unity.Netcode;
using System;
using Zippers.Network.Contracts;

public class PlayerStamina : NetworkBehaviour
{
    public event Action<float, float> OnStaminaChanged;
    public event Action OnStaminaEmpty;

    private PlayerStats _playerStats;
    private IPlayerStatProvider _statProvider;
    private PlayerMovement _playerMovement;
    private PlayerCombatNetState _combatNetState;

    private float _regenDelayTimer;
    private float _consumePeriodTimer;
    private float _regenPeriodTimer;

    [Header("플레이어 스테미나 정보")]
    [field: SerializeField] public float CurrentStamina { get; private set; }
    [field: SerializeField] public float MaxStamina { get; private set; }

    public bool CanSprint => CurrentStamina > 0f;

    private void Awake()
    {
        _playerStats = GetComponent<PlayerStats>();
        _statProvider = GetComponent<IPlayerStatProvider>();
        _playerMovement = GetComponent<PlayerMovement>();
        _combatNetState = GetComponent<PlayerCombatNetState>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        SubscribeCombatNetState();

        if (IsServer)
        {
            Init();
        }

        SyncFromCombatNetState();
    }

    public override void OnNetworkDespawn()
    {
        UnsubscribeCombatNetState();

        base.OnNetworkDespawn();
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (_playerMovement == null)
        {
            return;
        }

        bool isMoving = _playerMovement.MoveDir.sqrMagnitude > 0.01f;
        bool isUsingStamina = _playerMovement.IsSprinting && isMoving;

        if (isUsingStamina)
        {
            UseStamina();
            return;
        }

        RecoverStamina();
    }

    private void Init()
    {
        if (!TryGetMaxStamina(out float totalStamina))
        {
            DebugTool.Error("[PlayerStamina] 스테미나 스탯 정보를 찾을 수 없습니다.", DebugType.Character, this);
            return;
        }

        if (_combatNetState == null)
        {
            DebugTool.Error("[PlayerStamina] PlayerCombatNetState가 없습니다.", DebugType.CombatNet, this);
            return;
        }

        _combatNetState.ServerInitializeStamina(totalStamina);

        _regenDelayTimer = 0f;
        _consumePeriodTimer = 0f;
        _regenPeriodTimer = 0f;

        SyncFromCombatNetState();

        DebugTool.Log($"[PlayerStamina] 네트워크 스테미나 초기화: {CurrentStamina}/{MaxStamina}", DebugType.CombatNet, this);
    }

    // 업그레이드 UI에서 연결
    public void RefreshMaxStamina()
    {
        if (!TryGetMaxStamina(out float totalStamina))
        {
            DebugTool.Error("[PlayerStamina] 스테미나 스탯 정보를 찾을 수 없습니다.", DebugType.Character, this);
            return;
        }

        if (_combatNetState == null)
        {
            return;
        }

        if (!IsServer)
        {
            DebugTool.Log("[PlayerStamina] 서버가 아니므로 최대 스테미나 갱신을 무시합니다.", DebugType.CombatNet, this);
            return;
        }

        _combatNetState.ServerSetMaxStamina(totalStamina);
    }

    public bool TryStartSprint()
    {
        if (!CanSprint)
        {
            DebugTool.Log("스테미나가 부족해서 달리기 불가", DebugType.Character, this);
            return false;
        }

        return true;
    }

    private void UseStamina()
    {
        if (CurrentStamina <= 0f)
        {
            StopSprintNoStamina();
            return;
        }

        _consumePeriodTimer += Time.deltaTime;

        if (_consumePeriodTimer < GetStaminaPeriod())
        {
            return;
        }

        _consumePeriodTimer = 0f;

        float consumeAmount = GetStaminaConsume();

        if (IsServer)
        {
            _combatNetState.ServerTryUseStamina(consumeAmount);
        }
        else
        {
            RequestUseStaminaServerRpc(consumeAmount);
        }

        ResetRegenDelay();
    }

    private void RecoverStamina()
    {
        _consumePeriodTimer = 0f;

        if (CurrentStamina >= MaxStamina)
        {
            return;
        }

        if (_regenDelayTimer > 0f)
        {
            _regenDelayTimer -= Time.deltaTime;
            return;
        }

        _regenPeriodTimer += Time.deltaTime;

        if (_regenPeriodTimer < GetStaminaPeriod())
        {
            return;
        }

        _regenPeriodTimer = 0f;

        float recoverAmount = MaxStamina * (GetStaminaRegen() / 100f);

        if (IsServer)
        {
            _combatNetState.ServerRecoverStamina(recoverAmount);
        }
        else
        {
            RequestRecoverStaminaServerRpc(recoverAmount);
        }
    }

    private void StopSprintNoStamina()
    {
        if (_playerMovement != null)
        {
            _playerMovement.SetSprint(false);
        }

        OnStaminaEmpty?.Invoke();

        DebugTool.Log("스테미나 소진 - 달리기 중단", DebugType.Character, this);
    }

    private void ResetRegenDelay()
    {
        _regenDelayTimer = GetStaminaDelay();
        _regenPeriodTimer = 0f;
    }

    [ServerRpc]
    private void RequestUseStaminaServerRpc(float amount)
    {
        if (_combatNetState == null)
        {
            return;
        }

        _combatNetState.ServerTryUseStamina(amount);
    }

    [ServerRpc]
    private void RequestRecoverStaminaServerRpc(float amount)
    {
        if (_combatNetState == null)
        {
            return;
        }

        _combatNetState.ServerRecoverStamina(amount);
    }

    private void SubscribeCombatNetState()
    {
        if (_combatNetState == null)
        {
            return;
        }

        _combatNetState.OnStaminaChanged += HandleNetStaminaChanged;
    }

    private void UnsubscribeCombatNetState()
    {
        if (_combatNetState == null)
        {
            return;
        }

        _combatNetState.OnStaminaChanged -= HandleNetStaminaChanged;
    }

    private void HandleNetStaminaChanged(float currentStamina, float maxStamina)
    {
        if (!CanReadCombatStaminaState())
        {
            return;
        }

        CurrentStamina = currentStamina;
        MaxStamina = maxStamina;

        OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);

        if (IsOwner && CurrentStamina <= 0f)
        {
            StopSprintNoStamina();
        }

        DebugTool.Log(
            $"[PlayerStamina] 네트워크 스테미나 반영: {CurrentStamina}/{MaxStamina}",
            DebugType.CombatNet,
            this
        );
    }

    private void SyncFromCombatNetState()
    {
        if (CanReadCombatStaminaState())
        {
            return;
        }

        CurrentStamina = _combatNetState.CurrentStamina;
        MaxStamina = _combatNetState.MaxStamina;

        OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);
    }
    private bool CanReadCombatStaminaState()
    {
        return _combatNetState != null &&
               _combatNetState.IsSpawned &&
               (_combatNetState.IsServer || _combatNetState.IsOwner);
    }

    private bool TryGetMaxStamina(out float totalStamina)
    {
        if (_statProvider != null)
        {
            totalStamina = _statProvider.TotalMaxStamina;
            return true;
        }

        if (_playerStats != null)
        {
            totalStamina = _playerStats.TotalStamina;
            return true;
        }

        totalStamina = 0f;
        return false;
    }

    private float GetStaminaPeriod()
    {
        if (_statProvider != null)
        {
            return _statProvider.StaminaPeriod;
        }

        if (_playerStats != null)
        {
            return _playerStats.StaminaPeriod;
        }

        return 0.1f;
    }

    private float GetStaminaConsume()
    {
        if (_statProvider != null)
        {
            return _statProvider.StaminaConsume;
        }

        if (_playerStats != null)
        {
            return _playerStats.StaminaConsume;
        }

        return 0f;
    }

    private float GetStaminaRegen()
    {
        if (_statProvider != null)
        {
            return _statProvider.TotalStaminaRegen;
        }

        if (_playerStats != null)
        {
            return _playerStats.TotalStaminaRegen;
        }

        return 0f;
    }

    private float GetStaminaDelay()
    {
        if (_statProvider != null)
        {
            return _statProvider.StaminaDelay;
        }

        if (_playerStats != null)
        {
            return _playerStats.StaminaDelay;
        }

        return 0f;
    }
}

/*
Unity 적용 방법
1. 기존 PlayerStamina.cs 전체를 이 코드로 교체한다.
2. 플레이어 프리팹에 PlayerCombatNetState가 붙어 있는지 확인한다.
4. PlayerCombatNetState에 ServerInitializeStamina(), ServerSetMaxStamina(),
   ServerTryUseStamina(), ServerRecoverStamina(), OnStaminaChanged가 있는지 확인한다.
5. 스테미나 UI는 기존 PlayerStamina.OnStaminaChanged를 그대로 구독하면 된다.
6. 스테미나 값은 서버가 PlayerCombatNetState에서 변경하고, Owner 클라이언트 UI에 동기화된다.
*/