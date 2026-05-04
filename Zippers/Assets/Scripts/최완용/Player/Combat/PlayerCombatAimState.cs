using UnityEngine;

public class PlayerCombatAimState : IState
{
    private PlayerCombatStateMachine _combatStateMachine;

    public PlayerCombatAimState(PlayerCombatStateMachine combatStateMachine)
    {
        _combatStateMachine = combatStateMachine;
    }

    public void Enter()
    {
        Debug.Log("[PlayerCombatAimState] Aim 상태 진입");
    }

    public void Exit()
    {
    }

    public void UpdateState()
    {
    }
}