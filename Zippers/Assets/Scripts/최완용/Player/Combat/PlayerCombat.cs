using System;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public event Action OnAttackPerformed;

    private PlayerStats _playerStats;
    private PlayerReload _playerReload;

    private PlayerCombatStateMachine _combatStateMachine;
    private PlayerGun _playerGun;
    private PlayerHitScan _playerHitScan;

    private float _lastAttackTime = 0f;

    private void Awake()
    {
        _playerStats = GetComponent<PlayerStats>();
        _playerReload = GetComponent<PlayerReload>();
        _combatStateMachine = GetComponent<PlayerCombatStateMachine>();
        _playerGun = GetComponent<PlayerGun>();
        _playerHitScan = GetComponent<PlayerHitScan>();
    }

    public void TryAttack()
    {
        if (_playerStats == null)
        {
            Debug.LogError("[PlayerCombat] PlayerStats가 없습니다.");
            return;
        }

        if (_playerReload.IsReloading)
        {
            Debug.Log("[PlayerCombat] 재장전 중이라 공격 불가");
            return;
        }
        if (Time.time < _lastAttackTime + _playerStats.AttackSpeed)
        {
            Debug.Log("[PlayerCombat] 공격 속도 제한 중");
            return;
        }
        if(IsGunWeapon() && !_combatStateMachine.IsAiming)
        {
            return;
        }
        if (!_playerReload.TryUseAmmo())
        {
            Debug.Log("[PlayerCombat] 탄창 없음 - 자동 재장전 시도");
            _combatStateMachine.RequestReload();
            return;
        }

        _lastAttackTime = Time.time;
        float damage = _playerStats.GetRandomDamage();

        OnAttackPerformed?.Invoke();
        _combatStateMachine.RequestAttack();

        ExecuteAttack(damage);

        Debug.Log($"[PlayerCombat] 공격 성공 / Damage: {damage}");

        // TODO: 이후 실제 공격 판정 추가
        // 원거리: Raycast 또는 Projectile
        // 근접: 범위 판정
    }
    private void ExecuteAttack(float damage)
    {
        switch(_playerStats.WeaponType)
        {
            case WeaponType.Rifle:
            case WeaponType.Pistol:
                Projectile(damage);
                break;
            case WeaponType.Melee:
                MeleeHitScan(damage);
                break;
            case WeaponType.Shotgun:
                ShotGunHitScan(damage); 
                break;
        }
    }
    private void Projectile(float damage)
    {
        bool canPirece = _playerStats.WeaponType == WeaponType.Pistol || _playerStats.WeaponType == WeaponType.Rifle;

        _playerGun.Shoot
            (
                damage,
                _playerStats.BulletSpeed,
                _playerStats.BulletDistance,
                canPirece
            );
    }
    private void MeleeHitScan(float damage)
    {
        _playerHitScan.MeleeHitScan(damage);
    }

    private void ShotGunHitScan(float damage)
    {
        _playerHitScan.ShotGunHitScan
            (
                damage,
                _playerStats.BulletDistance
            );
    }
    private bool IsGunWeapon()
    {
        return _playerStats.WeaponType == WeaponType.Pistol ||
            _playerStats.WeaponType == WeaponType.Rifle ||
            _playerStats.WeaponType == WeaponType.Shotgun;

    }
}