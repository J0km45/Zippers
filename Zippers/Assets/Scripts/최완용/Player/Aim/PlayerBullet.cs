using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [Header("충돌 설정")]
    [SerializeField] private LayerMask _targetLayerMask; // 데미지를 줄 대상 레이어

    private readonly HashSet<IDamagable> _hitTargets = new HashSet<IDamagable>(); // 이미 맞은 대상 저장

    private Vector3 _moveDirection; // 총알이 이동할 방향
    private Vector3 _startPosition; // 총알이 생성된 시작 위치

    private float _damage; // 총알 고유 데미지가 아니라, 발사 시 PlayerCombat에서 전달받은 데미지
    private float _speed; // 총알 이동 속도, PlayerClassDataSO의 BulletSpeed 사용
    private float _maxDistance; // 총알 최대 이동 거리, PlayerClassDataSO의 BulletDistance 사용
    private bool _canPierce; // 관통 가능 여부, Rifle/Pistol은 true

    private bool _isInitialized; // 초기화 완료 여부

    /// <summary>
    /// 투사체 초기화
    /// </summary>
    public void Initialize(Vector3 direction, float damage, float speed, float maxDistance, bool canPierce)
    {
        _moveDirection = direction.normalized;
        _damage = damage;
        _speed = speed;
        _maxDistance = maxDistance;
        _canPierce = canPierce;

        _startPosition = transform.position;
        _hitTargets.Clear();
        _isInitialized = true;

        Debug.Log($"[PlayerProjectile] 초기화 완료 / Damage: {_damage}, Speed: {_speed}, Distance: {_maxDistance}, Pierce: {_canPierce}");
    }

    private void Update()
    {
        if (!_isInitialized)
            return;

        Move();
        CheckDistance();
    }

    /// <summary>
    /// 투사체 이동
    /// </summary>
    private void Move()
    {
        transform.position += _moveDirection * _speed * Time.deltaTime;
    }

    /// <summary>
    /// 최대 사거리 체크
    /// </summary>
    private void CheckDistance()
    {
        float movedDistance = Vector3.Distance(_startPosition, transform.position);

        if (movedDistance < _maxDistance)
            return;

        DebugTool.Log("최대 사거리 도달", DebugType.Data, this);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsTargetLayer(other.gameObject))
            return;

        IDamagable damagable = other.GetComponentInParent<IDamagable>();

        if (damagable == null)
        {
            return;
        }

        if (_hitTargets.Contains(damagable))
        {
            return;
        }

        _hitTargets.Add(damagable);
        damagable.TakeDamage(_damage);

        if (!_canPierce)
        {
            Destroy(gameObject);
        }
    }
    /// <summary>
    /// 충돌 대상이 타겟 레이어인지 확인
    /// </summary>
    private bool IsTargetLayer(GameObject target)
    {
        int targetLayer = target.layer;
        return (_targetLayerMask.value & (1 << targetLayer)) != 0;
    }
}