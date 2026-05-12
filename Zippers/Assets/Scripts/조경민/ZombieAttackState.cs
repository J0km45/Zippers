using UnityEngine;

public class ZombieAttackState : IState
{
    private ZombieController _zombie;
    private bool _hasAttacked;

    public ZombieAttackState(ZombieController zombie)
    {
        _zombie = zombie;
    }
    
    public void Enter()
    {
        _hasAttacked = false;
        _zombie.Agent.isStopped = true;

        Vector3 directionToPlayer = _zombie.Player.position - _zombie.transform.position;
        _zombie.transform.rotation = Quaternion.LookRotation(directionToPlayer);

        _zombie.Animator.SetBool("IsAttacking", true);
        _zombie.Animator.SetTrigger("Attack");
    }

    public void UpdateState()
    {

    }

    public void OnAttackHit()
    {
        if (_hasAttacked) return;
        _hasAttacked = true;

        _zombie.ZombieAttack.Attack(_zombie);
    }

    public void OnAttackEnd()
    {
        _zombie.SetAttackCooldown();
        _zombie.Animator.SetBool("IsAttacking", false);
        _zombie.ChangeState(_zombie.Chase);
    }

    public void Exit()
    {
        _zombie.Agent.isStopped = false;
    }
}
