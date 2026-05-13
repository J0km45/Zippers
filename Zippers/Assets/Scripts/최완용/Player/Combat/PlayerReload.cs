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
            DebugTool.Log($"[PlayerReload] 최대 탄창 증가: {beforeMaxBullet} -> {MaxBullet}", DebugType.Data,this);
        }

        CurrentBullet = Mathf.Min(CurrentBullet, MaxBullet);
        OnAmmoChanged?.Invoke(CurrentBullet, MaxBullet);
    }

    public bool TryUseAmmo()
    {
        //TODO : 탄약 차감은 클라이언트가 직접 처리하지 않고 서버 검증후 반영해야됨
        if (!UsesAmmo)
        {
            return true;
        }

        if (IsReloading)
        {
            return false;
        }

        if (CurrentBullet <= 0)
        {
            return false;
        }

        CurrentBullet--;
        OnAmmoChanged?.Invoke(CurrentBullet, MaxBullet);

        DebugTool.Log($"[PlayerReload] 탄 사용: {CurrentBullet}/{MaxBullet}", DebugType.Data,this);
        return true;
    }

    public bool StartReload()
    {
        //TODO : 재장전 시작은 서버가 현재탄약과 재장전 상태를 확인한 후 승인 해야됨
        if (!UsesAmmo)
        {
            return false;
        }

        if (IsReloading)
        {
            return false;
        }

        if (CurrentBullet >= MaxBullet)
        {
            return false;
        }

        _reloadCoroutine = StartCoroutine(ReloadRoutine());
        return true;
    }

    private IEnumerator ReloadRoutine()
    {
        //TODO : 재장전 완료 시간과 탄약 복구는 서버 기준으로 동기화해야됨
        IsReloading = true;
        OnReloadStarted?.Invoke();


        yield return new WaitForSeconds(_playerStats.TotalReloadTime);

        CurrentBullet = MaxBullet;
        IsReloading = false;
        _reloadCoroutine = null;

        //TODO : 탄약UI갱신 이벤트는 서버가 확정한 다음 값을 호출해야됨
        OnAmmoChanged?.Invoke(CurrentBullet, MaxBullet);
        OnReloadCompleted?.Invoke();

        DebugTool.Log($"[PlayerReload] 재장전 완료: {CurrentBullet}/{MaxBullet}", DebugType.Data,this);
    }
}