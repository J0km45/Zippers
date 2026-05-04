using UnityEngine;
using System;

public class PlayerStamina : MonoBehaviour
{
    public event Action<float, float> OnStaminaChanged;
    public event Action OnStaminaEmpty;

    [SerializeField] private float _staminaTest;

    private PlayerStats _playerStats;
    private PlayerMovement _playerMovement;

    private float _regenDelayTimer;
    private float _regenPeriodTimer;

    public float CurrentStamina { get; private set; }
    public float MaxStamina { get; private set; }

    public bool CanSprint => CurrentStamina > 0f;

    private void Awake()
    {
        _playerStats = GetComponent<PlayerStats>();
        _playerMovement = GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        bool isMoving = _playerMovement.MoveDir.sqrMagnitude > 0.01f;
        bool isUsingStamina =_playerMovement.IsSprinting && isMoving;

        if (isUsingStamina) 
        {
            UseStamina();
            return;
        }
        RecoverStamina();
    }

    private void Init()
    {
        MaxStamina = _playerStats.Stamina;
        CurrentStamina = MaxStamina;

        _regenDelayTimer = 0f;
        _regenPeriodTimer = 0f;

        OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);

        DebugTool.Log("스테미나 초기화", DebugType.Data, this);
    }

    public bool TryStartSprint()
    {
        if(!CanSprint)
        {
            return false;
        }
        return true;
    }

    private void UseStamina()
    {
        // 왜 두번 초기화 하지?
        if(CurrentStamina <=0f)
        {
            StopSprintNoStamina();
            return;
        }

        CurrentStamina -= _staminaTest * Time.deltaTime;
        CurrentStamina = Mathf.Max(CurrentStamina, 0f);

        ResetRegenDelay();
        OnStaminaChanged?.Invoke(MaxStamina, CurrentStamina);

        if(CurrentStamina < 0f)
        {
            StopSprintNoStamina();
        }
    }

    private void RecoverStamina()
    {
        if(CurrentStamina >= MaxStamina)
        {
            return;
        }
        if(_regenDelayTimer > 0f)
        {
            _regenDelayTimer -= Time.deltaTime;
            return;
        }

        _regenPeriodTimer += Time.deltaTime;

        _regenPeriodTimer = 0f;
        CurrentStamina += _playerStats.StaminaRegen;
        CurrentStamina = Mathf.Min(CurrentStamina,MaxStamina);

        OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);
    }

    private void StopSprintNoStamina()
    {
        CurrentStamina = 0f;

        OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);
        OnStaminaEmpty?.Invoke();
    }

    private void ResetRegenDelay()
    {
        _regenDelayTimer = _playerStats.StaminaDelay;
        _regenPeriodTimer = 0f;
    }
}
