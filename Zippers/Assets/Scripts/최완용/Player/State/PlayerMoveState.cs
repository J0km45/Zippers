using Audio;
using UnityEngine;

public class PlayerMoveState : IState
{
    private readonly PlayerStateMachine _stateMachine;
    private readonly PlayerMovement _playerMovement;
    private readonly PlayerAnimation _playerAnimation;
    private readonly PlayerSfxController _playerSfxController;

    //오디오
    private float _sfxTimer;
    private float _walkSfxInterval = 0.30f;
    private float _sprintSfxInterval = 0.25f;
    public PlayerMoveState(PlayerStateMachine stateMachine, PlayerMovement playerMovement, PlayerAnimation playerAnimation, PlayerSfxController playerSfxController)
    {
        _stateMachine = stateMachine;
        _playerMovement = playerMovement;
        _playerAnimation = playerAnimation;
        _playerSfxController = playerSfxController;
    }
    public void Enter()
    {
        _sfxTimer = 0f;
        //Debug.Log("[PlayerMoveState] Move 상태 진입");
    }
    public void Exit()
    {
        _playerMovement.SetMoveInput(Vector2.zero);
        _playerAnimation.SetIdle();
        _sfxTimer = 0f;
        //Debug.Log("[PlayerMoveState] Move 상태 퇴장");
    }
    public void UpdateState()
    {
        Vector2 moveInput = _stateMachine.MoveInput;
        _playerMovement.SetMoveInput(moveInput);
        _playerMovement.Move();
        //_playerAnimation.SetMoveDirection(moveInput);

        ///
        Vector3 localMoveDir = _playerMovement.transform.InverseTransformDirection(_playerMovement.MoveDir);
        Vector2 animationMoveInput = new Vector2(localMoveDir.x, localMoveDir.z);

        _playerAnimation.SetMoveDirection(animationMoveInput);
        ///

        _playerAnimation.SetSprint(_playerMovement.IsSprinting);

        MoveSfx(moveInput);
        //Debug.Log("[PlayerMoveState] Move 상태 업데이트");
    }
    private void MoveSfx(Vector2 moveInput)
    {
        _sfxTimer -= Time.deltaTime;

        if (_sfxTimer > 0f)
        {
            return;
        }
        if ( _playerMovement.IsSprinting )
        {
            _playerSfxController.PlaySprintSfx();
            _sfxTimer = _sprintSfxInterval;
            return;
        }

        _playerSfxController.PlayWalkingSfx();
        _sfxTimer = _walkSfxInterval;
    }
}
