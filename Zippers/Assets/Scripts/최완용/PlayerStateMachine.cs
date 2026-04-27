using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    private StateMachine _stateMachine;

    private PlayerMovement _playerMovement;
    private PlayerStateType _playerStateType;
    private PlayerAnimation _playerAnimation;

    private PlayerIdleState _idleState;
    private PlayerMoveState _moveState;

    private Vector2 _moveInput;

    public Vector2 MoveInput => _moveInput;

    private void Awake()
    {
        _stateMachine = new StateMachine();

        _playerMovement = GetComponent<PlayerMovement>();
        _playerAnimation = GetComponent<PlayerAnimation>();

        _idleState = new PlayerIdleState(this, _playerMovement, _playerAnimation);
        _moveState = new PlayerMoveState(this, _playerMovement, _playerAnimation);
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

    public void ChangeState(PlayerStateType stateType)
    {
        if (_playerStateType == stateType)
            return;

        _playerStateType = stateType;
        switch (stateType)
        {
            case PlayerStateType.Idle:
                _stateMachine.ChangeState(_idleState);
                break;
            case PlayerStateType.Move:
                _stateMachine.ChangeState(_moveState);
                break;
        }
    }
}