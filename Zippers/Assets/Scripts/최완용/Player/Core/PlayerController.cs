using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    private PlayerActions _playerAction;

    private PlayerStateMachine _playerStateMachine;
    private PlayerCombatStateMachine _combatStateMachine;
    private PlayerCombat _playerCombat;
    private PlayerMovement _playerMovement;
    private PlayerStamina _playerStamina;
    private bool _isLocalInputEnabled;

    private void Awake()
    {
        _playerAction = new PlayerActions();
        _playerMovement = GetComponent<PlayerMovement>();
        _playerStateMachine = GetComponent<PlayerStateMachine>();
        _combatStateMachine = GetComponent<PlayerCombatStateMachine>();
        _playerCombat = GetComponent<PlayerCombat>();
        _playerStamina = GetComponent<PlayerStamina>();
    }

    private void Start()
    {
        // 네트워크 없이 싱글 테스트할 때는 기존처럼 입력을 사용한다.
        if (!IsNetworkGameRunning())
        {
            EnableLocalInput();
        }
    }

    public override void OnNetworkSpawn()
    {
        if(IsOwner || IsLocalPlayer)
        {
            EnableLocalInput();
            return;
        }
        DisableLocalInput();
    }

    public override void OnNetworkDespawn()
    {
        DisableLocalInput();
    }

    private void OnEnable()
    {
        //_playerAction.Enable();

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

        //_playerAction.Disable();
        DisableLocalInput();
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        if (!CanUseLocalInput())
        {
            return;
        }

        if (ctx.performed)
        {
            Vector2 moveInput = ctx.ReadValue<Vector2>();
            _playerStateMachine.SetMoveInput(moveInput);
        }

        if (ctx.canceled)
        {
            _playerStateMachine.SetMoveInput(Vector2.zero);
        }
    }

    private void OnAttack(InputAction.CallbackContext ctx)
    {
        if (!CanUseLocalInput())
        {
            return;

        }
        if (IsInputBlocked())
        {
            return;
        }

        if (!ctx.performed)
            return;

        if (_playerCombat == null)
        {
            return;
        }

        _playerCombat.TryAttack();
    }

    private void OnAiming(InputAction.CallbackContext ctx)
    {
        if (!CanUseLocalInput())
        {
            return;
        }

        if (IsInputBlocked())
        {
            return;
        }
        if (_combatStateMachine == null)
        {
            return;
        }

        if (ctx.performed)
        {
            //추가
            StopSprintForAiming();

            _combatStateMachine.SetAiming(true);
        }
        else if (ctx.canceled)
        {
            _combatStateMachine.SetAiming(false);
        }
    }

    private void OnReload(InputAction.CallbackContext ctx)
    {
        if (!CanUseLocalInput())
        {
            return;
        }
        if (!ctx.performed)
            return;

        if(IsInputBlocked())
        {
            return;
        }

        if (_playerCombat == null)
        {
            return;
        }

        _playerCombat.TryReload();
    }
    private void OnSprint(InputAction.CallbackContext ctx)
    {
        if (!CanUseLocalInput())
        {
            return;
        }

        if (IsInputBlocked())
        {
            _playerMovement.SetSprint(false);
            return;
        }
        if (ctx.performed)
        {
            if(_playerStamina ==null)
            {
                _playerMovement.SetSprint(false);
                return;
            }

            if(!_playerStamina.TryStartSprint())
            {
                _playerMovement.SetSprint(false);
                return;
            }

            //추가
            StopAimingForSprint();

            _playerMovement.SetSprint(true);
        }

        else if (ctx.canceled)
        {
            _playerMovement.SetSprint(false);
        }
    }

    //추가(조준중 달리기하면 조준 해제)
    private void StopAimingForSprint()
    {
        if(_combatStateMachine == null)
        {
            return;
        }
        if(!_combatStateMachine.IsAiming)
        {
            return;
        }

        _combatStateMachine.SetAiming(false);
    }
    //추가(달리기중 조준하면 조준 해제)
    private void StopSprintForAiming()
    {
        if(_playerMovement == null)
        {
            return;
        }
        if(!_playerMovement.IsSprinting)
        {
            return;
        }

        _playerMovement.SetSprint(false);
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

    //네트워크
    private void EnableLocalInput()
    {
        if (_playerAction == null || _isLocalInputEnabled)
        {
            return;
        }

        _playerAction.Enable();
        _isLocalInputEnabled = true;
    }

    private void DisableLocalInput()
    {
        if (_playerAction == null)
        {
            return;
        }

        _playerAction.Disable();
        _isLocalInputEnabled = false;
        ResetLocalInputState();
    }

    private void ResetLocalInputState()
    {
        if (_playerStateMachine != null)
        {
            _playerStateMachine.SetMoveInput(Vector2.zero);
        }

        if (_playerMovement != null)
        {
            _playerMovement.StopMove();
        }

        if (_combatStateMachine != null && _combatStateMachine.IsAiming)
        {
            _combatStateMachine.SetAiming(false);
        }
    }

    private bool CanUseLocalInput()
    {
        return _isLocalInputEnabled;
    }

    private bool IsNetworkGameRunning()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
    }
}