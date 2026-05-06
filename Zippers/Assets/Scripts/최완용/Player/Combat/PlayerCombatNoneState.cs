using UnityEngine;

public class PlayerCombatNoneState : IState
{
    private PlayerCombatStateMachine _combatStateMachine;

    public PlayerCombatNoneState(PlayerCombatStateMachine combatStateMachine)
    {
        _combatStateMachine = combatStateMachine;
    }

    public void Enter()
    {
        Debug.Log("[PlayerCombatNoneState] None 상태 진입");
    }

    public void Exit()
    {
    }

    public void UpdateState()
    {
    }
}