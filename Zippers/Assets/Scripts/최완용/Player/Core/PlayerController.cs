using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerActions _playerAction;

    private PlayerStateMachine _playerStateMachine;
    private PlayerCombatStateMachine _combatStateMachine;
    private PlayerCombat _playerCombat;
    private PlayerMovement _playerMovement;

    private void Awake()
    {
        _playerAction = new PlayerActions();
        _playerMovement = GetComponent<PlayerMovement>();
        _playerStateMachine = GetComponent<PlayerStateMachine>();
        _combatStateMachine = GetComponent<PlayerCombatStateMachine>();
        _playerCombat = GetComponent<PlayerCombat>();
    }

    private void OnEnable()
    {
        _playerAction.Enable();

        _playerAction.PlayerControl.Move.performed += OnMove;
        _playerAction.PlayerControl.Move.canceled += OnMove;

        _playerAction.PlayerControl.Fire.performed += OnAttack;
        _playerAction.PlayerControl.Fire.canceled += OnAttack;

        _playerAction.PlayerControl.Aiming.performed += OnAiming;
        _playerAction.PlayerControl.Aiming.canceled += OnAiming;

        _playerAction.PlayerControl.Reload.performed += OnReload;
        _playerAction.PlayerControl.Reload.canceled += OnReload;

        _playerAction.PlayerControl.Sprint.performed += OnSprint;
        _playerAction.PlayerControl.Sprint.canceled += OnSprint;
    }

    private void OnDisable()
    {
        _playerAction.PlayerControl.Move.performed -= OnMove;
        _playerAction.PlayerControl.Move.canceled -= OnMove;

        _playerAction.PlayerControl.Fire.performed -= OnAttack;
        _playerAction.PlayerControl.Fire.canceled -= OnAttack;

        _playerAction.PlayerControl.Aiming.performed -= OnAiming;
        _playerAction.PlayerControl.Aiming.canceled -= OnAiming;

        _playerAction.PlayerControl.Reload.performed -= OnReload;
        _playerAction.PlayerControl.Reload.canceled -= OnReload;

        _playerAction.PlayerControl.Sprint.performed -= OnSprint;
        _playerAction.PlayerControl.Sprint.canceled -= OnSprint;

        _playerAction.Disable();
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Vector2 moveInput = ctx.ReadValue<Vector2>();
            _playerStateMachine.SetMoveInput(moveInput);

            Debug.Log($"[PlayerController] 이동 입력: {moveInput}");
        }

        if (ctx.canceled)
        {
            _playerStateMachine.SetMoveInput(Vector2.zero);

            Debug.Log("[PlayerController] 이동 입력 취소");
        }
    }

    private void OnAttack(InputAction.CallbackContext ctx)
    {
        if (IsInputBlocked())
        {
            return;
        }

        if (!ctx.performed)
            return;

        if (_playerCombat == null)
        {
            Debug.LogWarning("[PlayerController] PlayerCombat이 없습니다.");
            return;
        }

        Debug.Log("[PlayerController] 공격 입력");
        _playerCombat.TryAttack();
    }

    private void OnAiming(InputAction.CallbackContext ctx)
    {
        if (IsInputBlocked())
        {
            return;
        }
        if (_combatStateMachine == null)
        {
            Debug.LogWarning("[PlayerController] PlayerCombatStateMachine이 없습니다.");
            return;
        }

        if (ctx.performed)
        {
            Debug.Log("[PlayerController] 조준 시작");
            _combatStateMachine.SetAiming(true);
        }
        else if (ctx.canceled)
        {
            Debug.Log("[PlayerController] 조준 종료");
            _combatStateMachine.SetAiming(false);
        }
    }

    private void OnReload(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        if(IsInputBlocked())
        {
            return;
        }

        if (_combatStateMachine == null)
        {
            Debug.LogWarning("[PlayerController] PlayerCombatStateMachine이 없습니다.");
            return;
        }

        Debug.Log("[PlayerController] 재장전 입력");
        _combatStateMachine.RequestReload();
    }
    private void OnSprint(InputAction.CallbackContext ctx)
    {
        if (IsInputBlocked())
        {
            _playerMovement.SetSprint(false);
            return;
        }
        if (ctx.performed)
        {
            _playerMovement.SetSprint(true);
        }
        else if (ctx.canceled)
        {
            _playerMovement.SetSprint(false);
        }
    }
    private bool IsInputBlocked()
    {
        if (_playerStateMachine == null)
        {
            DebugTool.MissingComponent(nameof(PlayerStateMachine), this);
            return true;
        }

        if (_playerStateMachine.IsRetired)
        {
            DebugTool.Log("리타이어 상태라 입력을 무시합니다.", DebugType.Character, this);
            return true;
        }

        return false;
    }
}