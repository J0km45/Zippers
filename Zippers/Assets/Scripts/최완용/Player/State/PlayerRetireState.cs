using UnityEngine;

public class PlayerRetireState : IState
{
    private PlayerStateMachine _stateMachine;
    private PlayerMovement _playerMovement;
    private PlayerAnimation _playerAnimation;
    private BoxCollider _playerCollider;

    public PlayerRetireState(PlayerStateMachine stateMachine, PlayerMovement playerMovement, PlayerAnimation playerAnimation, BoxCollider playerCollider)
    {
        _stateMachine = stateMachine;
        _playerMovement = playerMovement;
        _playerAnimation = playerAnimation;
        _playerCollider = playerCollider;
    }
    public void Enter()
    {
        if(_playerMovement != null)
        {
            _playerMovement.SetMoveInput(Vector2.zero);
        }
        if(_playerAnimation != null)
        {
            _playerAnimation.SetIdle();
            _playerAnimation.PlayDie();
            _playerCollider.enabled = false;
        }

        DebugTool.Log("[PlayerRetireState] Retire 상태 진입",DebugType.Character,null);
    }

    public void Exit()
    {
        
    }

    public void UpdateState()
    {

    }
}
