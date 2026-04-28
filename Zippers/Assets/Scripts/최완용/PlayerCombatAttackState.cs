using UnityEngine;

public class PlayerCombatAttackState : IState
{
    private readonly PlayerCombatStateMachine _combatStateMachine;
    private readonly PlayerAnimation _playerAnimation;
    private readonly float _attackStateTime;

    private float _elapsedTime;

    public PlayerCombatAttackState(
        PlayerCombatStateMachine combatStateMachine,
        PlayerAnimation playerAnimation,
        float attackStateTime)
    {
        _combatStateMachine = combatStateMachine;
        _playerAnimation = playerAnimation;
        _attackStateTime = attackStateTime;
    }

    public void Enter()
    {
        _elapsedTime = 0f;

        if (_playerAnimation != null)
        {
            _playerAnimation.PlayAttack();
        }

        Debug.Log("[PlayerCombatAttackState] Attack 상태 진입");
    }

    public void Exit()
    {
        Debug.Log("[PlayerCombatAttackState] Attack 상태 종료");
    }

    public void UpdateState()
    {
        _elapsedTime += Time.deltaTime;

        if (_elapsedTime < _attackStateTime)
            return;

        _combatStateMachine.ReturnCombatState();
    }
}