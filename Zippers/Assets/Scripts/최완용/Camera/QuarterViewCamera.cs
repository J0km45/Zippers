using UnityEngine;
using UnityEngine.InputSystem;

public class QuarterViewCamera : MonoBehaviour
{
    [Header("추적 대상")]
    [SerializeField] private Transform _target;

    [Header("카메라 위치 설정")]
    [SerializeField] private Vector3 _offset;
    [SerializeField] private float _followSpeed = 10f;

    [Header("장애물 감지 설정")]
    [SerializeField] private LayerMask _wallLayer;
    [SerializeField] private float _wallDistance = 0.8f;

    [Header("조준 카메라 설정")]
    [SerializeField] private PlayerAim _playerAim;
    [SerializeField] private float _aimCamera = 5f;

    private Camera _mainCamera;

    private void Awake()
    {
        if (_target != null && _playerAim == null)
        {
            _playerAim = _target.GetComponent<PlayerAim>();
        }
        _mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (_target == null)
            return;

        FollowTarget();
    }

    private void FollowTarget()
    {
        Vector3 delta = _offset;

        if (_playerAim != null && _playerAim.IsAiming)
        {
            delta += AimOffset();
        }

        Vector3 targetPosition = HandleWallCollision(delta);

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            _followSpeed * Time.deltaTime
        );
    }

    private Vector3 AimOffset()
    {

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = _mainCamera.ScreenPointToRay(mousePosition);

        Plane plane = new Plane(Vector3.up, _target.position);

        if (!plane.Raycast(ray, out float distance))
            return Vector3.zero;

        Vector3 mouseAimPosition = ray.GetPoint(distance);

        Vector3 aimDirection = mouseAimPosition - _target.position;
        aimDirection.y = 0f;

        return aimDirection.normalized * _aimCamera;
    }

    private Vector3 HandleWallCollision(Vector3 delta)
    {
        if (Physics.Raycast(
                _target.position,
                delta.normalized,
                out RaycastHit hit,
                delta.magnitude,
                _wallLayer))
        {
            float distanceToWall = (hit.point - _target.position).magnitude * _wallDistance;
            return _target.position + delta.normalized * distanceToWall;
        }

        return _target.position + delta;
    }
}