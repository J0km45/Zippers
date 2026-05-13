using Audio;
using UnityEngine;

public class PlayerCombatAttackState : IState
{
    private PlayerCombatStateMachine _combatStateMachine;
    private PlayerAnimation _playerAnimation;
    private WeaponSFXController _weaponSFXController;
    private PlayerStats _playerStats;
    private float _attackStateTime;

    private float _elapsedTime;

    public PlayerCombatAttackState(PlayerCombatStateMachine combatStateMachine, PlayerAnimation playerAnimation, float attackStateTime, WeaponSFXController weaponSFXController, PlayerStats playerStats)
    {
        _combatStateMachine = combatStateMachine;
        _playerAnimation = playerAnimation;
        _attackStateTime = attackStateTime;
        _weaponSFXController = weaponSFXController;
        _playerStats = playerStats;
    }

    public void Enter()
    {
        _elapsedTime = 0f;

        if (_playerAnimation != null)
        {
            _playerAnimation.PlayAttack();

            PlayAttackSfx();

            //if (_playerStats.WeaponType == WeaponType.Rifle)
            //{
            //    _weaponSFXController.PlayWeaponSfx(WeaponType.Rifle);
            //}
            //else if (_playerStats.WeaponType == WeaponType.Melee)
            //{
            //    _weaponSFXController.PlayWeaponSfx(WeaponType.Melee);   
            //}
            //else if( _playerStats.WeaponType == WeaponType.Pistol)
            //{
            //    _weaponSFXController.PlayWeaponSfx(WeaponType.Pistol);
            //}
            //else
            //{
            //    _weaponSFXController.PlayWeaponSfx(WeaponType.Shotgun);
            //}
        }
    }

    public void Exit()
    {
        DebugTool.Log("[PlayerCombatAttackState] Attack 상태 종료", DebugType.Data);
    }

    public void UpdateState()
    {
        _elapsedTime += Time.deltaTime;

        if (_elapsedTime < _attackStateTime)
            return;

        _combatStateMachine.ReturnCombatState();
    }
    private void PlayAttackSfx()
    {
        if (_weaponSFXController == null)
        {
            return;
        }

        if (_playerStats == null)
        {
            return;
        }

        _weaponSFXController.PlayWeaponSfx(_playerStats.WeaponType);
    }
}