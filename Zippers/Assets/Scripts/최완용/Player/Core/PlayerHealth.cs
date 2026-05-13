using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamagable
{
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

    [Header("플레이어 체력 정보")]
    [field:SerializeField] public float CurrentHealth { get; private set; }
    [field:SerializeField] public float MaxHealth { get; private set; }
    [field:SerializeField] public bool IsDead { get; private set; }

    [Space(10)] [Header("디버그 테스트")]
    [SerializeField] private float _debugDamageAmount = 10f;
    
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
        MaxHealth = _playerStats.TotalMaxHealth;
        CurrentHealth = MaxHealth;
        IsDead = false;
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

        DebugTool.Log($"PlayerHealth 초기화: {CurrentHealth}/{MaxHealth}", DebugType.Character, this);
    }
    //업그레이드 UI에서 연결
    public void RefreshHealth()
    {
        float beforeHealth = MaxHealth;
        MaxHealth = _playerStats.TotalMaxHealth;

        float incresaseHealth = MaxHealth - beforeHealth;

        if (incresaseHealth > 0f)
        {
            CurrentHealth += incresaseHealth;
        }
        CurrentHealth = MathF.Min(CurrentHealth, MaxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public void Heal(float amont)
    {
        if(IsDead)
        {
            DebugTool.Log("죽음 상태입니다. 치료할 수 없습니다.", DebugType.Character, this);
            return;
        }
        if (amont <= 0f)
        {
            DebugTool.Log("음수 치료량은 적용되지 않습니다.", DebugType.Character, this);
            return;
        }

        float beforeHealth = CurrentHealth;

        CurrentHealth = Mathf.Min(CurrentHealth + amont, MaxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        DebugTool.Log($"치료 적용: {amont}, 현재 체력: {CurrentHealth}/{MaxHealth}", DebugType.Character, this);
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
        PlayerTransform playerTransform = GetComponent<PlayerTransform>();

        if (playerTransform != null)
        {
            playerTransform.Unregister();
        }

        PlayerDied?.Invoke();
        DebugTool.Log("플레이어 사망", DebugType.Character, this);
    }
}
