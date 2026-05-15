using Audio;
using UnityEngine;
using Zippers.Network.Contracts;

public class PlayerCombatReloadState : IState
{
    private PlayerCombatStateMachine _combatStateMachine;
    private PlayerAnimation _playerAnimation;
    private PlayerReload _playerReload;
    private WeaponSFXController _weaponSFXController;
    private IPlayerStatProvider _statProvider;

    public PlayerCombatReloadState(PlayerCombatStateMachine combatStateMachine,PlayerAnimation playerAnimation,PlayerReload playerReload, WeaponSFXController weaponSFXController, IPlayerStatProvider statProvider)
    {
        _combatStateMachine = combatStateMachine;
        _playerAnimation = playerAnimation;
        _playerReload = playerReload;
        _weaponSFXController = weaponSFXController;
        _statProvider = statProvider;

    }

    public void Enter()
    {
        if (_playerReload == null)
        {
            _combatStateMachine.ReturnCombatState();
            return;
        }

        if (!_playerReload.IsReloading)
        {
            bool reloadStarted = _playerReload.StartReload();

            if (!reloadStarted)
            {
                _combatStateMachine.ReturnCombatState();
                return;
            }
        }

        if (_playerAnimation != null)
        {
            _playerAnimation.PlayReload();
        }
        _weaponSFXController.PlayReloadSfx(_statProvider.WeaponType);

    }

    public void Exit()
    {
        Debug.Log("[PlayerCombatReloadState] Reload 상태 종료");
    }

    public void UpdateState()
    {
    }
}