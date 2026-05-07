using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using Audio;

public class ZombieController : MonoBehaviour, IDamagable, IPoolable//NetworkBehaviour
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

    [Header("재화 프리팹")]
    [Tooltip("개인 재화")]
    [SerializeField] private GameObject _scrapPrefab;
    [Tooltip("팀 재화")]
    [SerializeField] private GameObject _suppliesPrefab;
    [Tooltip("메타 재화")]
    [SerializeField] private GameObject _infectionSamplePrefab;

    private StateMachine _stateMachine;
    private ZombieCountManager _zombieCount;
    private bool _isDead; // 죽음 상태 여부
    private bool _hasSpawnedReward; // 보상 생성 여부
    private bool _isCountRemoved; // 카운트 제거 여부
    private float _timer; // Groan Sfx 재생용 타이머
    private float _lastAttackTime = 0f; // 공격 쿨타임 관리용 시간

    public ZombieChaseState Chase { get; private set; }
    public ZombieAttackState Attack { get; private set; }
    public ZombieHitState Hit { get; private set; }
    public ZombieDieState Die { get; private set; }

    public NavMeshAgent Agent { get; private set; }
    public Animator Animator { get; private set; }
    public ZombieSfxController Sfx { get; private set; }
    public IZombieAttack ZombieAttack { get; private set; }
    public LayerMask PlayerLayer => _playerLayer;
    public Transform LeftHand => _leftHand;
    public Transform RightHand => _rightHand;
    public float StunDuration => _stunDuration;

    //public NetworkVariable<int> CurrentHp = new NetworkVariable<float>();
    public float CurrentHp; //임시(테스트용)
    public Transform Player;

    public ZombieType Type => _stat.ZombieType;
    public float MaxHp => _stat.MaxHealth;
    public float MoveSpeed => _stat.BaseMoveSpeed;
    public float DetectMoveSpeed => _stat.ChasingMoveSpeed;
    public float MinAttackDamage => _stat.MinDamage;
    public float MaxAttackDamage => _stat.MaxDamage;
    public float AttackCooldown => _stat.AttackSpeed;
    public float HandRadius => _stat.HandRadius;
    public float AttackRange => _stat.AttackRange;
    public float DetectRange => _stat.DetectionRange;

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
    // TODO : NGO 적용되면 수정
    //public override void OnNetworkSpawn()
    //{
    //    if (IsServer)
    //    {
    //        CurrentHp.Value = MaxHp;
    //    }
    //}

    public void Init(ZombieCountManager zombieCount)
    {
        _zombieCount = zombieCount;
        _zombieCount.AddCount();
    }

    public void OnSpawn()
    {
        ResetZombie();
    }

    private void ResetZombie()
    {
        _isDead = false;
        _hasSpawnedReward = false;
        _isCountRemoved = false;

        CurrentHp = MaxHp;

        _timer = 0f;
        _lastAttackTime = 0f;

        if (TryGetComponent(out Collider collider))
        {
            collider.enabled = true;
        }

        Agent.enabled = true;
        Agent.isStopped = false;
        Agent.stoppingDistance = AttackRange;

        ChangeState(Chase);
    }

    public void OnDespawn()
    {
        if (_isCountRemoved) return;
        
        _isCountRemoved = true;
        _zombieCount.RemoveCount();
    }

    private void Update()
    {
        PlayGroanSfx();
        // TODO : 플레이어 위치 받아오는거 필요함
        //Player = 가장 가까운 생존 플레이어
        _stateMachine.Update();
    }

    private void PlayGroanSfx()
    {
        if (_isDead) return;
        // TODO : NGO 적용되면 서버에서 타이머 관리하도록 변경
        _timer += Time.deltaTime;

        if (_timer >= _groanSfxInterval)
        {
            _timer = 0f;
            if (Random.value < _groanSfxChance)
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
        // if (!IsServer) return false;
        // TODO : 서버시간으로 변경 필요
        return Time.time >= _lastAttackTime + AttackCooldown;
    }

    public void SetAttackCooldown()
    {
        // if (!IsServer) return;
        // TODO : 서버시간으로 변경 필요
        _lastAttackTime = Time.time;
    }

    public void OnAttackHit() => Attack.OnAttackHit();

    public void OnAttackEnd() => Attack.OnAttackEnd();

    public void OnFootStep() => Chase.OnFootStep();

    public void OnAttackSfx() => Attack.OnAttackSfx();

    // TODO : NGO 적용되면 수정
    public void TakeDamage(float damage)
    {
        if (_isDead) return;

        Sfx.PlayHitSfx();
        CurrentHp -= damage;

        if (CurrentHp <= 0)
        {
            _isDead = true;
            Sfx.PlayDeathSfx();
            ChangeState(Die);
            return;
        }

        ChangeState(Hit);
    }

    public void SpawnReward()
    {
        if (_hasSpawnedReward) return;

        _hasSpawnedReward = true;

        Sfx.PlayDropResourcesSfx(ResourcesType.Scrap);
        TrySpawnReward(_scrapPrefab, ResourcesType.Scrap, ScrapDropChance, MinScrap, MaxScrap);
        TrySpawnReward(_suppliesPrefab, ResourcesType.Supplies, SuppliesDropChance, MinSupplies, MaxSupplies);
        TrySpawnReward(_infectionSamplePrefab, ResourcesType.InfectionSample, SampleDropChance, InfectionSample, InfectionSample);
    }

    private void TrySpawnReward(GameObject prefab, ResourcesType type, float dropChance, int minAmount, int maxAmount)
    {
        float randomChance = Random.Range(0f, 100f);
        if (randomChance > dropChance) return;

        int amount = Random.Range(minAmount, maxAmount + 1);

        Vector2 randomPos = Random.insideUnitCircle;
        Vector3 spawnPos = transform.position + new Vector3(randomPos.x, 0, randomPos.y);

        GameObject obj = PoolManager.Instance.Get(prefab, spawnPos, Quaternion.identity);

        if (obj.TryGetComponent(out Reward reward))
        {
            reward.Init(type, amount);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(gameObject.transform.position, DetectRange); // 감지 범위
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere((LeftHand.position + RightHand.position) * 0.5f, HandRadius); // 손 범위
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(gameObject.transform.position, AttackRange); // 공격 범위
    }
}
