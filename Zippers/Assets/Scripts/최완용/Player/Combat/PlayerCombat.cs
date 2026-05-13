using System;
using UnityEngine;
using Unity.Netcode;

public class PlayerCombat : NetworkBehaviour
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
        //TODO : 공격 요청은 서버에서 공격 쿨타임 , 탄약, 재장전 ,사망상태를 검증해야됨
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
        //TODO : 서버 에서 검증해야됨
        if (Time.time < _lastAttackTime + _playerStats.TotalAttackSpeed)
        {
            DebugTool.Log("[PlayerCombat] 공격 속도 제한 중", DebugType.Data, this);
            return;
        }
        if(IsGunWeapon() && !_combatStateMachine.IsAiming)
        {
            return;
        }
        if (!_playerReload.TryUseAmmo())
        {
            DebugTool.Log("[PlayerCombat] 탄창 없음 - 자동 재장전 시도", DebugType.Data, this);
            _combatStateMachine.RequestReload();
            return;
        }

        _lastAttackTime = Time.time;
        //TODO : 데미지 계산은 모든 업그레이드가 반영된 서버 스텟기준으로 해야됨
        float damage = _playerStats.GetRandomDamage();

        OnAttackPerformed?.Invoke();
        _combatStateMachine.RequestAttack();

        ExecuteAttack(damage);

        DebugTool.Log($"[PlayerCombat] 공격 성공 / Damage: {damage}", DebugType.Data, this);

        // TODO: 이후 실제 공격 판정 추가
        // 원거리: Raycast 또는 Projectile
        // 근접: 범위 판정
    }
    private void ExecuteAttack(float damage)
    {
        //TODO : 실제 공격 실행은 서버 승인 후 클라이언트가 애니매이션/사운드/이펙트를 재생하는 구조로 변경
        switch (_playerStats.WeaponType)
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
                _playerStats.TotalBulletSpeed,
                _playerStats.TotalBulletDistance,
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
                _playerStats.TotalBulletDistance
            );
    }
    private bool IsGunWeapon()
    {
        return _playerStats.WeaponType == WeaponType.Pistol ||
            _playerStats.WeaponType == WeaponType.Rifle ||
            _playerStats.WeaponType == WeaponType.Shotgun;

    }
}