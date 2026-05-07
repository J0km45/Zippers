using UnityEngine;

public class ZombieDieState : IState
{
    private ZombieController _zombie;

    public ZombieDieState(ZombieController zombie)
    {
        _zombie = zombie;
    }

    public void Enter()
    {
        if (_zombie.Agent != null && _zombie.Agent.enabled)
        {
            _zombie.Agent.isStopped = true;
            _zombie.Agent.enabled = false;
        }

        if (_zombie.TryGetComponent(out Collider collider))
        {
            collider.enabled = false;
        }

        _zombie.Animator.SetTrigger("Die");
    }

    public void UpdateState()
    {
        AnimatorStateInfo stateInfo = _zombie.Animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.normalizedTime >= 1.0f)
        {
            _zombie.SpawnReward();
            PoolManager.Instance.Release(_zombie.gameObject);
        }
    }

    public void Exit()
    {

    }
}
