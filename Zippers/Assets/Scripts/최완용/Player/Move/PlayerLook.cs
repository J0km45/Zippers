using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{

    [SerializeField] private PlayerAim _playerAim;
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private PlayerAimCal _playerAimCal;

    [Header("회전 설정")]
    [SerializeField] private float _rotateSpeed = 20f;

    private void Awake()
    {
        _playerAim = GetComponent<PlayerAim>();
        _playerMovement = GetComponent<PlayerMovement>();
        _playerAimCal = GetComponent<PlayerAimCal>();
    }

    private void Update()
    {
        if (_playerAim.IsAiming)
        {
            LookAtMouse();
        }
        else
        {
            LookAtMoveDirection();
        }
    }

    // 우클릭 조준 중: 마우스 방향 바라보기
    private void LookAtMouse()
    {
        if (_playerAimCal == null)
            return;

        if (!_playerAimCal.TryGetAimPoint(out Vector3 aimPoint))
            return;

        Vector3 lookDirection = aimPoint - transform.position;
        lookDirection.y = 0f;

        Rotate(lookDirection);
    }


    // 우클릭 안 누름: 이동 방향 바라보기
    private void LookAtMoveDirection()
    {
        Vector3 moveDirection = _playerMovement.MoveDir;

        if (moveDirection.sqrMagnitude < 0.001f)
            return;

        Rotate(moveDirection);
    }

    //플레이어 회전 처리
    private void Rotate(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            _rotateSpeed * Time.deltaTime
        );
    }
}