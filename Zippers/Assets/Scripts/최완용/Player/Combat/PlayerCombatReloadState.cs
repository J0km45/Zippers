using Audio;
using UnityEngine;

public class PlayerCombatReloadState : IState
{
    private PlayerCombatStateMachine _combatStateMachine;
    private PlayerAnimation _playerAnimation;
    private PlayerReload _playerReload;
    private WeaponSFXController _weaponSFXController;

    public PlayerCombatReloadState(PlayerCombatStateMachine combatStateMachine,PlayerAnimation playerAnimation,PlayerReload playerReload, WeaponSFXController weaponSFXController)
    {
        _combatStateMachine = combatStateMachine;
        _playerAnimation = playerAnimation;
        _playerReload = playerReload;
        _weaponSFXController = weaponSFXController;
    }

    public void Enter()
    {
        if (_playerReload == null)
        {
            Debug.LogWarning("[PlayerCombatReloadState] PlayerReload가 없습니다.");
            _combatStateMachine.ReturnCombatState();
            return;
        }

        bool reloadStarted = _playerReload.StartReload();

        if (!reloadStarted)
        {
            _combatStateMachine.ReturnCombatState();
            return;
        }

        if (_playerAnimation != null)
        {
            _playerAnimation.PlayReload();
        }
        _weaponSFXController.PlayReloadSfx(WeaponType.Rifle);

        Debug.Log("[PlayerCombatReloadState] Reload 상태 진입");
    }

    public void Exit()
    {
        Debug.Log("[PlayerCombatReloadState] Reload 상태 종료");
    }

    public void UpdateState()
    {
    }
}