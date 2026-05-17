using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using Zippers.Network.Contracts;
using Audio;

/// <summary>
/// 플레이어 전투 런타임 상태를 네트워크로 관리하는 클래스.
/// 체력, 사망, 스테미나, 탄약, 재장전 상태를 서버 권한으로 관리한다.
/// </summary>
public class PlayerCombatNetState : NetworkBehaviour, IPlayerStatusReader, IPlayerCombatCommands
{
    private const float MinHealth = 0f;
    private const float DefaultMaxHealth = 100f;

    private const float MinStamina = 0f;
    private const float DefaultMaxStamina = 30f;

    private const float MinAmmo = 0f;
    private const float DefaultMaxAmmo = 0f;

    [Header("사망 처리")]
    [SerializeField] private float _deathDisableDelay = 8f;

    private IPlayerStatProvider _statProvider;
    private PlayerSfxController _playerSfxController;
    private PlayerAnimation _playerAnimation;
    private Coroutine _deathDisableCoroutine;

    private readonly NetworkVariable<float> _currentHealth = new NetworkVariable<float>(
        DefaultMaxHealth,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private readonly NetworkVariable<float> _maxHealth = new NetworkVariable<float>(
        DefaultMaxHealth,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private readonly NetworkVariable<bool> _isDead = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private readonly NetworkVariable<float> _currentStamina = new NetworkVariable<float>(
        DefaultMaxStamina,
        NetworkVariableReadPermission.Owner,
        NetworkVariableWritePermission.Server
    );

    private readonly NetworkVariable<float> _maxStamina = new NetworkVariable<float>(
        DefaultMaxStamina,
        NetworkVariableReadPermission.Owner,
        NetworkVariableWritePermission.Server
    );

    private readonly NetworkVariable<float> _currentAmmo = new NetworkVariable<float>(
        DefaultMaxAmmo,
        NetworkVariableReadPermission.Owner,
        NetworkVariableWritePermission.Server
    );

    private readonly NetworkVariable<float> _maxAmmo = new NetworkVariable<float>(
        DefaultMaxAmmo,
        NetworkVariableReadPermission.Owner,
        NetworkVariableWritePermission.Server
    );

    private readonly NetworkVariable<bool> _isReloading = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Owner,
        NetworkVariableWritePermission.Server
    );

    public float CurrentHealth => _currentHealth.Value;
    public float MaxHealth => _maxHealth.Value;
    public bool IsDead => _isDead.Value;

    public float CurrentStamina => _currentStamina.Value;
    public float MaxStamina => _maxStamina.Value;

    public float CurrentAmmo => _currentAmmo.Value;
    public float MaxAmmo => _maxAmmo.Value;

    public bool IsReloading => _isReloading.Value;

    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnAmmoChanged;
    public event Action<float, float> OnStaminaChanged;
    public event Action<bool> OnReloadStateChanged;

    public event Action OnDamageReceived;
    public event Action OnPlayerDied;
    public event Action OnPlayerRevived;
    private void Awake()
    {
        _statProvider = GetComponent<IPlayerStatProvider>();
        _playerSfxController = GetComponent<PlayerSfxController>();
        _playerAnimation = GetComponent<PlayerAnimation>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _currentHealth.OnValueChanged += HandleHealthChanged;
        _maxHealth.OnValueChanged += HandleMaxHealthChanged;
        _isDead.OnValueChanged += HandleDeadStateChanged;

        _currentAmmo.OnValueChanged += HandleAmmoChanged;
        _maxAmmo.OnValueChanged += HandleMaxAmmoChanged;
        _isReloading.OnValueChanged += HandleReloadStateChanged;

        _currentStamina.OnValueChanged += HandleStaminaChanged;
        _maxStamina.OnValueChanged += HandleMaxStaminaChanged;

        DebugTool.Log(
            $"[CombatNet] Spawn / OwnerClientId: {OwnerClientId}",
            DebugType.CombatNet,
            this
        );
    }

    public override void OnNetworkDespawn()
    {
        _currentHealth.OnValueChanged -= HandleHealthChanged;
        _maxHealth.OnValueChanged -= HandleMaxHealthChanged;
        _isDead.OnValueChanged -= HandleDeadStateChanged;

        _currentAmmo.OnValueChanged -= HandleAmmoChanged;
        _maxAmmo.OnValueChanged -= HandleMaxAmmoChanged;
        _isReloading.OnValueChanged -= HandleReloadStateChanged;

        _currentStamina.OnValueChanged -= HandleStaminaChanged;
        _maxStamina.OnValueChanged -= HandleMaxStaminaChanged;

        DebugTool.Log(
            $"[CombatNet] Despawn / OwnerClientId: {OwnerClientId}",
            DebugType.CombatNet,
            this
        );

        base.OnNetworkDespawn();
    }

    /// <summary>
    /// 서버에서 체력을 초기화한다.
    /// 이후 IPlayerStatProvider.TotalMaxHealth 값을 받아 호출하면 된다.
    /// </summary>
    public void ServerInitializeHealth(float maxHealth)
    {
        if (!IsServer)
        {
            DebugTool.Log("[CombatNet] 서버가 아니므로 체력 초기화 불가", DebugType.CombatNet, this);
            return;
        }

        float safeMaxHealth = Mathf.Max(1f, maxHealth);

        _maxHealth.Value = safeMaxHealth;
        _currentHealth.Value = safeMaxHealth;
        _isDead.Value = false;

        DebugTool.Log(
            $"[CombatNet] 체력 초기화 / OwnerClientId: {OwnerClientId}, HP: {_currentHealth.Value}/{_maxHealth.Value}",
            DebugType.CombatNet,
            this
        );
    }
    // 서버에서 최대 체력을 갱신한다.
    // 최대 체력이 증가하면 증가분만큼 현재 체력도 올린다.
    // 최대 체력이 감소하면 현재 체력은 새 최대값을 넘지 않도록 보정한다.
    public void ServerSetMaxHealth(float maxHealth)
    {
        if (!IsServer)
        {
            DebugTool.Log("[CombatNet] 서버가 아니므로 최대 체력 변경 불가", DebugType.CombatNet, this);
            return;
        }

        float safeMaxHealth = Mathf.Max(1f, maxHealth);
        float previousMaxHealth = _maxHealth.Value;
        float increaseHealth = safeMaxHealth - previousMaxHealth;

        _maxHealth.Value = safeMaxHealth;

        if (increaseHealth > 0f)
        {
            _currentHealth.Value = Mathf.Min(_maxHealth.Value, _currentHealth.Value + increaseHealth);
        }
        else
        {
            _currentHealth.Value = Mathf.Min(_currentHealth.Value, _maxHealth.Value);
        }

        DebugTool.Log(
            $"[CombatNet] 최대 체력 변경 / OwnerClientId: {OwnerClientId}, HP: {_currentHealth.Value}/{_maxHealth.Value}",
            DebugType.CombatNet,
            this
        );
    }
    // 서버에서 최대 스테미나를 갱신한다.
    // 최대 스테미나가 증가하면 증가분만큼 현재 스테미나도 올린다.
    // 최대 스테미나가 감소하면 현재 스테미나는 새 최대값을 넘지 않도록 보정한다.
    public void ServerSetMaxStamina(float maxStamina)
    {
        if (!IsServer)
        {
            DebugTool.Log("[CombatNet] 서버가 아니므로 최대 스테미나 변경 불가", DebugType.CombatNet, this);
            return;
        }

        float safeMaxStamina = Mathf.Max(0f, maxStamina);
        float previousMaxStamina = _maxStamina.Value;
        float increaseStamina = safeMaxStamina - previousMaxStamina;

        _maxStamina.Value = safeMaxStamina;

        if (increaseStamina > 0f)
        {
            _currentStamina.Value = Mathf.Min(_maxStamina.Value, _currentStamina.Value + increaseStamina);
        }
        else
        {
            _currentStamina.Value = Mathf.Min(_currentStamina.Value, _maxStamina.Value);
        }

        DebugTool.Log(
            $"[CombatNet] 최대 스테미나 변경 / OwnerClientId: {OwnerClientId}, Stamina: {_currentStamina.Value}/{_maxStamina.Value}",
            DebugType.CombatNet,
            this
        );
    }

    // 서버에서 스테미나를 초기화한다.
    // 이후 IPlayerStatProvider.TotalMaxStamina 값을 받아 호출하면 된다.
    public void ServerInitializeStamina(float maxStamina)
    {
        if (!IsServer)
        {
            DebugTool.Log("[CombatNet] 서버가 아니므로 스테미나 초기화 불가", DebugType.CombatNet, this);
            return;
        }

        float safeMaxStamina = Mathf.Max(0f, maxStamina);

        _maxStamina.Value = safeMaxStamina;
        _currentStamina.Value = safeMaxStamina;

        DebugTool.Log(
            $"[CombatNet] 스테미나 초기화 / OwnerClientId: {OwnerClientId}, Stamina: {_currentStamina.Value}/{_maxStamina.Value}",
            DebugType.CombatNet,
            this
        );
    }

    /// <summary>
    /// 서버에서 탄약을 초기화한다.
    /// 이후 IPlayerStatProvider.TotalMagazineCapacity 값을 받아 호출하면 된다.
    /// 기존 PlayerReload의 MaxBullet 역할이 여기서는 MaxAmmo가 된다.
    /// </summary>
    public void ServerInitializeAmmo(float maxAmmo)
    {
        if (!IsServer)
        {
            DebugTool.Log("[CombatNet] 서버가 아니므로 탄약 초기화 불가", DebugType.CombatNet, this);
            return;
        }

        float safeMaxAmmo = Mathf.Max(0f, maxAmmo);

        _maxAmmo.Value = safeMaxAmmo;
        _currentAmmo.Value = safeMaxAmmo;
        _isReloading.Value = false;

        DebugTool.Log(
            $"[CombatNet] 탄약 초기화 / OwnerClientId: {OwnerClientId}, Ammo: {_currentAmmo.Value}/{_maxAmmo.Value}",
            DebugType.CombatNet,
            this
        );
    }
    // 서버에서 최대 탄약을 갱신한다.
    // 최대 탄약이 증가하면 증가분만큼 현재 탄약도 올린다.
    // 최대 탄약이 감소하면 현재 탄약은 새 최대값을 넘지 않도록 보정한다.
    public void ServerSetMaxAmmo(float maxAmmo)
    {
        if (!IsServer)
        {
            DebugTool.Log("[CombatNet] 서버가 아니므로 최대 탄약 변경 불가", DebugType.CombatNet, this);
            return;
        }

        float safeMaxAmmo = Mathf.Max(0f, maxAmmo);
        float previousMaxAmmo = _maxAmmo.Value;
        float increaseAmmo = safeMaxAmmo - previousMaxAmmo;

        _maxAmmo.Value = safeMaxAmmo;

        if (increaseAmmo > 0f)
        {
            _currentAmmo.Value = Mathf.Min(_maxAmmo.Value, _currentAmmo.Value + increaseAmmo);
        }
        else
        {
            _currentAmmo.Value = Mathf.Min(_currentAmmo.Value, _maxAmmo.Value);
        }

        DebugTool.Log(
            $"[CombatNet] 최대 탄약 변경 / OwnerClientId: {OwnerClientId}, Ammo: {_currentAmmo.Value}/{_maxAmmo.Value}",
            DebugType.CombatNet,
            this
        );
    }

    /// 서버에서 체력을 회복한다.
    /// 사망 상태에서는 회복하지 않는다.
    public void ServerApplyHeal(float amount, string source)
    {
        if (!IsServer)
        {
            DebugTool.Log("[CombatNet] 서버가 아니므로 회복 적용 불가", DebugType.CombatNet, this);
            return;
        }

        if (_isDead.Value)
        {
            DebugTool.Log("[CombatNet] 사망 상태라 회복 무시", DebugType.CombatNet, this);
            return;
        }

        if (amount <= 0f)
        {
            DebugTool.Log($"[CombatNet] 회복량이 0 이하라 무시 / Amount: {amount}", DebugType.CombatNet, this);
            return;
        }

        _currentHealth.Value = Mathf.Min(_maxHealth.Value, _currentHealth.Value + amount);

        DebugTool.Log(
            $"[CombatNet] 회복 적용 / OwnerClientId: {OwnerClientId}, Amount: {amount}, Source: {source}, HP: {_currentHealth.Value}/{_maxHealth.Value}",
            DebugType.CombatNet,
            this
        );
    }

    /// 서버에서 데미지를 적용한다.
    /// 클라이언트는 직접 체력을 깎지 않고 서버 판정 이후 이 함수를 통해 처리한다.
    public void ServerApplyDamage(float damage, ulong attackerClientId, string source)
    {
        if (!IsServer)
        {
            DebugTool.Log("[CombatNet] 서버가 아니므로 데미지 적용 불가", DebugType.CombatNet, this);
            return;
        }

        if (_isDead.Value)
        {
            DebugTool.Log("[CombatNet] 이미 사망한 플레이어라 데미지 무시", DebugType.CombatNet, this);
            return;
        }

        if (damage <= 0f)
        {
            DebugTool.Log($"[CombatNet] 데미지가 0 이하라 무시 / Damage: {damage}", DebugType.CombatNet, this);
            return;
        }

        _currentHealth.Value = Mathf.Max(MinHealth, _currentHealth.Value - damage);

        PlayDamageClientRpc();

        DebugTool.Log(
            $"[CombatNet] 데미지 적용 / OwnerClientId: {OwnerClientId}, Attacker: {attackerClientId}, Damage: {damage}, Source: {source}, HP: {_currentHealth.Value}/{_maxHealth.Value}",
            DebugType.CombatNet,
            this
        );

        if (_currentHealth.Value <= MinHealth)
        {
            ServerKill(source);
        }
    }

    [ClientRpc]
    private void PlayDamageClientRpc()
    {
        if (_playerSfxController == null)
        {
            DebugTool.Log("[CombatNet] PlayerSfxController가 없어 피격 소리 재생 불가", DebugType.Audio, this);
        }
        else if (IsFemaleCharacter())
        {
            _playerSfxController.PlayFemaleHitSfx();
        }
        else
        {
            _playerSfxController.PlayMaleHitSfx();
        }

        OnDamageReceived?.Invoke();

        DebugTool.Log(
            $"[CombatNet] 피격 연출 동기화 / OwnerClientId: {OwnerClientId}",
            DebugType.CombatNet,
            this
        );
    }

    /// 서버에서 플레이어를 사망 처리한다.
    public void ServerKill(string reason)
    {
        if (!IsServer)
        {
            DebugTool.Log("[CombatNet] 서버가 아니므로 사망 처리 불가", DebugType.CombatNet, this);
            return;
        }

        if (_isDead.Value)
        {
            return;
        }

        _currentHealth.Value = MinHealth;
        _isDead.Value = true;
        _isReloading.Value = false;

        DebugTool.Log(
            $"[CombatNet] 사망 처리 / OwnerClientId: {OwnerClientId}, Reason: {reason}",
            DebugType.CombatNet,
            this
        );
    }

    /// 서버에서 플레이어를 부활 처리한다.
    /// healthRatio는 MaxHealth 기준 0~1 비율이다.
    public void ServerRevive(float healthRatio)
    {
        if (!IsServer)
        {
            DebugTool.Log("[CombatNet] 서버가 아니므로 부활 처리 불가", DebugType.CombatNet, this);
            return;
        }

        if (!_isDead.Value)
        {
            DebugTool.Log("[CombatNet] 사망 상태가 아니라 부활 무시", DebugType.CombatNet, this);
            return;
        }

        float safeRatio = Mathf.Clamp01(healthRatio);
        float reviveHealth = Mathf.Max(1f, _maxHealth.Value * safeRatio);

        _isDead.Value = false;
        _currentHealth.Value = reviveHealth;

        DebugTool.Log(
            $"[CombatNet] 부활 처리 / OwnerClientId: {OwnerClientId}, HP: {_currentHealth.Value}/{_maxHealth.Value}",
            DebugType.CombatNet,
            this
        );
    }

    /// 서버에서 탄약을 사용한다.
    /// PlayerReload 연결 단계에서 사용한다.
    public bool ServerTryConsumeAmmo(float amount)
    {
        if (!IsServer)
        {
            DebugTool.Log("[CombatNet] 서버가 아니므로 탄약 사용 불가", DebugType.CombatNet, this);
            return false;
        }

        if (_isDead.Value || _isReloading.Value)
        {
            DebugTool.Log("[CombatNet] 사망 또는 재장전 중이라 탄약 사용 불가", DebugType.CombatNet, this);
            return false;
        }

        if (amount <= 0f)
        {
            return false;
        }

        if (_currentAmmo.Value < amount)
        {
            DebugTool.Log("[CombatNet] 탄약 부족", DebugType.CombatNet, this);
            return false;
        }

        _currentAmmo.Value = Mathf.Max(MinAmmo, _currentAmmo.Value - amount);

        DebugTool.Log(
            $"[CombatNet] 탄약 사용 / OwnerClientId: {OwnerClientId}, Ammo: {_currentAmmo.Value}/{_maxAmmo.Value}",
            DebugType.CombatNet,
            this
        );

        return true;
    }

    // 서버에서 재장전 상태를 설정한다.
    public void ServerSetReloading(bool isReloading)
    {
        if (!IsServer)
        {
            DebugTool.Log("[CombatNet] 서버가 아니므로 재장전 상태 변경 불가", DebugType.CombatNet, this);
            return;
        }

        if (_isDead.Value)
        {
            _isReloading.Value = false;
            return;
        }

        _isReloading.Value = isReloading;

        DebugTool.Log(
            $"[CombatNet] 재장전 상태 변경 / OwnerClientId: {OwnerClientId}, IsReloading: {_isReloading.Value}",
            DebugType.CombatNet,
            this
        );
    }

    // 서버에서 탄약을 최대 탄약으로 채운다.
    public void ServerFillAmmo()
    {
        if (!IsServer)
        {
            DebugTool.Log("[CombatNet] 서버가 아니므로 탄약 충전 불가", DebugType.CombatNet, this);
            return;
        }

        _currentAmmo.Value = _maxAmmo.Value;
        _isReloading.Value = false;

        DebugTool.Log(
            $"[CombatNet] 탄약 충전 / OwnerClientId: {OwnerClientId}, Ammo: {_currentAmmo.Value}/{_maxAmmo.Value}",
            DebugType.CombatNet,
            this
        );
    }

    /// <summary>
    /// 서버에서 스테미나를 사용한다.
    /// PlayerStamina 연결 단계에서 사용한다.
    /// </summary>
    public bool ServerTryUseStamina(float amount)
    {
        if (!IsServer)
        {
            DebugTool.Log("[CombatNet] 서버가 아니므로 스테미나 사용 불가", DebugType.CombatNet, this);
            return false;
        }

        if (_isDead.Value)
        {
            return false;
        }

        if (amount <= 0f)
        {
            return false;
        }

        if (_currentStamina.Value <= amount)
        {
            _currentStamina.Value = MinStamina;

            DebugTool.Log("[CombatNet] 스테미나 부족", DebugType.CombatNet, this);
            return false;
        }

        _currentStamina.Value = Mathf.Max(MinStamina, _currentStamina.Value - amount);

        DebugTool.Log(
            $"[CombatNet] 스테미나 사용 / OwnerClientId: {OwnerClientId}, Stamina: {_currentStamina.Value}/{_maxStamina.Value}",
            DebugType.CombatNet,
            this
        );

        return true;
    }

    // 서버에서 스테미나를 회복한다.
    public void ServerRecoverStamina(float amount)
    {
        if (!IsServer)
        {
            DebugTool.Log("[CombatNet] 서버가 아니므로 스테미나 회복 불가", DebugType.CombatNet, this);
            return;
        }

        if (_isDead.Value)
        {
            return;
        }

        if (amount <= 0f)
        {
            return;
        }

        _currentStamina.Value = Mathf.Min(_maxStamina.Value, _currentStamina.Value + amount);
    }

    private void HandleHealthChanged(float previousValue, float newValue)
    {
        OnHealthChanged?.Invoke(newValue, _maxHealth.Value);

        DebugTool.Log(
            $"[CombatNet] 체력 동기화 / OwnerClientId: {OwnerClientId}, {previousValue} -> {newValue}",
            DebugType.CombatNet,
            this
        );
    }

    private void HandleMaxHealthChanged(float previousValue, float newValue)
    {
        OnHealthChanged?.Invoke(_currentHealth.Value, newValue);

        DebugTool.Log(
            $"[CombatNet] 최대 체력 동기화 / OwnerClientId: {OwnerClientId}, {previousValue} -> {newValue}",
            DebugType.CombatNet,
            this
        );
    }

    private void HandleDeadStateChanged(bool previousValue, bool newValue)
    {
        if (previousValue == newValue)
        {
            return;
        }

        if (newValue)
        {
            PlayDeathSfx();

            OnPlayerDied?.Invoke();
            PlayDeathAnimationByNetwork();
            StartDisableAfterDeath();

            DebugTool.Log(
                $"[CombatNet] 사망 상태 동기화 / OwnerClientId: {OwnerClientId}, IsOwner: {IsOwner}",
                DebugType.CombatNet,
                this
            );

            return;
        }

        OnPlayerRevived?.Invoke();

        DebugTool.Log(
            $"[CombatNet] 부활 상태 동기화 / OwnerClientId: {OwnerClientId}",
            DebugType.CombatNet,
            this
        );
    }
    private void HandleAmmoChanged(float previousValue, float newValue)
    {
        OnAmmoChanged?.Invoke(newValue, _maxAmmo.Value);

        DebugTool.Log(
            $"[CombatNet] 탄약 동기화 / OwnerClientId: {OwnerClientId}, {previousValue} -> {newValue}",
            DebugType.CombatNet,
            this
        );
    }

    private void HandleMaxAmmoChanged(float previousValue, float newValue)
    {
        OnAmmoChanged?.Invoke(_currentAmmo.Value, newValue);

        DebugTool.Log(
            $"[CombatNet] 최대 탄약 동기화 / OwnerClientId: {OwnerClientId}, {previousValue} -> {newValue}",
            DebugType.CombatNet,
            this
        );
    }

    private void HandleStaminaChanged(float previousValue, float newValue)
    {
        OnStaminaChanged?.Invoke(newValue, _maxStamina.Value);

        DebugTool.Log(
            $"[CombatNet] 스테미나 동기화 / OwnerClientId: {OwnerClientId}, {previousValue} -> {newValue}",
            DebugType.CombatNet,
            this
        );
    }

    private void HandleMaxStaminaChanged(float previousValue, float newValue)
    {
        OnStaminaChanged?.Invoke(_currentStamina.Value, newValue);

        DebugTool.Log(
            $"[CombatNet] 최대 스테미나 동기화 / OwnerClientId: {OwnerClientId}, {previousValue} -> {newValue}",
            DebugType.CombatNet,
            this
        );
    }

    private void HandleReloadStateChanged(bool previousValue, bool newValue)
    {
        OnReloadStateChanged?.Invoke(newValue);

        DebugTool.Log(
            $"[CombatNet] 재장전 상태 동기화 / OwnerClientId: {OwnerClientId}, {previousValue} -> {newValue}",
            DebugType.CombatNet,
            this
        );
    }
    private bool IsFemaleCharacter()
    {
        if (_statProvider == null)
        {
            DebugTool.Log("[CombatNet] IPlayerStatProvider가 없어 남자 SFX로 재생", DebugType.Audio, this);
            return false;
        }

        return _statProvider.WeaponType == WeaponType.Pistol ||
               _statProvider.WeaponType == WeaponType.Shotgun;
    }

    private void PlayDeathSfx()
    {
        if (_playerSfxController == null)
        {
            DebugTool.Log("[CombatNet] PlayerSfxController가 없어 죽음 소리 재생 불가", DebugType.Audio, this);
            return;
        }

        if (IsFemaleCharacter())
        {
            _playerSfxController.PlayFemaleDeathSfx();
            return;
        }

        _playerSfxController.PlayMaleDeathSfx();
    }
    private void StartDisableAfterDeath()
    {
        if (_deathDisableCoroutine != null)
        {
            StopCoroutine(_deathDisableCoroutine);
        }

        _deathDisableCoroutine = StartCoroutine(DisableAfterDeathRoutine());
    }

    private IEnumerator DisableAfterDeathRoutine()
    {
        float delay = Mathf.Max(0f, _deathDisableDelay);
        yield return new WaitForSeconds(delay);

        gameObject.SetActive(false);

        DebugTool.Log(
            $"[CombatNet] 사망 애니메이션 후 플레이어 비활성화 / OwnerClientId: {OwnerClientId}",
            DebugType.CombatNet,
            this
        );
    }
    private void PlayDeathAnimationByNetwork()
    {
        if (_playerAnimation == null)
        {
            DebugTool.Log("[CombatNet] PlayerAnimation이 없어 죽음 애니메이션 재생 불가", DebugType.Character, this);
            return;
        }

        _playerAnimation.SetIdleByNetwork();
        _playerAnimation.PlayDieByNetwork();

        DebugTool.Log("[CombatNet] 죽음 애니메이션 네트워크 연출 실행", DebugType.Character, this);
    }
}

/*
Unity 적용 방법
1. 기존 PlayerCombatNetState.cs 전체를 이 코드로 교체한다.
2. 플레이어 프리팹 루트 오브젝트에 PlayerCombatNetState가 붙어 있는지 확인한다.
*/