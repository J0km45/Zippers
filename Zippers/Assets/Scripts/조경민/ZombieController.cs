using UnityEngine;
using UnityEngine.AI;
//using Unity.Netcode;

public class ZombieController : MonoBehaviour
{
    [SerializeField] private ZombieStatSO _stat;

    private StateMachine _stateMachine;
    private NavMeshAgent _agent;
    private Animator _animator;
    
    public ZombieChaseState Chase { get; private set; }
    public NavMeshAgent Agent => _agent;
    public Animator Animator => _animator;

    //public NetworkVariable<int> CurrentHp = new NetworkVariable<float>();
    public float CurrentHp; //임시(테스트용)
    public Transform Player; //임시(테스트용)


    public ZombieType Type => _stat.Type;
    public float MaxHp => _stat.MaxHp;
    public float MoveSpeed => _stat.MoveSpeed;
    public float DetectMoveSpeed => _stat.DetectMoveSpeed;
    public float AttackDamage=> _stat.AttackDamage;
    public float AttackCooldown => _stat.AttackCooldown;
    public float AttackRange => _stat.AttackRange;
    public float DetectRange => _stat.DetectRange;
    public float KillReward => _stat.KillReward;

    private void Awake()
    {
        _stateMachine = new StateMachine();
        Chase = new ZombieChaseState(this);

        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponentInChildren<Animator>();
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
        _stateMachine.Update();
    }

    public void ChangeState(IState state)
    {
        _stateMachine.ChangeState(state);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(gameObject.transform.position, DetectRange);
    }
}
