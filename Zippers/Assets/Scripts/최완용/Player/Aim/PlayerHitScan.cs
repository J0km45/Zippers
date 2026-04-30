using System.Collections.Generic;
using UnityEngine;

public class PlayerHitScan : MonoBehaviour
{
    [Header("Layer")]
    [SerializeField] private LayerMask _targetLayer;

    [Header("샷건 설정")]
    [SerializeField] private Transform _shotgunFirePoint;
    [SerializeField] private float _shotgunRadius = 1.5f;

    [Header("근접 설정")]
    [SerializeField] private Transform _meleePoint;
    [SerializeField] private float _meleeRadius = 1.0f;

    //한번에 공격에 중복 타격 금지
    private HashSet<IDamagable> _hitTarget = new HashSet<IDamagable>();

    public void ShotGunHitScan(float damage, float shotgunDistance)
    {
        _hitTarget.Clear();
        Vector3 origin = _shotgunFirePoint.position;
        Vector3 direction = GetAttackDirection();

        RaycastHit[] hits = Physics.SphereCastAll
            (
                origin,
                _shotgunRadius,
                direction, 
                shotgunDistance,
                _targetLayer
            );

        foreach(RaycastHit hit in hits)
        {
            ApplyDamage(hit.collider, damage);
        }
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

    private void ApplyDamage(Collider targetCollider, float damage)
    {
        IDamagable damagable = targetCollider.GetComponentInParent<IDamagable>();

        _hitTarget.Add(damagable);
        damagable.TakeDamage(damage);
    }

    public Vector3 GetAttackDirection()
    {
        Vector3 direction = transform.forward;
        direction.y = 0f;

        return direction.normalized;
    }
    private void OnDrawGizmosSelected()
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

        Gizmos.DrawWireSphere(_shotgunFirePoint.position, _shotgunRadius);
        Gizmos.DrawLine(_shotgunFirePoint.position, _shotgunFirePoint.position + direction * 3f);
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
