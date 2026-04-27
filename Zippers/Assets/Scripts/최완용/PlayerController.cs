using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerActions _playerAction;
    private PlayerStateMachine _playerStateMachine;
    private PlayerAim _playerAim;

    private void Awake()
    {
        _playerAction = new PlayerActions();
        _playerAim = GetComponent<PlayerAim>();
        _playerStateMachine = GetComponent<PlayerStateMachine>();
    }

    private void OnEnable()
    {
        _playerAction.Enable();
        _playerAction.PlayerControl.Move.performed += OnMove;
        _playerAction.PlayerControl.Move.canceled += OnMove;

        _playerAction.PlayerControl.Fire.performed += OnFire;
        _playerAction.PlayerControl.Fire.canceled += OnFire;

        _playerAction.PlayerControl.Aiming.performed += OnAiming;
        _playerAction.PlayerControl.Aiming.canceled += OnAiming;

        _playerAction.PlayerControl.Reload.performed += OnReload;
        _playerAction.PlayerControl.Reload.canceled += OnReload;
    }

    private void OnDisable()
    {
        _playerAction.PlayerControl.Move.performed -= OnMove;
        _playerAction.PlayerControl.Move.canceled -= OnMove;

        _playerAction.PlayerControl.Fire.performed -= OnFire;
        _playerAction.PlayerControl.Fire.canceled -= OnFire;

        _playerAction.PlayerControl.Aiming.performed -= OnAiming;
        _playerAction.PlayerControl.Aiming.canceled -= OnAiming;

        _playerAction.PlayerControl.Reload.performed -= OnReload;
        _playerAction.PlayerControl.Reload.canceled -= OnReload;
        _playerAction.Disable();
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        if(ctx.performed )
        {
            Vector2 moveInput = ctx.ReadValue<Vector2>();
            _playerStateMachine.SetMoveInput(moveInput);
            // 이동 처리 로직
            Debug.Log($"[PlayerController] 이동 입력: {moveInput}");
        }
        if(ctx.canceled)
        {
            _playerStateMachine.SetMoveInput(Vector2.zero);
            // 이동 취소 처리 로직
            Debug.Log("[PlayerController] 이동 입력 취소");
        }
    }

    private void OnFire(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            // 발사 처리 로직
            Debug.Log("발싸!");
        }
        //TODO : PlayerShooter만드면 여기서 호출
    }

    private void OnAiming(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            _playerAim.SetAiming(true);
            Debug.Log("조준시작");
        }
        else if (ctx.canceled)
        {
            _playerAim.SetAiming(false);
            Debug.Log("조준 종료");
        }
    }

    private void OnReload(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            // 재장전 처리 로직
            Debug.Log("재장전!");
        }
        ////TODO : PlayerReload 여기서 호출
    }
}