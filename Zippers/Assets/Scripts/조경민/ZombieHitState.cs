using UnityEngine;

public class ZombieHitState : IState
{
    private ZombieController _zombie;
    private float _hitStartTime;

    public ZombieHitState(ZombieController zombie)
    {
        _zombie = zombie;
    }

    public void Enter()
    {
        // if (!IsServer) return;
        // TODO : NGO 적용되면 서버시간으로 변경
        _hitStartTime = Time.time;
        _zombie.Agent.isStopped = true;
        _zombie.Animator.SetTrigger("Hit");
        _zombie.Animator.SetBool("IsHit", true);
    }

    public void UpdateState()
    {
        // if (!IsServer) return;

        if (Time.time >= _hitStartTime + _zombie.StunDuration)
        {
            _zombie.ChangeState(_zombie.Chase);
        }
    }

    public void Exit()
    {
        _zombie.Agent.isStopped = false;
        _zombie.Animator.SetBool("IsHit", false);
    }
}
