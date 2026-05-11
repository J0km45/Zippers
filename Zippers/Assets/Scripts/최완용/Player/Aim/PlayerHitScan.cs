using System.Collections.Generic;
using UnityEngine;

public class PlayerHitScan : MonoBehaviour
{
    [Header("Layer")]
    [SerializeField] private LayerMask _targetLayer;

    [Header("샷건 설정")]
    [SerializeField] private Transform _shotgunFirePoint;
    [SerializeField] private float _shotgunAngle = 45f;
    [SerializeField] private int _shotgunRayCount = 4;

    [Header("근접 설정")]
    [SerializeField] private Transform _meleePoint;
    [SerializeField] private float _meleeRadius = 1.0f;

    //한번에 공격에 중복 타격 금지
    private HashSet<IDamagable> _hitTarget = new HashSet<IDamagable>();

    public void ShotGunHitScan(float damage, float shotgunDistance)
    {
        if (_shotgunFirePoint == null)
        {
            Debug.LogWarning("[PlayerHitScan] Shotgun Fire Point가 없습니다.");
            return;
        }

        if (_shotgunRayCount <= 0)
        {
            Debug.LogWarning("[PlayerHitScan] Shotgun Ray Count가 0 이하입니다.");
            return;
        }

        _hitTarget.Clear();

        Vector3 origin = _shotgunFirePoint.position;
        Vector3 centerDirection = GetAttackDirection();

        float halfAngle = _shotgunAngle * 0.5f;

        for (int i = 0; i < _shotgunRayCount; i++)
        {
            float angle;

            if (_shotgunRayCount == 1)
            {
                angle = 0f;
            }
            else
            {
                float t = i / (float)(_shotgunRayCount - 1);
                angle = Mathf.Lerp(-halfAngle, halfAngle, t);
            }

            Vector3 rayDirection = Quaternion.AngleAxis(angle, Vector3.up) * centerDirection;
            rayDirection.y = 0f;
            rayDirection.Normalize();

            if (Physics.Raycast(origin, rayDirection, out RaycastHit hit, shotgunDistance, _targetLayer))
            {
                ApplyDamage(hit.collider, damage);

                // 빨간색 = 적중한 Ray
                Debug.DrawRay(origin, rayDirection * hit.distance, Color.red, 0.3f);
            }
            else
            {
                // 노란색 = 빗나간 Ray
                Debug.DrawRay(origin, rayDirection * shotgunDistance, Color.yellow, 0.3f);
            }
        }

        Debug.Log($"[PlayerHitScan] 샷건 발사 / RayCount: {_shotgunRayCount}, Distance: {shotgunDistance}");
    }
    public void MeleeHitScan(float damage)
    {
        if (_meleePoint == null)
        {
            Debug.LogWarning("[PlayerHitScan] Melee Point가 없습니다.");
            return;
        }

        _hitTarget.Clear();

        Collider[] hits = Physics.OverlapSphere(
            _meleePoint.position,
            _meleeRadius,
            _targetLayer
        );

        foreach (Collider hit in hits)
        {
            ApplyDamage(hit, damage);
        }

        Debug.Log($"[PlayerHitScan] 근접 판정 완료 / Radius: {_meleeRadius}, HitCount: {hits.Length}");
    }
    private bool IsInsideShotgunCone(Vector3 origin, Vector3 attackDirection, Vector3 targetPosition)
    {
        Vector3 directionToTarget = targetPosition - origin;
        directionToTarget.y = 0f;

        if (directionToTarget.sqrMagnitude < 0.001f)
            return true;

        float halfAngle = _shotgunAngle * 0.5f;
        float targetAngle = Vector3.Angle(attackDirection, directionToTarget.normalized);

        return targetAngle <= halfAngle;
    }

    private void ApplyDamage(Collider targetCollider, float damage)
    {
        IDamagable damagable = targetCollider.GetComponentInParent<IDamagable>();

        if(_hitTarget.Contains(damagable))
        {
            return;
        }
        _hitTarget.Add(damagable);
        damagable.TakeDamage(damage);
    }

    public Vector3 GetAttackDirection()
    {
        Vector3 direction = transform.forward;
        direction.y = 0f;

        return direction.normalized;
    }
    private void OnDrawGizmos()
    {
        DrawShotgunGizmo();
        DrawMeleeGizmo();
    }


    private void DrawShotgunGizmo()
    {
        if (_shotgunFirePoint == null)
            return;

        Vector3 direction = transform.forward;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            direction = Vector3.forward;

        direction.Normalize();

        float gizmoDistance = 3f;
        float halfAngle = _shotgunAngle * 0.5f;

        Vector3 leftDirection = Quaternion.AngleAxis(-halfAngle, Vector3.up) * direction;
        Vector3 rightDirection = Quaternion.AngleAxis(halfAngle, Vector3.up) * direction;

        // 샷건 원뿔 기즈모 색상
        Gizmos.color = Color.blue;

        Vector3 origin = _shotgunFirePoint.position;

        Gizmos.DrawLine(origin, origin + direction * gizmoDistance);
        Gizmos.DrawLine(origin, origin + leftDirection * gizmoDistance);
        Gizmos.DrawLine(origin, origin + rightDirection * gizmoDistance);
    }

    /// <summary>
    /// 근접 판정 범위 시각화
    /// </summary>
    private void DrawMeleeGizmo()
    {
        if (_meleePoint == null)
            return;

        Gizmos.DrawWireSphere(_meleePoint.position, _meleeRadius);
    }
}
