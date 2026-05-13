using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// 쿼터뷰 카메라.
public class QuarterViewCamera : MonoBehaviour
{
    [Header("추적 대상")]
    [SerializeField] private Transform _target;

    [Header("카메라 위치 설정")]
    [SerializeField] private Vector3 _offset = new Vector3(0f, 10f, -8f);

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
    [Tooltip("0 = 플레이어 위치 ,1 = 에임위치 면 0.5 = AimOffset")]
    [Range(0f, 1f)]
    [SerializeField] private float _aimCameraOffset = 0.5f;

    [Tooltip("조준 오프셋이 부드럽게 이동하는 속도")]
    [SerializeField] private float _aimOffsetSmoothSpeed = 12f;

    [Header("디버그")]
    [SerializeField] private bool _showDebugLog = false;

    private Camera _mainCamera;
    private RaycastHit[] _hitBuffer;
    private float _detectTimer;
    private Vector3 _currentAimOffset;
    private PlayerStats _playerstats;

    // 현재 감지된 장애물
    private readonly HashSet<ObstacleFadeTarget> _currentDetectedTargets = new();

    // 이전 감지 장애물
    private readonly HashSet<ObstacleFadeTarget> _previousDetectedTargets = new();

    // Fade 업데이트 순회용 List
    private readonly List<ObstacleFadeTarget> _activeFadeTargets = new();

    // Fade 대상 중복 확인용 HashSet
    private readonly HashSet<ObstacleFadeTarget> _activeFadeTargetSet = new();

    // Collider 기준 ObstacleFadeTarget 캐싱
    private readonly Dictionary<Collider, ObstacleFadeTarget> _fadeTargetCache = new();

    /// <summary>
    /// 카메라가 추적할 대상 Transform. 런타임에 멀티플레이어 로컬 플레이어 바인딩용으로 외부에서 설정 가능.
    /// PlayerOwnershipGate (이수형) 가 IsOwner==true 시점에 호출.
    /// </summary>
    public Transform Target
    {
        get => _target;
        set
        {
            _target = value;

           if(_target ==null)
           {
                _playerstats = null;
                return;
           }
           if( _playerAim == null)
           {
                _playerAim = _target.GetComponent<PlayerAim>();
           }
           _playerstats = _target.GetComponent<PlayerStats>();
        }
    }

    /// <summary>
    /// 조준 카메라 오프셋 계산에 사용하는 PlayerAim. 런타임 멀티플레이어 바인딩용.
    /// Target 과 함께 설정되어야 정상 동작 (Target 만 바뀌고 PlayerAim 이 이전 플레이어를 가리키면 오작동 가능).
    /// </summary>
    public PlayerAim PlayerAim
    {
        get => _playerAim;
        set => _playerAim = value;
    }

    private void Awake()
    {
        _mainCamera = Camera.main;
        _hitBuffer = new RaycastHit[_maxHitCount];

       if(_target !=null)
       {
            if(_playerAim ==null)
            {
                _playerAim = _target.GetComponent<PlayerAim>();
            }
            _playerstats = _target.GetComponent<PlayerStats>();
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

    /// 플레이어를 즉시 따라가기
    private void FollowTarget()
    {
        Vector3 targetPosition = _target.position + _offset;
        Vector3 targetAimOffset = Vector3.zero;

        if (_playerAim != null && _playerAim.IsAiming)
        {
            targetAimOffset = CalculateAimOffset();
        }

        _currentAimOffset = Vector3.Lerp(
            _currentAimOffset,
            targetAimOffset,
            _aimOffsetSmoothSpeed * Time.deltaTime
        );

        transform.position = targetPosition + _currentAimOffset;
    }

    /// 조준 중 카메라가 이동할 방향을 계산한다.
    /// 조준 중 카메라가 플레이어와 에임 위치 사이로 이동할 오프셋을 계산한다.
    private Vector3 CalculateAimOffset()
    {
        if (Mouse.current == null)
        {
            return Vector3.zero;
        }
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Vector2 screenCenter = new Vector2
            (
                Screen.width * 0.5f,
                Screen.height * 0.5f
            );

        Vector2 screenDirection = mousePosition - screenCenter;

        if (screenDirection.sqrMagnitude < 0.01f)
        {
            return Vector3.zero;
        }

        screenDirection.x /= Screen.width * 0.5f;
        screenDirection.y /= Screen.height * 0.5f;

        screenDirection = Vector2.ClampMagnitude(screenDirection, 1f);

        Vector3 cameraRight = transform.right;
        cameraRight.y = 0f;

        Vector3 cameraForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);

        if (cameraRight.sqrMagnitude < 0.01f || cameraForward.sqrMagnitude < 0.01f)
            return Vector3.zero;

        cameraRight.Normalize();
        cameraForward.Normalize();

        Vector3 aimDirection =
            cameraRight * screenDirection.x +
            cameraForward * screenDirection.y;

        if (aimDirection.sqrMagnitude < 0.01f)
            return Vector3.zero;

        if(_playerstats == null)
        {
            return Vector3.zero;
        }

        float maxAimOffsetDistance = _playerstats.SightRange;

        Vector3 aimOffset = aimDirection * maxAimOffsetDistance * _aimCameraOffset;

        return OffsetCameraSightRange(aimOffset);
    }

    private Vector3 OffsetCameraSightRange(Vector3 aimOffset)
    {
        if(_playerstats ==null)
        {
            return aimOffset;
        }
        float sightRange = _playerstats.SightRange;

        if(sightRange <= 0f)
        {
            return Vector3.zero;
        }

        if(aimOffset.sqrMagnitude <= sightRange*sightRange)
        {
            return aimOffset;
        }

        return aimOffset.normalized * sightRange;
    }

    /// 카메라와 플레이어 사이의 장애물을 감지한다.
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

    /// 이전 감지 장애물 목록을 저장한다.
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

    /// 감지된 장애물을 투명화 대상으로 등록한다.
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

    /// 더 이상 감지되지 않는 장애물을 원래 상태로 복구한다.
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
                DebugTool.Log($"[QuarterViewCamera] 장애물 감지 해제: {previousTarget.name}", DebugType.Data, previousTarget);
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