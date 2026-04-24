using UnityEngine;

public class PlayerMoveState : IState
{
    private readonly PlayerStateMachine _stateMachine;
    private readonly PlayerMovement _playerMovement;
    private readonly PlayerAnimation _playerAnimation;
    public PlayerMoveState(PlayerStateMachine stateMachine, PlayerMovement playerMovement, PlayerAnimation playerAnimation)
    {
        _stateMachine = stateMachine;
        _playerMovement = playerMovement;
        _playerAnimation = playerAnimation;
    }
    public void Enter()
    {
        //Debug.Log("[PlayerMoveState] Move 상태 진입");
    }
    public void Exit()
    {
        _playerMovement.SetMoveInput(Vector2.zero);
        _playerAnimation.SetIdle();
        //Debug.Log("[PlayerMoveState] Move 상태 퇴장");
    }
    public void UpdateState()
    {
        Vector2 moveInput = _stateMachine.MoveInput;
        _playerMovement.SetMoveInput(moveInput);
        _playerMovement.Move();
        _playerAnimation.SetMoveDirection(moveInput);
        //Debug.Log("[PlayerMoveState] Move 상태 업데이트");
    }
}
