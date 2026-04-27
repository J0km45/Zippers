using UnityEngine;

public class ZombieChaseState : IState
{
    private ZombieController _zombie;

    public ZombieChaseState(ZombieController zombie)
    {
        _zombie = zombie;
    }

    public void Enter()
    {
        _zombie.Agent.speed = _zombie.MoveSpeed;
    }

    public void UpdateState()
    {
        // TODO : 플레이어 위치 받아오는거 필요함
        //Transform player = 가장 가까운 생존 플레이어 위치

        Transform player = _zombie.Player; //임시(테스트용)
        float distanceToPlayer = Vector3.Distance(_zombie.transform.position, player.position);

        _zombie.Agent.speed = distanceToPlayer <= _zombie.DetectRange ? _zombie.DetectMoveSpeed : _zombie.MoveSpeed;

        _zombie.Agent.SetDestination(player.position);
        _zombie.Animator.SetFloat("MoveSpeed", _zombie.Agent.velocity.magnitude);
        // Debug.Log($"속도: {_zombie.Agent.speed}");
    }

    public void Exit()
    {
        
    }
}
