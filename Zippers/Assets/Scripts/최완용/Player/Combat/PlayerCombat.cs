using System;
using Unity.Netcode;
using UnityEngine;
using Zippers.Network.Contracts;
using Audio;
public class PlayerCombat : NetworkBehaviour
{
    public event Action OnAttackPerformed;

    private PlayerStats _playerStats;
    private IPlayerStatProvider _statProvider;

    private PlayerReload _playerReload;
    private PlayerCombatNetState _combatNetState;

    private PlayerCombatStateMachine _combatStateMachine;
    private PlayerGun _playerGun;
    private PlayerHitScan _playerHitScan;

    private PlayerAnimation _playerAnimation;
    private WeaponSFXController _weaponSFXController;

    private float _lastAttackTime = 0f;

    private void Awake()
    {
        _playerStats = GetComponent<PlayerStats>();
        _statProvider = GetComponent<IPlayerStatProvider>();

        _playerReload = GetComponent<PlayerReload>();
        _combatNetState = GetComponent<PlayerCombatNetState>();

        _combatStateMachine = GetComponent<PlayerCombatStateMachine>();
        _playerGun = GetComponent<PlayerGun>();
        _playerHitScan = GetComponent<PlayerHitScan>();

        _playerAnimation = GetComponent<PlayerAnimation>();
        _weaponSFXController = GetComponentInChildren<WeaponSFXController>();
    }

    public void TryAttack()
    {
        bool isAiming = _combatStateMachine != null && _combatStateMachine.IsAiming;

        if (!IsServer)
        {
            RequestAttackServerRpc(isAiming);
            return;
        }

        TryAttackServer(isAiming);
    }

    public void TryReload()
    {
        if (!IsServer)
        {
            RequestReloadServerRpc();
            return;
        }

        TryReloadServer();
    }

    [ServerRpc]
    private void RequestAttackServerRpc(bool isAiming, ServerRpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != OwnerClientId)
        {
            DebugTool.Log("[PlayerCombat] Owner가 아닌 클라이언트의 공격 요청 무시", DebugType.CombatNet, this);
            return;
        }

        TryAttackServer(isAiming);
    }

    [ServerRpc]
    private void RequestReloadServerRpc(ServerRpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != OwnerClientId)
        {
            DebugTool.Log("[PlayerCombat] Owner가 아닌 클라이언트의 재장전 요청 무시", DebugType.CombatNet, this);
            return;
        }

        TryReloadServer();
    }

    private void TryAttackServer(bool isAiming)
    {
        if (!IsServer)
        {
            return;
        }

        if (_playerReload == null)
        {
            DebugTool.Log("[PlayerCombat] PlayerReload가 없습니다.", DebugType.CombatNet, this);
            return;
        }

        if (!TryGetWeaponType(out WeaponType weaponType))
        {
            DebugTool.Log("[PlayerCombat] 무기 정보를 찾을 수 없습니다.", DebugType.CombatNet, this);
            return;
        }

        if (IsDead())
        {
            DebugTool.Log("[PlayerCombat] 사망 상태라 공격 불가", DebugType.CombatNet, this);
            return;
        }

        if (IsReloading())
        {
            DebugTool.Log("[PlayerCombat] 재장전 중이라 공격 불가", DebugType.CombatNet, this);
            return;
        }

        if (Time.time < _lastAttackTime + GetAttackSpeed())
        {
            DebugTool.Log("[PlayerCombat] 공격 속도 제한 중", DebugType.CombatNet, this);
            return;
        }

        if (IsGunWeapon() && !isAiming)
        {
            DebugTool.Log("[PlayerCombat] 총기 무기는 조준 중일 때만 공격 가능", DebugType.CombatNet, this);
            return;
        }

        if (!_playerReload.TryUseAmmo())
        {
            DebugTool.Log("[PlayerCombat] 탄창 없음 - 서버에서 재장전 시도", DebugType.CombatNet, this);
            TryReloadServer();
            return;
        }

        _lastAttackTime = Time.time;

        float damage = GetRandomDamage();

        ExecuteAttack(damage);
        PlayAttackClientRpc();

        DebugTool.Log($"[PlayerCombat] 서버 공격 성공 / Weapon: {weaponType}, Damage: {damage}", DebugType.CombatNet, this);
    }

    private void TryReloadServer()
    {
        if (!IsServer)
        {
            return;
        }

        if (_playerReload == null)
        {
            DebugTool.Log("[PlayerCombat] PlayerReload가 없습니다.", DebugType.CombatNet, this);
            return;
        }

        if (IsDead())
        {
            DebugTool.Log("[PlayerCombat] 사망 상태라 재장전 불가", DebugType.CombatNet, this);
            return;
        }

        bool reloadStarted = _playerReload.StartReload();

        if (!reloadStarted)
        {
            DebugTool.Log("[PlayerCombat] 재장전 시작 실패", DebugType.CombatNet, this);
            return;
        }

        PlayReloadClientRpc();

        DebugTool.Log("[PlayerCombat] 서버 재장전 시작 성공", DebugType.CombatNet, this);
    }

    [ClientRpc]
    private void PlayAttackClientRpc()
    {
        OnAttackPerformed?.Invoke();

        if (_weaponSFXController == null)
        {
            DebugTool.Log("[PlayerCombat] WeaponSFXController가 없어 공격 소리 재생 불가", DebugType.Audio, this);
            return;
        }

        if (!TryGetWeaponType(out WeaponType weaponType))
        {
            DebugTool.Log("[PlayerCombat] 무기 타입을 찾을 수 없어 공격 소리 재생 불가", DebugType.Audio, this);
            return;
        }

        _weaponSFXController.PlayWeaponSfx(weaponType);

        if (_combatStateMachine != null)
        {
            _combatStateMachine.RequestAttack();
        }
    }

    private void ExecuteAttack(float damage)
    {
        if (!TryGetWeaponType(out WeaponType weaponType))
        {
            DebugTool.Log("[PlayerCombat] 무기 정보를 찾을 수 없습니다.", DebugType.CombatNet, this);
            return;
        }

        switch (weaponType)
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
        if (_playerGun == null)
        {
            DebugTool.Log("[PlayerCombat] PlayerGun이 없습니다.", DebugType.CombatNet, this);
            return;
        }

        if (!TryGetWeaponType(out WeaponType weaponType))
        {
            DebugTool.Log("[PlayerCombat] 무기 정보를 찾을 수 없습니다.", DebugType.CombatNet, this);
            return;
        }

        bool canPierce = false;
            //weaponType == WeaponType.Pistol || weaponType == WeaponType.Rifle;

        _playerGun.Shoot(
            damage,
            GetBulletSpeed(),
            GetBulletDistance(),
            canPierce
        );
    }

    private void MeleeHitScan(float damage)
    {
        if (_playerHitScan == null)
        {
            DebugTool.Log("[PlayerCombat] PlayerHitScan이 없습니다.", DebugType.CombatNet, this);
            return;
        }

        _playerHitScan.MeleeHitScan(damage);
    }

    private void ShotGunHitScan(float damage)
    {
        if (_playerHitScan == null)
        {
            DebugTool.Log("[PlayerCombat] PlayerHitScan이 없습니다.", DebugType.CombatNet, this);
            return;
        }

        float bulletDistance = GetBulletDistance();

        _playerHitScan.ShotGunHitScan(
            damage,
            bulletDistance
        );

        PlayShotgunVisualClientRpc(bulletDistance);
    }
    [ClientRpc]
    private void PlayShotgunVisualClientRpc(float shotgunDistance)
    {
        if (_playerHitScan == null)
        {
            return;
        }

        _playerHitScan.ShowShotgunVisual(shotgunDistance);
    }

    private bool IsGunWeapon()
    {
        if (!TryGetWeaponType(out WeaponType weaponType))
        {
            return false;
        }

        return weaponType == WeaponType.Pistol ||
               weaponType == WeaponType.Rifle ||
               weaponType == WeaponType.Shotgun;
    }

    private bool TryGetWeaponType(out WeaponType weaponType)
    {
        if (_statProvider != null)
        {
            weaponType = _statProvider.WeaponType;
            return true;
        }

        if (_playerStats != null)
        {
            weaponType = _playerStats.WeaponType;
            return true;
        }

        weaponType = default;
        return false;
    }

    private float GetAttackSpeed()
    {
        if (_statProvider != null)
        {
            return _statProvider.TotalAttackSpeed;
        }

        if (_playerStats != null)
        {
            return _playerStats.TotalAttackSpeed;
        }

        return 0.1f;
    }

    private float GetRandomDamage()
    {
        if (_playerStats != null)
        {
            return _playerStats.GetRandomDamage();
        }

        if (_statProvider != null)
        {
            return UnityEngine.Random.Range(
                _statProvider.TotalMinDamage,
                _statProvider.TotalMaxDamage
            );
        }

        return 0f;
    }

    private float GetBulletSpeed()
    {
        if (_statProvider != null)
        {
            return _statProvider.TotalBulletSpeed;
        }

        if (_playerStats != null)
        {
            return _playerStats.TotalBulletSpeed;
        }

        return 0f;
    }

    private float GetBulletDistance()
    {
        if (_statProvider != null)
        {
            return _statProvider.TotalBulletDistance;
        }

        if (_playerStats != null)
        {
            return _playerStats.TotalBulletDistance;
        }

        return 0f;
    }

    private bool IsDead()
    {
        return _combatNetState != null && _combatNetState.IsDead;
    }

    private bool IsReloading()
    {
        if (_combatNetState != null)
        {
            return _combatNetState.IsReloading;
        }

        return _playerReload != null && _playerReload.IsReloading;
    }
    [ClientRpc]
    private void PlayReloadClientRpc()
    {
        // 재장전 애니메이션은 모든 클라이언트에서 보여야 함
        if (_playerAnimation != null)
        {
            _playerAnimation.PlayReloadLocal();
        }

        if (_weaponSFXController == null)
        {
            DebugTool.Log("[PlayerCombat] WeaponSFXController가 없어 재장전 소리 재생 불가", DebugType.Audio, this);
            return;
        }

        if (!TryGetWeaponType(out WeaponType weaponType))
        {
            DebugTool.Log("[PlayerCombat] 무기 타입을 찾을 수 없어 재장전 소리 재생 불가", DebugType.Audio, this);
            return;
        }

        _weaponSFXController.PlayReloadSfx(weaponType);

        DebugTool.Log(
            $"[PlayerCombat] 재장전 RPC 실행 / IsOwner: {IsOwner}",
            DebugType.CombatNet,
            this
        );
    }
}

/*
Unity 적용 방법
1. 기존 PlayerCombat.cs 전체를 이 코드로 교체한다.
2. 기존 함수명 TryAttack, ExecuteAttack, Projectile, MeleeHitScan, ShotGunHitScan, IsGunWeapon은 유지된다.
3. 플레이어 프리팹에 PlayerCombatNetState가 붙어 있는지 확인한다.
4. PlayerReload가 PlayerCombatNetState와 연결되어 있는지 확인한다.
5. 공격 입력은 기존처럼 PlayerCombat.TryAttack()을 호출하면 된다.
6. 수동 재장전 입력은 PlayerCombat.TryReload()를 호출하도록 PlayerController.OnReload를 수정한다.
7. 이 코드는 네트워크 전용 구조라서 오프라인 싱글 분기는 없다.
*/