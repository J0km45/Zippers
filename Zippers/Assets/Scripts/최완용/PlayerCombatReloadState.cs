using UnityEngine;

public class PlayerCombatReloadState : IState
{
    private readonly PlayerCombatStateMachine _combatStateMachine;
    private readonly PlayerAnimation _playerAnimation;
    private readonly PlayerReload _playerReload;

    public PlayerCombatReloadState(
        PlayerCombatStateMachine combatStateMachine,
        PlayerAnimation playerAnimation,
        PlayerReload playerReload)
    {
        _combatStateMachine = combatStateMachine;
        _playerAnimation = playerAnimation;
        _playerReload = playerReload;
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