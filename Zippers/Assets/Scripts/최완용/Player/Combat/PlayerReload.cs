using System;
using System.Collections;
using UnityEngine;

public class PlayerReload : MonoBehaviour
{
    public event Action OnReloadStarted;
    public event Action OnReloadCompleted;
    public event Action<float, float> OnAmmoChanged;

    private PlayerStats _playerStats;
    private Coroutine _reloadCoroutine;

    public float CurrentBullet { get; private set; }
    public float MaxBullet { get; private set; }
    public bool IsReloading { get; private set; }
    public bool UsesAmmo { get; private set; }

    private void Awake()
    {
        _playerStats = GetComponent<PlayerStats>();
    }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        if (_playerStats == null)
        {
            Debug.LogError("[PlayerReload] PlayerStats가 없습니다.");
            return;
        }

        UsesAmmo = _playerStats.UseBullet;

        if (!UsesAmmo)
        {
            MaxBullet = 0;
            CurrentBullet = 0;
            IsReloading = false;

            Debug.Log("[PlayerReload] 탄창을 사용하지 않는 클래스입니다.");
            return;
        }

        MaxBullet = _playerStats.TotalMagazineCapacity;
        CurrentBullet = MaxBullet;
        IsReloading = false;

        OnAmmoChanged?.Invoke(CurrentBullet, MaxBullet);
        Debug.Log($"[PlayerReload] 탄창 초기화 완료: {CurrentBullet}/{MaxBullet}");
    }
    //업그레이드 UI에서 연결
    public void RefreshMaxBullet()
    {
        if (!UsesAmmo)
        {
            return;
        }

        float beforeMaxBullet = MaxBullet;
        MaxBullet = _playerStats.TotalMagazineCapacity;

        if (MaxBullet > beforeMaxBullet)
        {
            Debug.Log($"[PlayerReload] 최대 탄창 증가: {beforeMaxBullet} -> {MaxBullet}");
        }

        CurrentBullet = Mathf.Min(CurrentBullet, MaxBullet);
        OnAmmoChanged?.Invoke(CurrentBullet, MaxBullet);
    }

    public bool TryUseAmmo()
    {
        if (!UsesAmmo)
        {
            Debug.Log("[PlayerReload] 탄창을 사용하지 않는 공격입니다.");
            return true;
        }

        if (IsReloading)
        {
            Debug.Log("[PlayerReload] 재장전 중이라 공격 불가");
            return false;
        }

        if (CurrentBullet <= 0)
        {
            Debug.Log("[PlayerReload] 남은 탄 수 없음");
            return false;
        }

        CurrentBullet--;
        OnAmmoChanged?.Invoke(CurrentBullet, MaxBullet);

        Debug.Log($"[PlayerReload] 탄 사용: {CurrentBullet}/{MaxBullet}");
        return true;
    }

    public bool StartReload()
    {
        if (!UsesAmmo)
        {
            Debug.Log("[PlayerReload] 탄창을 사용하지 않아 재장전 불필요");
            return false;
        }

        if (IsReloading)
        {
            Debug.Log("[PlayerReload] 이미 재장전 중");
            return false;
        }

        if (CurrentBullet >= MaxBullet)
        {
            Debug.Log("[PlayerReload] 탄창이 가득 차 있어 재장전 무시");
            return false;
        }

        _reloadCoroutine = StartCoroutine(ReloadRoutine());
        return true;
    }

    private IEnumerator ReloadRoutine()
    {
        IsReloading = true;
        OnReloadStarted?.Invoke();

        Debug.Log("[PlayerReload] 재장전 시작");

        yield return new WaitForSeconds(_playerStats.TotalReloadTime);

        CurrentBullet = MaxBullet;
        IsReloading = false;
        _reloadCoroutine = null;

        OnAmmoChanged?.Invoke(CurrentBullet, MaxBullet);
        OnReloadCompleted?.Invoke();

        Debug.Log($"[PlayerReload] 재장전 완료: {CurrentBullet}/{MaxBullet}");
    }

    public void CancelReload()
    {
        if (!IsReloading)
            return;

        if (_reloadCoroutine != null)
        {
            StopCoroutine(_reloadCoroutine);
            _reloadCoroutine = null;
        }

        IsReloading = false;

        Debug.Log("[PlayerReload] 재장전 취소");
    }
}