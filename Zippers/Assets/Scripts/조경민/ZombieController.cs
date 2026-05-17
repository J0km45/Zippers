using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using Audio;

public class ZombieController : NetworkBehaviour, IDamagable
{
    [SerializeField] private ZombieStatSO _stat;

    [Tooltip("플레이어 레이어")]
    [SerializeField] private LayerMask _playerLayer;
    [Tooltip("왼손 위치")]
    [SerializeField] private Transform _leftHand;
    [Tooltip("오른손 위치")]
    [SerializeField] private Transform _rightHand;
    [Tooltip("Groan Sfx 재생 주기")]
    [SerializeField] private float _groanSfxInterval = 3f;
    [Tooltip("Groan Sfx 재생 확률")]
    [SerializeField] private float _groanSfxChance = 0.3f;
    [Tooltip("피격 시 경직 시간")]
    [SerializeField] private float _stunDuration = 0.2f;
    [Tooltip("플레이어 감지 주기")]
    [SerializeField] private float _playerDetectInterval = 0.5f;

    [Header("재화 프리팹")]
    [SerializeField] private ResourceSo _resourceSO;

    private StateMachine _stateMachine;
    private ZombieCountManager _zombieCount;
    private bool _isDead; // 죽음 상태 여부
    private bool _hasSpawnedReward; // 보상 생성 여부
    private bool _isCountRemoved; // 카운트 제거 여부
    private float _groanSfxTimer; // Groan Sfx 재생용 타이머
    private float _lastAttackTime; // 공격 쿨타임 관리용 시간
    private float _playerDetectTimer; // 플레이어 감지용 타이머
    private float _healthRegenTimer; // 체력 재생용 타이머

    public ZombieChaseState Chase { get; private set; }
    public ZombieAttackState Attack { get; private set; }
    public ZombieHitState Hit { get; private set; }
    public ZombieDieState Die { get; private set; }

    public NavMeshAgent Agent { get; private set; }
    public Animator Animator { get; private set; }
    public ZombieSfxController Sfx { get; private set; }
    public IZombieAttack ZombieAttack { get; private set; }
    public Transform Player { get; private set; }
    public NodeManager NodeManager { get; private set; }
    public LayerMask PlayerLayer => _playerLayer;
    public Transform LeftHand => _leftHand;
    public Transform RightHand => _rightHand;
    public float StunDuration => _stunDuration;

    public NetworkVariable<float> CurrentHp = new NetworkVariable<float>();

    public ZombieType Type => _stat.ZombieType;
    public float MaxHp => _stat.MaxHealth * NodeScaling.GetMultiplier(NodeManager.BattleCount).Health;
    public float MoveSpeed => _stat.BaseMoveSpeed * NodeScaling.GetMultiplier(NodeManager.BattleCount).MoveSpeed;
    public float DetectMoveSpeed => _stat.ChasingMoveSpeed * NodeScaling.GetMultiplier(NodeManager.BattleCount).MoveSpeed;
    public float MinAttackDamage => _stat.MinDamage;
    public float MaxAttackDamage => _stat.MaxDamage;
    public float AttackCooldown => _stat.AttackSpeed * NodeScaling.GetMultiplier(NodeManager.BattleCount).AttackSpeed;
    public float HandRadius => _stat.HandRadius;
    public float AttackRange => _stat.AttackRange;
    public float DetectRange => _stat.DetectionRange;
    public float HealthRegen => _stat.HealthRegen;
    public float HealthPeriod => _stat.HealthPeriod;

    public int MinScrap => _stat.MinScrap;
    public int MaxScrap => _stat.MaxScrap;
    public float ScrapDropChance => _stat.ScrapDropChance;
    public int MinSupplies => _stat.MinSupplies;
    public int MaxSupplies => _stat.MaxSupplies;
    public float SuppliesDropChance => _stat.SuppliesDropChance;
    public int InfectionSample => _stat.InfectionSample;
    public float SampleDropChance => _stat.SampleDropChance;

    private void Awake()
    {
        _stateMachine = new StateMachine();
        Chase = new ZombieChaseState(this);
        Attack = new ZombieAttackState(this);
        Hit = new ZombieHitState(this);
        Die = new ZombieDieState(this);

        Agent = GetComponent<NavMeshAgent>();
        Animator = GetComponentInChildren<Animator>();
        Sfx = GetComponent<ZombieSfxController>();
        ZombieAttack = GetComponent<IZombieAttack>();
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        if (_isCountRemoved) return;

        _isCountRemoved = true;
        _zombieCount.RemoveCount();
    }

    public void Init(ZombieCountManager zombieCount, NodeManager nodeManager)
    {
        _zombieCount = zombieCount;
        NodeManager = nodeManager;
        ResetZombie();
        _zombieCount.AddCount();
    }

    private void ResetZombie()
    {
        _isDead = false;
        _hasSpawnedReward = false;
        _isCountRemoved = false;

        if (IsServer)
        {
            CurrentHp.Value = MaxHp;
        }

        _groanSfxTimer = 0f;
        _lastAttackTime = 0f;
        _playerDetectTimer = 0f;
        _healthRegenTimer = 0f;

        if (TryGetComponent(out Collider collider))
        {
            collider.enabled = true;
        }

        if (!Agent.enabled)
        {
            Agent.enabled = true;
        }

        Agent.Warp(transform.position);
        if (Agent.isOnNavMesh)
        {
            Agent.isStopped = false;
            Agent.stoppingDistance = AttackRange;
        }
        else
        {
            DebugTool.Log("좀비가 NavMesh 위에 있지 않음", DebugType.Zombie, this);
        }

        RefreshPlayer();
        ChangeState(Chase);
    }

    private void Update()
    {
        if (!IsServer) return;

        PlayGroanSfx();
        UpdatePlayer();
        RegenHealth();
        _stateMachine.Update();
    }

    private void PlayGroanSfx()
    {
        if (_isDead) return;

        _groanSfxTimer += Time.deltaTime;

        if (_groanSfxTimer >= _groanSfxInterval)
        {
            _groanSfxTimer = 0f;
            if (Random.value < _groanSfxChance)
            {
                PlayGroanSfxClientRpc();
            }
        }
    }

    [ClientRpc]
    private void PlayGroanSfxClientRpc()
    {
        if (Type == ZombieType.Boss)
        {
            Sfx.PlayBossGroanSfx();
        }
        else
        {
            Sfx.PlayGroanSfx();
        }
    }

    private void UpdatePlayer()
    {
        _playerDetectTimer += Time.deltaTime;

        if (_playerDetectTimer < _playerDetectInterval) return;

        _playerDetectTimer = 0f;
        RefreshPlayer();
    }

    private void RefreshPlayer()
        => Player = PlayerTransformList.instance.GetClosestPlayer(transform.position);

    private void RegenHealth()
    {
        if (_isDead) return;
        if (CurrentHp.Value >= MaxHp) return;

        _healthRegenTimer += Time.deltaTime;

        if (_healthRegenTimer >= HealthPeriod)
        {
            _healthRegenTimer = 0f;
            float helathRegenAmount = MaxHp * (HealthRegen / 100f);
            CurrentHp.Value += helathRegenAmount;
            CurrentHp.Value = Mathf.Min(CurrentHp.Value, MaxHp);
            DebugTool.Log($"좀비 체력 회복: {helathRegenAmount}, 현재 체력: {CurrentHp.Value}", DebugType.Zombie, this);
        }
    }

    public void ChangeState(IState state)
    {
        _stateMachine.ChangeState(state);
    }

    public float GetDistanceToPlayer()
    {
        return Vector3.Distance(transform.position, Player.position);
    }

    public bool CanAttack()
    {
        if (!IsServer) return false;

        return Time.time >= _lastAttackTime + AttackCooldown;
    }

    public void SetAttackCooldown()
    {
        if (!IsServer) return;

        _lastAttackTime = Time.time;
    }

    public void OnAttackHit()
    {
        if (!IsServer) return;

        Attack.OnAttackHit();
    }

    public void OnAttackEnd()
    {
        if (!IsServer) return;

        Attack.OnAttackEnd();
    }

    public void OnFootStep()
    {
        if (!IsServer) return;

        PlayFootStepSfxClientRpc();
    }

    [ClientRpc]
    private void PlayFootStepSfxClientRpc()
    {
        if (Type == ZombieType.Boss)
        {
            Sfx.PlayBossMoveSfx();
        }
        else
        {
            Sfx.PlayMoveSfx();
        }
    }

    public void OnAttackSfx()
    {
        if (!IsServer) return;

        PlayAttackSfxClientRpc();
    }

    [ClientRpc]
    private void PlayAttackSfxClientRpc()
    {
        Sfx.PlayAttackSfx(Type);
    }

    public void TakeDamage(float damage)
    {
        if (!IsServer) return;
        if (_isDead) return;

        PlayHitSfxClientRpc();
        CurrentHp.Value -= damage;
        DebugTool.Log($"좀비가 {damage} 데미지 입음. 현재 체력: {CurrentHp.Value}", DebugType.Zombie, this);

        if (CurrentHp.Value <= 0)
        {
            _isDead = true;
            PlayDeathSfxClientRpc();
            ChangeState(Die);
            return;
        }

        ChangeState(Hit);
    }

    [ClientRpc]
    private void PlayHitSfxClientRpc()
    {
        Sfx.PlayHitSfx();
    }

    [ClientRpc]
    private void PlayDeathSfxClientRpc()
    {
        Sfx.PlayDeathSfx();
    }

    public void SpawnReward()
    {
        if (!IsServer) return;
        if (_hasSpawnedReward) return;

        _hasSpawnedReward = true;

        TrySpawnReward(ResourcesType.Scrap, ScrapDropChance, MinScrap, MaxScrap);
        TrySpawnReward(ResourcesType.Supplies, SuppliesDropChance, MinSupplies, MaxSupplies);
        TrySpawnReward(ResourcesType.InfectionSample, SampleDropChance, InfectionSample, InfectionSample);
    }

    private void TrySpawnReward(ResourcesType type, float dropChance, int minAmount, int maxAmount)
    {
        float randomChance = Random.Range(0f, 100f);
        if (randomChance > dropChance) return;

        PlayDropResourcesSfxClientRpc(type);
        int amount = Random.Range(minAmount, maxAmount + 1);
        float finalAmount = amount;
        if (type == ResourcesType.Scrap)
        {
            finalAmount *= NodeScaling.GetMultiplier(NodeManager.BattleCount).ScrapDrop;
        }
        else if (type == ResourcesType.Supplies)
        {
            finalAmount *= NodeScaling.GetMultiplier(NodeManager.BattleCount).SupplyDrop;
        }

        Vector2 randomPos = Random.insideUnitCircle;
        Vector3 spawnPos = transform.position + new Vector3(randomPos.x, 0, randomPos.y);

        GameObject obj = PoolManager.Instance.Get(_resourceSO.GetResource(type), spawnPos, Quaternion.identity);

        if (obj.TryGetComponent(out Reward reward))
        {
            reward.Init(type, finalAmount);
        }
    }

    [ClientRpc]
    private void PlayDropResourcesSfxClientRpc(ResourcesType type)
    {
        Sfx.PlayDropResourcesSfx(type);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(gameObject.transform.position, DetectRange); // 감지 범위
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere((LeftHand.position + RightHand.position) * 0.5f, HandRadius); // 손 범위
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(RightHand.position, HandRadius); // 오른손(보스)
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(gameObject.transform.position, AttackRange); // 공격 범위
    }
}
