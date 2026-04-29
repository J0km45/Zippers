using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private PlayerAim _playerAim;
    [SerializeField] private PlayerMovement _playerMovement;

    [Header("회전 설정")]
    [SerializeField] private float _rotateSpeed = 20f;

    private void Awake()
    {
        _playerAim = GetComponent<PlayerAim>();
        _playerMovement = GetComponent<PlayerMovement>();

        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }
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
        if (_mainCamera == null)
            return;

        if (Mouse.current == null)
            return;

        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        Ray ray = _mainCamera.ScreenPointToRay(mouseScreenPosition);

        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (!groundPlane.Raycast(ray, out float distance))
            return;

        Vector3 mouseWorldPosition = ray.GetPoint(distance);

        Vector3 lookDirection = mouseWorldPosition - transform.position;
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