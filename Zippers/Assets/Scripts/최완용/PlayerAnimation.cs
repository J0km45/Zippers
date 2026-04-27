using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    private static readonly int Speed = Animator.StringToHash("Speed");

    [SerializeField] private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void SetMoveDirection(Vector2 moveInput)
    {
        Vector2 normalizedInput = moveInput.normalized;
        _animator.SetFloat(MoveX, normalizedInput.x);
        _animator.SetFloat(MoveY, normalizedInput.y);
        _animator.SetFloat(Speed, normalizedInput.sqrMagnitude);
    }

    public void SetIdle()
    {
        _animator.SetFloat(MoveX, 0f);
        _animator.SetFloat(MoveY, 0f);
        _animator.SetFloat(Speed, 0f);
    }
}
