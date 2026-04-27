using UnityEngine;

public class ZombieAnimationEventController : MonoBehaviour
{
    private ZombieController _zombie;

    private void Awake()
    {
        _zombie = GetComponentInParent<ZombieController>();
    }

    public void OnAttackHit()
    {
        _zombie.OnAttackHit();
    }

    public void OnAttackEnd()
    {
        _zombie.OnAttackEnd();
    }
}
