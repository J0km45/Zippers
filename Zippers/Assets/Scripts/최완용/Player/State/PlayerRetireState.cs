using Audio;
using UnityEngine;

public class PlayerRetireState : IState
{
    private PlayerStateMachine _stateMachine;
    private PlayerMovement _playerMovement;
    private PlayerAnimation _playerAnimation;
    private BoxCollider _playerCollider;
    private PlayerSfxController _playerSfxController;

    public PlayerRetireState(PlayerStateMachine stateMachine, PlayerMovement playerMovement, PlayerAnimation playerAnimation, BoxCollider playerCollider, PlayerSfxController playerSfxController)
    {
        _stateMachine = stateMachine;
        _playerMovement = playerMovement;
        _playerAnimation = playerAnimation;
        _playerCollider = playerCollider;
        _playerSfxController = playerSfxController;
    }
    public void Enter()
    {
        if(_playerMovement != null)
        {
            _playerMovement.StopMove();
        }
        if(_playerAnimation != null)
        {
            _playerAnimation.SetIdle();
            _playerAnimation.PlayDie();
            _playerCollider.enabled = false;
            _playerSfxController.PlayMaleDeathSfx();
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
