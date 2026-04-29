using UnityEngine;

public class PlayerHitState : IState
{
    private PlayerStateMachine _stateMachine;
    private PlayerMovement _movement;
    private PlayerAnimation _animation;
    private float _hitDuration;

    private float _elapsedTime;

    public PlayerHitState(PlayerStateMachine stateMachine, PlayerMovement movement, PlayerAnimation animation, float hitDuration)
    {
        _stateMachine = stateMachine;
        _movement = movement;
        _animation = animation;
        _hitDuration = hitDuration;
    }
    public void Enter()
    {
        _elapsedTime = 0f;
        _animation.PlayerHit();
    }

    public void Exit()
    {
       
    }

    public void UpdateState()
    {
        _elapsedTime += Time.deltaTime;
        Vector2 moveInput = _stateMachine.MoveInput;
        _movement.Move();

        if(_elapsedTime < _hitDuration)
        {
            return;
        }
        ReturnMoveState();
    }
    public void ReturnMoveState()
    {
        if(_stateMachine.MoveInput.sqrMagnitude > 0.01f)
        {
            _stateMachine.ChangeState(PlayerStateType.Move);
            return;
        }
        else
        {
            _stateMachine.ChangeState(PlayerStateType.Idle);
        }
    }
}
