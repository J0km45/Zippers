using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using Audio;

public class ZombieController : MonoBehaviour, IDamagable//NetworkBehaviour
{
    [SerializeField] private ZombieStatSO _stat;

    [Tooltip("Groan Sfx 재생 주기")]
    [SerializeField] private float _groanSfxInterval = 3f;
    [Tooltip("Groan Sfx 재생 확률")]
    [SerializeField] private float _groanSfxChance = 0.3f;
    [Tooltip("피격 시 경직 시간")]
    [SerializeField] private float _stunDuration = 0.2f;
    [Tooltip("재화 프리팹")]
    [SerializeField] private GameObject _rewardPrefab;

    private StateMachine _stateMachine;
    private NavMeshAgent _agent;
    private Animator _animator;
    private ZombieSfxController _sfx;
    private bool _isDead;
    private bool _hasSpawnedReward;
    private float _timer;

    public ZombieChaseState Chase { get; private set; }
    public ZombieAttackState Attack { get; private set; }
    public ZombieHitState Hit { get; private set; }
    public ZombieDieState Die { get; private set; }
    public NavMeshAgent Agent => _agent;
    public Animator Animator => _animator;
    public ZombieSfxController Sfx => _sfx;
    public float StunDuration => _stunDuration;

    //public NetworkVariable<int> CurrentHp = new NetworkVariable<float>();
    public float CurrentHp; //임시(테스트용)
    public Transform Player; //임시(테스트용)
    public LayerMask PlayerLayer;
    public Transform LeftHand;
    public Transform RightHand;
    public float LastAttackTime { get; private set; } = 0f;

    public ZombieType Type => _stat.Type;
    public float MaxHp => _stat.MaxHp;
    public float MoveSpeed => _stat.MoveSpeed;
    public float DetectMoveSpeed => _stat.DetectMoveSpeed;
    public float MinAttackDamage => _stat.MinAttackDamage;
    public float MaxAttackDamage => _stat.MaxAttackDamage;
    public float AttackCooldown => _stat.AttackCooldown;
    public float HandRadius => _stat.HandRadius;
    public float AttackRange => _stat.AttackRange;
    public float DetectRange => _stat.DetectRange;

    private void Awake()
    {
        _stateMachine = new StateMachine();
        Chase = new ZombieChaseState(this);
        Attack = new ZombieAttackState(this);
        Hit = new ZombieHitState(this);
        Die = new ZombieDieState(this);

        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponentInChildren<Animator>();
        _sfx = GetComponent<ZombieSfxController>();
        _agent.stoppingDistance = AttackRange;
        CurrentHp = MaxHp; // 임시(테스트용)
    }
    // TODO : NGO 적용되면 수정
    //public override void OnNetworkSpawn()
    //{
    //    if (IsServer)
    //    {
    //        CurrentHp.Value = MaxHp;
    //    }
    //}

    private void Start()
    {
        _stateMachine.ChangeState(Chase);
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

        if(_timer >= _groanSfxInterval)
        {
            _timer = 0f;
            if (Random.value < _groanSfxChance)
            {
                if (Type == ZombieType.Boss)
                {
                    _sfx.PlayBossGroanSfx();
                }
                else
                {
                    _sfx.PlayGroanSfx();
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
        return Time.time >= LastAttackTime + AttackCooldown;
    }

    public void SetAttackCooldown()
    {
        // if (!IsServer) return;
        // TODO : 서버시간으로 변경 필요
        LastAttackTime = Time.time;
    }

    public void OnAttackHit() => Attack.OnAttackHit();

    public void OnAttackEnd() => Attack.OnAttackEnd();

    public void OnFootStep() => Chase.OnFootStep();

    public void OnAttackSfx() => Attack.OnAttackSfx();

    // TODO : NGO 적용되면 수정
    public void TakeDamage(float damage)
    {
        if (_isDead) return;

        _sfx.PlayHitSfx();
        CurrentHp -= damage;

        if (CurrentHp <= 0)
        {
            _isDead = true;
            _sfx.PlayDeathSfx();
            ChangeState(Die);
            return;
        }

        ChangeState(Hit);
    }

    public void SpawnReward()
    {
        if (_hasSpawnedReward) return;
        
        _hasSpawnedReward = true;
        // TODO : 수정해야됨
        //_sfx.PlayDropResourcesSfx(ResourcesType.Scrap);
        //Instantiate(_rewardPrefab, transform.position, Quaternion.identity);
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
