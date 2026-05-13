using UnityEngine;
using Unity.Netcode;

public class PlayerGun : MonoBehaviour
{
    [Header("투사체 설정")]
    [SerializeField] private PlayerBullet _playerBullet; // 생성할 총알 프리팹

    [Header("발사 위치")]
    [SerializeField] private Transform _firePoint; // 총알이 생성될 총구 위치

    [Header("총구 이펙트")]
    [SerializeField] private ParticleSystem _gunShotEffect; // 총을 쐈을 때 재생할 총구 연기 이펙트

    public void Shoot(float damage, float bulletSpeed, float bulletDistance, bool canPierce)
    {
        if (_playerBullet == null)
        {
            Debug.LogWarning("[PlayerGun] Bullet Prefab이 없습니다.");
            return;
        }

        if (_firePoint == null)
        {
            Debug.LogWarning("[PlayerGun] FirePoint가 없습니다.");
            return;
        }
        
        Vector3 shootDirection = GetShootDirection();

        if (shootDirection.sqrMagnitude < 0.001f)
        {
            Debug.LogWarning("[PlayerGun] 발사 방향을 계산하지 못했습니다.");
            return;
        }

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening && !NetworkManager.Singleton.IsServer)
        {
            return;
        }

        PlayerBullet bullet = Instantiate(
            _playerBullet,
            _firePoint.position,
            Quaternion.LookRotation(shootDirection, Vector3.up)
        );

        bullet.Initialize(
            shootDirection,
            damage,
            bulletSpeed,
            bulletDistance,
            canPierce
        );

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkObject networkObject = bullet.GetComponent<NetworkObject>();

            if (networkObject == null)
            {
                return;
            }

            networkObject.Spawn(true);
        }
        PlayGunShotEffect();

        DebugTool.Log($"[PlayerGun] 총알 발사 / Direction: {shootDirection}", DebugType.Data, this);
    }

    private Vector3 GetShootDirection()
    {
        Vector3 shootDirection = transform.forward;
        shootDirection.y = 0f;

        if (shootDirection.sqrMagnitude < 0.001f)
            return Vector3.forward;

        return shootDirection.normalized;
    }

    /// <summary>
    /// 총구 연기 이펙트 출력
    /// </summary>
    private void PlayGunShotEffect()
    {
        if (_gunShotEffect == null)
            return;

        _gunShotEffect.Play();
    }
}