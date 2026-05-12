public class ZombieChaseState : IState
{
    private ZombieController _zombie;

    public ZombieChaseState(ZombieController zombie)
    {
        _zombie = zombie;
    }

    public void Enter()
    {
        _zombie.Agent.isStopped = false;
        _zombie.Agent.speed = _zombie.MoveSpeed;
    }

    public void UpdateState()
    {
        float distance = _zombie.GetDistanceToPlayer();

        if (distance <= _zombie.AttackRange)
        {
            if (_zombie.Type == ZombieType.Ranged)
            {
                _zombie.Agent.isStopped = true;
                _zombie.Animator.SetFloat("MoveSpeed", 0f);
            }
            else
            {
                _zombie.Agent.isStopped = false;
            }

            if (_zombie.CanAttack())
            {
                _zombie.ChangeState(_zombie.Attack);
            }

            return;
        }

        _zombie.Agent.isStopped = false;
        _zombie.Agent.speed = distance <= _zombie.DetectRange ? _zombie.DetectMoveSpeed : _zombie.MoveSpeed;
        _zombie.Agent.SetDestination(_zombie.Player.position);
        _zombie.Animator.SetFloat("MoveSpeed", _zombie.Agent.velocity.magnitude);
    }

    public void Exit()
    {

    }
}
