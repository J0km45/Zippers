using Audio;
using UnityEngine;

public class PlayerHitState : IState
{
    private PlayerStateMachine _stateMachine;
    private PlayerMovement _movement;
    private PlayerAnimation _animation;
    private PlayerSfxController _playerSfxController;
    private float _hitDuration;

    private float _elapsedTime;

    public PlayerHitState(PlayerStateMachine stateMachine, PlayerMovement movement, PlayerAnimation animation, float hitDuration, PlayerSfxController playerSfxController)
    {
        _stateMachine = stateMachine;
        _movement = movement;
        _animation = animation;
        _hitDuration = hitDuration;
        _playerSfxController = playerSfxController;
    }
    public void Enter()
    {
        _elapsedTime = 0f;
        _animation.PlayerHit();
        _playerSfxController.PlayMaleHitSfx();
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
        _stateMachine.ReturnMoveOrIdleState();
    }
    //public void ReturnMoveState()
    //{
    //    if(_stateMachine.MoveInput.sqrMagnitude > 0.01f)
    //    {
    //        _stateMachine.ChangeState(PlayerStateType.Move);
    //        return;
    //    }
    //    else
    //    {
    //        _stateMachine.ChangeState(PlayerStateType.Idle);
    //    }
    //}
}
