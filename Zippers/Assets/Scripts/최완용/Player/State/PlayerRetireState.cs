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
        if (_playerMovement != null)
        {
            _playerMovement.StopMove();
        }

        if (_playerAnimation != null)
        {
            _playerAnimation.SetIdleByNetwork();
            _playerAnimation.PlayDieByNetwork();
        }
        else
        {
            DebugTool.Log("[PlayerRetireState] PlayerAnimation이 없어 죽음 애니메이션 재생 불가", DebugType.Character, null);
        }

        // 죽은 뒤에도 바닥을 뚫지 않도록 Collider는 끄지 않는다.

        DebugTool.Log("[PlayerRetireState] Retire 상태 진입", DebugType.Character, null);
    }

    public void Exit()
    {
        
    }

    public void UpdateState()
    {

    }
}
