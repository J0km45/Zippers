using Audio;
using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    private StateMachine _stateMachine;
    private PlayerCombatStateMachine _combatStateMachine;

    private PlayerMovement _playerMovement;
    private PlayerAnimation _playerAnimation;
    private PlayerHealth _playerHealth;

    private PlayerIdleState _idleState;
    private PlayerMoveState _moveState;
    private PlayerHitState _hitState;
    private PlayerRetireState _retireState;

    private PlayerSfxController _sfxController;

    private PlayerStateType _playerStateType;
    private Vector2 _moveInput;

    public Vector2 MoveInput => _moveInput;
    public PlayerStateType PlayerStateType => _playerStateType;

    public bool IsRetired => _playerStateType == PlayerStateType.Retire;


    private void Awake()
    {
        _stateMachine = new StateMachine();
        _combatStateMachine = GetComponent<PlayerCombatStateMachine>();

        _playerMovement = GetComponent<PlayerMovement>();
        _playerAnimation = GetComponent<PlayerAnimation>();
        _playerHealth = GetComponent<PlayerHealth>();
        _sfxController = GetComponent<PlayerSfxController>();

        _idleState = new PlayerIdleState(this, _playerMovement, _playerAnimation);
        _moveState = new PlayerMoveState(this, _playerMovement, _playerAnimation, _sfxController);
        _hitState = new PlayerHitState(this, _playerMovement, _playerAnimation, 0.25f, _sfxController);
        _retireState = new PlayerRetireState(this, _playerMovement, _playerAnimation, GetComponent<BoxCollider>(),_sfxController);
    }
    private void OnEnable()
    {
        _playerHealth.OnDamage += OnPlayerDamaged;
        _playerHealth.PlayerDied += OnPlayerDied;
    }

    private void OnDisable()
    {
        _playerHealth.OnDamage -= OnPlayerDamaged;
        _playerHealth.PlayerDied -= OnPlayerDied;
    }

    private void Start()
    {
        ChangeState(PlayerStateType.Idle);
    }
    public void Update()
    {
        _stateMachine.Update();
    }
    public void SetMoveInput(Vector2 moveInput)
    {
        _moveInput = moveInput;

        if (_moveInput.sqrMagnitude > 0.01f)
        {
            ChangeState(PlayerStateType.Move);
        }
        else
        {
            ChangeState(PlayerStateType.Idle);
        }
    }
    //추가
    public void ReturnMoveOrIdleState()
    {
        if (IsRetired)
        {
            DebugTool.Log("리타이어 상태라 Move/Idle 복귀를 무시합니다.", DebugType.Character, this);
            return;
        }

        if (_moveInput.sqrMagnitude > 0.01f)
        {
            ChangeState(PlayerStateType.Move);
            return;
        }

        ChangeState(PlayerStateType.Idle);

    }

    public void ChangeState(PlayerStateType stateType)
    {
        if (_playerStateType == stateType)
        {
            return;
        }

        if(_playerStateType == PlayerStateType.Retire)
        {
            DebugTool.Log("죽어있어서 State가 변하지 않습니다..", DebugType.Character, this); return;
        }

        _playerStateType = stateType;

        switch (stateType)
        {
            case PlayerStateType.Idle:
                _stateMachine.ChangeState(_idleState);
                break;
            case PlayerStateType.Move:
                _stateMachine.ChangeState(_moveState);
                break;
            case PlayerStateType.Hit:
                _stateMachine.ChangeState(_hitState);
                break;
            case PlayerStateType.Retire:
                _stateMachine.ChangeState(_retireState);
                break;
        }
    }
    private void OnPlayerDamaged()
    {
        if(IsRetired)
        {
            return;
        }
        DebugTool.Log("플레이어가 데미지를 입었습니다.", DebugType.Character, this);
        ChangeState(PlayerStateType.Hit);
    }
    private void OnPlayerDied()
    {
        _moveInput = Vector2.zero;
        _combatStateMachine.SetAiming(false);
        ChangeState(PlayerStateType.Retire);
    }
}