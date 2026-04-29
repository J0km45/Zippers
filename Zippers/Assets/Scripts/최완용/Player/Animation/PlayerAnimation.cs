using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int IsSprint = Animator.StringToHash("IsSprint");

    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int Reload = Animator.StringToHash("Reload");
    private static readonly int Hit = Animator.StringToHash("Hit");
    private static readonly int Die = Animator.StringToHash("Die");

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

    public void SetSprint(bool isSprinting)
    {
        _animator.SetBool(IsSprint, isSprinting);
    }

    public void SetIdle()
    {
        _animator.SetFloat(MoveX, 0f);
        _animator.SetFloat(MoveY, 0f);
        _animator.SetFloat(Speed, 0f);
        _animator.SetBool(IsSprint, false);
    }

    public void PlayAttack()
    {
        _animator.SetTrigger(Attack);
        DebugTool.Log("Attack 애니메이션 실행", DebugType.Character, this);
    }

    public void PlayReload()
    {
        _animator.SetTrigger(Reload);
        DebugTool.Log("Reload 애니메이션 실행", DebugType.Character, this);
    }
    public void PlayDie()
    {
        _animator.SetTrigger(Die);
        DebugTool.Log("Die 애니메이션 실행", DebugType.Character, this);
    }
    public void PlayerHit()
    {
        _animator.SetTrigger(Hit);
        DebugTool.Log("Hit 애니메이션 실행", DebugType.Character, this);
    }
}