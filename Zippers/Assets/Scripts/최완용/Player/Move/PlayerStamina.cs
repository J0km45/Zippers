using UnityEngine;
using System;

public class PlayerStamina : MonoBehaviour
{
    public event Action<float, float> OnStaminaChanged;
    public event Action OnStaminaEmpty;

    private PlayerStats _playerStats;
    private PlayerMovement _playerMovement;

    private float _regenDelayTimer;
    private float _consumePeriodTimer;
    private float _regenPeriodTimer;

    [Header("플레이어 스테미나 정보")]
    [field:SerializeField] public float CurrentStamina { get; private set; }
    [field:SerializeField] public float MaxStamina { get; private set; }

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
        if (_playerStats == null || _playerMovement == null)
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
        if (_playerStats == null)
        {
            DebugTool.Error("PlayerStats가 없습니다.", DebugType.Character, this);
            return;
        }

        MaxStamina = _playerStats.TotalStamina;
        CurrentStamina = MaxStamina;

        _regenDelayTimer = 0f;
        _consumePeriodTimer = 0f;
        _regenPeriodTimer = 0f;

        OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);

        DebugTool.Log($"스테미나 초기화: {CurrentStamina}/{MaxStamina}", DebugType.Character, this);
    }

    //업그레이드 UI에서 연결
    public void RefreshMaxStamina()
    {
        float beforeMaxStamina = MaxStamina;
        MaxStamina = _playerStats.TotalStamina;

        float increaseStamina = MaxStamina - beforeMaxStamina;

        if (increaseStamina > 0f)
        {
            CurrentStamina += increaseStamina;
        }
        CurrentStamina = MathF.Min(CurrentStamina, MaxStamina);
        OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);
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
        //TODO : 스테미나 소모/ 회복은 서버 검증후 동기화 필요
        if (CurrentStamina <= 0f)
        {
            StopSprintNoStamina();
            return;
        }

        _consumePeriodTimer += Time.deltaTime;

        if (_consumePeriodTimer < _playerStats.StaminaPeriod)
        {
            return;
        }

        _consumePeriodTimer = 0f;

        CurrentStamina -= _playerStats.StaminaConsume;
        CurrentStamina = Mathf.Max(CurrentStamina, 0f);

        ResetRegenDelay();

        OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);

        DebugTool.Log($"스테미나 소모: {CurrentStamina}/{MaxStamina}", DebugType.Character, this);

        if (CurrentStamina <= 0f)
        {
            StopSprintNoStamina();
        }
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

        if (_regenPeriodTimer < _playerStats.StaminaPeriod)
        {
            return;
        }

        _regenPeriodTimer = 0f;

        CurrentStamina += _playerStats.TotalStamina * (_playerStats.TotalStaminaRegen / 100f);
        CurrentStamina = Mathf.Min(CurrentStamina, MaxStamina);

        OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);

        DebugTool.Log($"스테미나 회복: {CurrentStamina}/{MaxStamina}", DebugType.Character, this);
    }

    private void StopSprintNoStamina()
    {
        CurrentStamina = 0f;

        if (_playerMovement != null)
        {
            _playerMovement.SetSprint(false);
        }

        OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);
        OnStaminaEmpty?.Invoke();

        DebugTool.Log("스테미나 소진 - 달리기 중단", DebugType.Character, this);
    }

    private void ResetRegenDelay()
    {
        _regenDelayTimer = _playerStats.StaminaDelay;
        _regenPeriodTimer = 0f;
    }
}