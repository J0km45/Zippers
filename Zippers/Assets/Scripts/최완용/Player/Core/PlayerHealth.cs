using System;
using UnityEngine;
using Zippers.Network.Contracts;

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
    private IPlayerStatProvider _statProvider;
    private PlayerCombatNetState _combatNetState;

    [Header("플레이어 체력 정보")]
    [field: SerializeField] public float CurrentHealth { get; private set; }
    [field: SerializeField] public float MaxHealth { get; private set; }
    [field: SerializeField] public bool IsDead { get; private set; }

    [Space(10)]
    [Header("디버그 테스트")]
    [SerializeField] private float _debugDamageAmount = 10f;

    private void Awake()
    {
        _playerStats = GetComponent<PlayerStats>();
        _statProvider = GetComponent<IPlayerStatProvider>();
        _combatNetState = GetComponent<PlayerCombatNetState>();
    }

    private void OnEnable()
    {
        SubscribeCombatNetState();
    }

    private void OnDisable()
    {
        UnsubscribeCombatNetState();
    }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        if (!TryGetMaxHealth(out float totalHealth))
        {
            DebugTool.Error("[PlayerHealth] 체력 스탯 정보를 찾을 수 없습니다.", DebugType.Character, this);
            return;
        }

        if (_combatNetState != null)
        {
            if (_combatNetState.IsServer)
            {
                _combatNetState.ServerInitializeHealth(totalHealth);
            }

            SyncFromCombatNetState();

            DebugTool.Log(
                $"[PlayerHealth] 네트워크 체력 초기화 연결: {CurrentHealth}/{MaxHealth}",
                DebugType.CombatNet,
                this
            );

            return;
        }

        MaxHealth = totalHealth;
        CurrentHealth = MaxHealth;
        IsDead = false;

        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

        DebugTool.Log($"PlayerHealth 초기화: {CurrentHealth}/{MaxHealth}", DebugType.Character, this);
    }

    // 업그레이드 UI에서 연결
    public void RefreshHealth()
    {
        if (!TryGetMaxHealth(out float totalHealth))
        {
            DebugTool.Error("[PlayerHealth] 체력 스탯 정보를 찾을 수 없습니다.", DebugType.Character, this);
            return;
        }

        if (_combatNetState != null)
        {
            if (!_combatNetState.IsServer)
            {
                DebugTool.Log("[PlayerHealth] 서버가 아니므로 최대 체력 갱신을 무시합니다.", DebugType.CombatNet, this);
                return;
            }

            // 기존 기획 유지:
            // MaxHealth 증가 시 증가분만큼 CurrentHealth도 증가.
            _combatNetState.ServerSetMaxHealth(totalHealth);
            return;
        }

        // PlayerCombatNetState가 없는 싱글/테스트 환경에서는 기존 방식 유지
        float beforeHealth = MaxHealth;
        MaxHealth = totalHealth;

        float increaseHealth = MaxHealth - beforeHealth;

        if (increaseHealth > 0f)
        {
            CurrentHealth += increaseHealth;
        }

        CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public void Heal(float amount)
    {
        if (_combatNetState != null)
        {
            if (!_combatNetState.IsServer)
            {
                DebugTool.Log("[PlayerHealth] 서버가 아니므로 회복 요청을 무시합니다.", DebugType.CombatNet, this);
                return;
            }

            _combatNetState.ServerApplyHeal(amount, "PlayerHealth.Heal");
            return;
        }

        if (IsDead)
        {
            DebugTool.Log("죽음 상태입니다. 치료할 수 없습니다.", DebugType.Character, this);
            return;
        }

        if (amount <= 0f)
        {
            DebugTool.Log("음수 치료량은 적용되지 않습니다.", DebugType.Character, this);
            return;
        }

        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

        DebugTool.Log($"치료 적용: {amount}, 현재 체력: {CurrentHealth}/{MaxHealth}", DebugType.Character, this);
    }

    // IDamagable 인터페이스에서 호출
    public void TakeDamage(float damage)
    {
        if (_combatNetState != null)
        {
            if (!_combatNetState.IsServer)
            {
                DebugTool.Log("[PlayerHealth] 서버가 아니므로 데미지 요청을 무시합니다.", DebugType.CombatNet, this);
                return;
            }

            _combatNetState.ServerApplyDamage(damage, 0, "PlayerHealth.TakeDamage");
            OnDamage?.Invoke();
            return;
        }

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

        if (CurrentHealth <= 0f)
        {
            Die();
        }

        OnDamage?.Invoke();
    }

    private void Die()
    {
        if (IsDead)
        {
            return;
        }

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

    private void SubscribeCombatNetState()
    {
        if (_combatNetState == null)
        {
            return;
        }

        _combatNetState.OnHealthChanged += HandleNetHealthChanged;
        _combatNetState.OnPlayerDied += HandleNetPlayerDied;
        _combatNetState.OnPlayerRevived += HandleNetPlayerRevived;
    }

    private void UnsubscribeCombatNetState()
    {
        if (_combatNetState == null)
        {
            return;
        }

        _combatNetState.OnHealthChanged -= HandleNetHealthChanged;
        _combatNetState.OnPlayerDied -= HandleNetPlayerDied;
        _combatNetState.OnPlayerRevived -= HandleNetPlayerRevived;
    }

    private void HandleNetHealthChanged(float currentHealth, float maxHealth)
    {
        CurrentHealth = currentHealth;
        MaxHealth = maxHealth;

        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

        DebugTool.Log(
            $"[PlayerHealth] 네트워크 체력 반영: {CurrentHealth}/{MaxHealth}",
            DebugType.CombatNet,
            this
        );
    }

    private void HandleNetPlayerDied()
    {
        Die();
    }

    private void HandleNetPlayerRevived()
    {
        IsDead = false;
        SyncFromCombatNetState();

        DebugTool.Log("[PlayerHealth] 네트워크 부활 상태 반영", DebugType.CombatNet, this);
    }

    private void SyncFromCombatNetState()
    {
        if (_combatNetState == null)
        {
            return;
        }

        CurrentHealth = _combatNetState.CurrentHealth;
        MaxHealth = _combatNetState.MaxHealth;
        IsDead = _combatNetState.IsDead;

        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    private bool TryGetMaxHealth(out float totalHealth)
    {
        if (_statProvider != null)
        {
            totalHealth = _statProvider.TotalMaxHealth;
            return true;
        }

        if (_playerStats != null)
        {
            totalHealth = _playerStats.TotalMaxHealth;
            return true;
        }

        totalHealth = 0f;
        return false;
    }
}

/*
Unity 적용 방법
1. 기존 PlayerHealth.cs 전체를 이 코드로 교체한다.
2. 플레이어 프리팹에 PlayerCombatNetState가 붙어 있는지 확인한다.
3. PlayerCombatNetState에 ServerInitializeHealth(), ServerSetMaxHealth(),
   ServerApplyHeal(), ServerApplyDamage(), ServerRevive()가 있는지 확인한다.
4. Contracts 폴더 안에 PlayerCombatNetState.cs가 있다면 삭제한다.
5. 호스트에서 Debug/테스트 데미지 적용을 눌러 체력 감소 로그가 찍히는지 확인한다.
6. 클라이언트에서 직접 TakeDamage가 호출되면 서버가 아니므로 무시되는 것이 정상이다.
*/