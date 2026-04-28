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
        //if (!_zombie.IsServer) return;

        if (_hasAttacked) return;
        _hasAttacked = true;

        switch (_zombie.Type)
        {
            case ZombieType.Normal:
                NormalAttack();
                break;
        }
    }

    private void NormalAttack()
    {
        Vector3 center = (_zombie.LeftHand.position + _zombie.RightHand.position) * 0.5f;
        Collider[] hits = Physics.OverlapSphere(center, _zombie.HandRadius, _zombie.PlayerLayer);

        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent(out IDamagable player))
            {
                float damage = Random.Range(_zombie.MinAttackDamage, _zombie.MaxAttackDamage);
                damage = Mathf.Round(damage * 10f) * 0.1f;
                player.TakeDamage(damage);
                break;
            }
        }
    }

    public void OnAttackEnd()
    {
        // if (!_zombie.IsServer) return;

        _zombie.SetAttackCooldown();
        _zombie.Animator.SetBool("IsAttacking", false);
        _zombie.ChangeState(_zombie.Chase);
    }

    public void Exit()
    {
        _zombie.Agent.isStopped = false;
    }
}
