using UnityEngine;

public class PlayerIdleState : IState
{
    private PlayerStateMachine _stateMachine;
    private PlayerMovement _playerMovement;
    private PlayerAnimation _playerAnimation;

    public PlayerIdleState(PlayerStateMachine stateMachine, PlayerMovement playerMovement, PlayerAnimation playerAnimation)
    {
        _stateMachine = stateMachine;
        _playerMovement = playerMovement;
        _playerAnimation = playerAnimation;
    }

    public void Enter()
    {
        _playerMovement.SetMoveInput(Vector2.zero);
        _playerAnimation.SetIdle();
        //Debug.Log("[PlayerIdleState] Idle 상태 진업");
    }

    public void Exit()
    {

        //Debug.Log("[PlayerIdleState] Idle 상태 퇴장");
    }

    public void UpdateState()
    {
    }
}
