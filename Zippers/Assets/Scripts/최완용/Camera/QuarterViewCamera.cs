using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuarterViewCamera : MonoBehaviour
{
    [Header("추적 대상")]
    [SerializeField] private Transform _target;

    [Header("카메라 위치 설정")]
    [SerializeField] private Vector3 _offset = new Vector3(0f, 10f, -8f);
    [SerializeField] private float _followSpeed = 10f;

    [Header("장애물 감지 설정")]
    [SerializeField] private LayerMask _obstacleLayer;
    [SerializeField] private float _detectRadius = 0.3f;
    [SerializeField] private int _maxHitCount = 32;

    [Tooltip("장애물 감지 주기.")]
    [SerializeField] private float _detectInterval = 0.05f;

    [Header("장애물 투명화 설정")]
    [Range(0f, 1f)]
    [SerializeField] private float _fadeAlpha = 0.2f;

    [SerializeField] private float _fadeSpeed = 4f;

    [Header("조준 카메라 설정")]
    [SerializeField] private PlayerAim _playerAim;
    [SerializeField] private float _aimCameraOffset = 5f;

    [Header("디버그")]
    [SerializeField] private bool _showDebugLog = false;

    private Camera _mainCamera;
    private RaycastHit[] _hitBuffer;
    private float _detectTimer;

    // 현재 감지된 장애물
    private readonly HashSet<ObstacleFadeTarget> _currentDetectedTargets = new();

    // 이전 감지 장애물
    private readonly HashSet<ObstacleFadeTarget> _previousDetectedTargets = new();

    // Fade 업데이트 순회용 List
    private readonly List<ObstacleFadeTarget> _activeFadeTargets = new();

    // Fade 대상 중복 확인용 HashSet
    private readonly HashSet<ObstacleFadeTarget> _activeFadeTargetSet = new();

    private readonly Dictionary<Collider, ObstacleFadeTarget> _fadeTargetCache = new();

    private void Awake()
    {
        _mainCamera = Camera.main;
        _hitBuffer = new RaycastHit[_maxHitCount];

        if (_target != null && _playerAim == null)
        {
            _playerAim = _target.GetComponent<PlayerAim>();
        }

        _detectTimer = 0f;
    }

    private void LateUpdate()
    {
        if (_target == null)
            return;

        FollowTarget();

        _detectTimer -= Time.deltaTime;

        if (_detectTimer <= 0f)
        {
            DetectObstacles();
            _detectTimer = _detectInterval;
        }

        UpdateFadeTargets();
    }

    private void FollowTarget()
    {
        Vector3 targetPosition = _target.position + _offset;

        if (_playerAim != null && _playerAim.IsAiming)
        {
            targetPosition += CalculateAimOffset();
        }

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            _followSpeed * Time.deltaTime
        );
    }

    private Vector3 CalculateAimOffset()
    {
        if (_mainCamera == null || Mouse.current == null)
            return Vector3.zero;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = _mainCamera.ScreenPointToRay(mousePosition);

        Plane groundPlane = new Plane(Vector3.up, _target.position);

        if (!groundPlane.Raycast(ray, out float distance))
            return Vector3.zero;

        Vector3 mouseWorldPosition = ray.GetPoint(distance);

        Vector3 aimDirection = mouseWorldPosition - _target.position;
        aimDirection.y = 0f;

        if (aimDirection.sqrMagnitude < 0.01f)
            return Vector3.zero;

        return aimDirection.normalized * _aimCameraOffset;
    }

    private void DetectObstacles()
    {
        CachePreviousDetectedTargets();
        _currentDetectedTargets.Clear();

        Vector3 directionToTarget = _target.position - transform.position;
        float distanceToTarget = directionToTarget.magnitude;

        if (distanceToTarget <= 0.01f)
            return;

        int hitCount = Physics.SphereCastNonAlloc(
            transform.position,
            _detectRadius,
            directionToTarget.normalized,
            _hitBuffer,
            distanceToTarget,
            _obstacleLayer,
            QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < hitCount; i++)
        {
            Collider hitCollider = _hitBuffer[i].collider;

            if (hitCollider == null)
                continue;

            ObstacleFadeTarget fadeTarget = GetFadeTarget(hitCollider);

            if (fadeTarget == null)
                continue;

            DetectFadeTarget(fadeTarget);
        }

        RestoreUndetectedTargets();
    }

    private void CachePreviousDetectedTargets()
    {
        _previousDetectedTargets.Clear();

        foreach (ObstacleFadeTarget fadeTarget in _currentDetectedTargets)
        {
            if (fadeTarget == null)
                continue;

            _previousDetectedTargets.Add(fadeTarget);
        }
    }

    private ObstacleFadeTarget GetFadeTarget(Collider hitCollider)
    {
        if (_fadeTargetCache.TryGetValue(hitCollider, out ObstacleFadeTarget cachedTarget))
        {
            return cachedTarget;
        }

        ObstacleFadeTarget fadeTarget = hitCollider.GetComponentInParent<ObstacleFadeTarget>();

        _fadeTargetCache.Add(hitCollider, fadeTarget);

        return fadeTarget;
    }

    private void DetectFadeTarget(ObstacleFadeTarget fadeTarget)
    {
        _currentDetectedTargets.Add(fadeTarget);

        if (_activeFadeTargetSet.Add(fadeTarget))
        {
            _activeFadeTargets.Add(fadeTarget);
        }

        fadeTarget.SetFade(true, _fadeAlpha);

        if (_showDebugLog)
        {
            Debug.Log($"[QuarterViewCamera] 장애물 감지: {fadeTarget.name}", fadeTarget);
        }
    }

    private void RestoreUndetectedTargets()
    {
        foreach (ObstacleFadeTarget previousTarget in _previousDetectedTargets)
        {
            if (previousTarget == null)
                continue;

            if (_currentDetectedTargets.Contains(previousTarget))
                continue;

            previousTarget.SetFade(false, _fadeAlpha);

            if (_showDebugLog)
            {
                Debug.Log($"[QuarterViewCamera] 장애물 감지 해제: {previousTarget.name}", previousTarget);
            }
        }
    }

    private void UpdateFadeTargets()
    {
        for (int i = _activeFadeTargets.Count - 1; i >= 0; i--)
        {
            ObstacleFadeTarget fadeTarget = _activeFadeTargets[i];

            if (fadeTarget == null)
            {
                RemoveActiveFadeTargetAt(i, null);
                continue;
            }

            fadeTarget.UpdateFade(_fadeSpeed);

            if (fadeTarget.IsOpaque() && fadeTarget.IsFadeFinished())
            {
                RemoveActiveFadeTargetAt(i, fadeTarget);
            }
        }
    }

    private void RemoveActiveFadeTargetAt(int index, ObstacleFadeTarget fadeTarget)
    {
        if (fadeTarget != null)
        {
            _activeFadeTargetSet.Remove(fadeTarget);
        }

        _activeFadeTargets.RemoveAt(index);
    }
}