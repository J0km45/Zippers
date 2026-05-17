using UnityEngine;
using Unity.Netcode.Components;
using Unity.Netcode;

public class PlayerAnimation : NetworkBehaviour
{
    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int IsSprint = Animator.StringToHash("IsSprint");

    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int Reload = Animator.StringToHash("Reload");

    private static readonly int Hit = Animator.StringToHash("Hit");
    private static readonly int Die = Animator.StringToHash("Die");

    private const string AttackTrigger = "Attack";
    private const string ReloadTrigger = "Reload";
    private const string HitTrigger = "Hit";
    private const string DieTrigger = "Die";

    [SerializeField] private Animator _animator;
    [SerializeField] private NetworkAnimator _networkAnimator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _networkAnimator = GetComponent<NetworkAnimator>();
    }

    public void SetMoveDirection(Vector2 moveInput)
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening && !IsServer)
        {
            SetMoveDirectionServerRpc(moveInput);
            return;
        }

        Vector2 normalizedInput = moveInput.normalized;

        _animator.SetFloat(MoveX, normalizedInput.x);
        _animator.SetFloat(MoveY, normalizedInput.y);
        _animator.SetFloat(Speed, normalizedInput.sqrMagnitude);

        //DebugTool.Log($"{normalizedInput}", DebugType.Character, this);
    }

    [ServerRpc]
    private void SetMoveDirectionServerRpc(Vector2 moveInput)
    {
        SetMoveDirection(moveInput);
    }

    public void SetSprint(bool isSprinting)
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening && !IsServer)
        {
            SetSprintServerRpc(isSprinting);
            return;
        }

        _animator.SetBool(IsSprint, isSprinting);
    }

    [ServerRpc]
    private void SetSprintServerRpc(bool isSprinting)
    {
        SetSprint(isSprinting);
    }

    public void SetIdle()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening && !IsServer)
        {
            SetIdleServerRpc();
            return;
        }

        _animator.SetFloat(MoveX, 0f);
        _animator.SetFloat(MoveY, 0f);
        _animator.SetFloat(Speed, 0f);
        _animator.SetBool(IsSprint, false);
    }

    [ServerRpc]
    private void SetIdleServerRpc()
    {
        SetIdle();
    }

    public void PlayAttack()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening && !IsServer)
        {
            PlayAttackServerRpc();
            return;
        }

        if (_networkAnimator != null)
        {
            _networkAnimator.SetTrigger(AttackTrigger);
        }
        else
        {
            _animator.SetTrigger(Attack);
        }

        DebugTool.Log("Attack 애니메이션 실행", DebugType.Character, this);
    }

    [ServerRpc]
    private void PlayAttackServerRpc()
    {
        PlayAttack();
    }

    public void PlayReload()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening && !IsServer)
        {
            PlayReloadServerRpc();
            return;
        }

        PlayReloadLocal();
    }

    [ServerRpc]
    private void PlayReloadServerRpc()
    {
        PlayReload();
    }
    public void PlayDie()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening && !IsServer)
        {
            PlayDieServerRpc();
            return;
        }

        if (_networkAnimator != null)
        {
            _networkAnimator.SetTrigger(DieTrigger);
        }
        else
        {
            _animator.SetTrigger(Die);
        }

        DebugTool.Log("Die 애니메이션 실행", DebugType.Character, this);
    }

    [ServerRpc]
    private void PlayDieServerRpc()
    {
        PlayDie();
    }

    public void PlayerHit()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening && !IsServer)
        {
            PlayerHitServerRpc();
            return;
        }

        if (_networkAnimator != null)
        {
            _networkAnimator.SetTrigger(HitTrigger);
        }

        else
        {
            _animator.SetTrigger(Hit);
        }

        DebugTool.Log("Hit 애니메이션 실행", DebugType.Character, this);
    }

    [ServerRpc]
    private void PlayerHitServerRpc()
    {
        PlayerHit();
    }
    public void PlayReloadLocal()
    {
        if (_networkAnimator != null)
        {
            _networkAnimator.SetTrigger(ReloadTrigger);
        }
        else
        {
            _animator.SetTrigger(Reload);
        }

        DebugTool.Log("Reload 애니메이션 로컬 실행", DebugType.Character, this);
    }
}