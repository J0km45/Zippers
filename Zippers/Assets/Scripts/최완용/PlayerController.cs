using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerInput _playerInput;
    private PlayerStateMachine _playerStateMachine;


    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _playerStateMachine = GetComponent<PlayerStateMachine>();
    }

    private void OnEnable()
    {
        _playerInput.onActionTriggered += HandleInput;
    }

    private void OnDisable()
    {
        _playerInput.onActionTriggered -= HandleInput;
    }

    private void HandleInput(InputAction.CallbackContext ctx)
    {
        switch(ctx.action.name)
        {
            case "Move":
                HandleMove(ctx);
                break;
            case "Fire":
                HandleFire(ctx);
                break;
            case "Aiming":
                HandleAiming(ctx);
                break;
            case "Reload":
                HandleReload(ctx);
                break;
        }
    }

    private void HandleMove(InputAction.CallbackContext ctx)
    {
        Vector2 moveInput = ctx.ReadValue<Vector2>();
        _playerStateMachine.SetMoveInput(moveInput);
        // 이동 처리 로직
        Debug.Log($"[PlayerController] 이동 입력: {moveInput}");
    }

    private void HandleFire(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            // 발사 처리 로직
            Debug.Log("Fire!");
        }
        //TODO : PlayerShooter만드면 여기서 호출
    }

    private void HandleAiming(InputAction.CallbackContext ctx)
    {

    }

    private void HandleReload(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            // 재장전 처리 로직
            Debug.Log("Reload!");
        }
        ////TODO : PlayerReload 여기서 호출
    }
}