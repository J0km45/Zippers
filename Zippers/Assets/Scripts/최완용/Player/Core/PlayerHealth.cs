using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamagable
{

    [Header("디버그 테스트")]
    [SerializeField] private float _debugDamageAmount = 9999f;

    [ContextMenu("Debug/테스트 데미지 적용")]
    private void DebugApplyDamage()
    {
        DebugTool.Log($"디버그 데미지 적용: {_debugDamageAmount}", DebugType.Character, this);
        TakeDamage(_debugDamageAmount);
    }



    public event Action<float, float> OnHealthChanged;
    public event Action OnDamage;
    public event Action PlayerDied;

    private PlayerStats _playerStats;

    public float CurrentHealth { get; private set; }
    public float MaxHealth { get; private set; }
    public bool IsDead { get; private set; }

    public void Awake()
    {
        _playerStats = GetComponent<PlayerStats>();
    }
    private void Start()
    {
        Init();
    }

    private void Init()
    {
        MaxHealth = _playerStats.MaxHealth;
        CurrentHealth = MaxHealth;
        IsDead = false;
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

        DebugTool.Log($"PlayerHealth 초기화: {CurrentHealth}/{MaxHealth}", DebugType.Character, this);
    }

    //인터페이스 참조
    public void TakeDamage(float damage)
    {
        ApplyDamage(damage);
    }

    private void ApplyDamage(float damage)
    {
        if (IsDead)
        {
            DebugTool.Log("죽음 상태입니다.", DebugType.Character, this);
            return;
        }
        if (damage <= 0f)
        {
            DebugTool.Log("음수 데미지는 적용되지 않습니다.", DebugType.Character, this);
            return;
        }

        CurrentHealth = Mathf.Max(CurrentHealth - damage, 0f);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        DebugTool.Log($"데미지 적용: {damage}, 현재 체력: {CurrentHealth}/{MaxHealth}", DebugType.Character, this);
        if(CurrentHealth <= 0f)
        {
            Die();
        }
        OnDamage?.Invoke();
    }

    private void Die()
    {
        IsDead = true;
        CurrentHealth = 0f;
        PlayerDied?.Invoke();
        DebugTool.Log("플레이어 사망", DebugType.Character, this);
    }
}
