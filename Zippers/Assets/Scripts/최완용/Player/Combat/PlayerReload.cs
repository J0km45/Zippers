using System;
using System.Collections;
using UnityEngine;
using Zippers.Network.Contracts;

public class PlayerReload : MonoBehaviour
{
    public event Action OnReloadStarted;
    public event Action OnReloadCompleted;
    public event Action<float, float> OnAmmoChanged;

    private PlayerStats _playerStats;
    private IPlayerStatProvider _statProvider;
    private PlayerCombatNetState _combatNetState;

    private Coroutine _reloadCoroutine;

    public float CurrentBullet { get; private set; }
    public float MaxBullet { get; private set; }
    public bool IsReloading { get; private set; }
    public bool UsesAmmo { get; private set; }

    private void Awake()
    {
        _playerStats = GetComponent<PlayerStats>();
        _statProvider = GetComponent<IPlayerStatProvider>();
        _combatNetState = GetComponent<PlayerCombatNetState>();
    }

    private void OnEnable()
    {
        SubscribeCombatNetState();
    }
    private void OnDisable()
    {
        UnsubscribeCombatNetState();
    }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        if(!TryGetUseAmmo(out bool useAmmo))
        {
            DebugTool.Log("Player정보를 찾을 수없습니다.",DebugType.CombatNet, this);
            return;
        }

        UsesAmmo = useAmmo;

        if (!UsesAmmo)
        {
            MaxBullet = 0;
            CurrentBullet = 0;
            IsReloading = false;

            if (CanWriteCombatAmmoState())
            {
                _combatNetState.ServerInitializeAmmo(0f);
            }

            Debug.Log("[PlayerReload] 탄창을 사용하지 않는 클래스입니다.");
            return;
        }
        if (!TryGetMaxBullet(out float totalMaxBullet))
        {
            DebugTool.Log("Player최대 탄약 정보를 찾을 수없습니다.", DebugType.CombatNet, this);
            return;
        }

        if (CanUseCombatNetState())
        {
            if (CanWriteCombatAmmoState())
            {
                _combatNetState.ServerInitializeAmmo(totalMaxBullet);
            }

            if (CanReadCombatAmmoState())
            {
                SyncFromCombatNetState();
            }

            DebugTool.Log(
                $"[PlayerReload] 네트워크 탄창 초기화 연결: {CurrentBullet}/{MaxBullet}",
                DebugType.CombatNet,
                this
            );

            return;
        }

        MaxBullet = totalMaxBullet;
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

        if (!TryGetMaxBullet(out float totalMaxBullet))
        {
            Debug.LogError("[PlayerReload] 최대 탄창 정보를 찾을 수 없습니다.");
            return;
        }

        if (CanUseCombatNetState())
        {
            if (!CanWriteCombatAmmoState())
            {
                DebugTool.Log("[PlayerReload] 서버가 아니므로 최대 탄약 갱신을 무시합니다.", DebugType.CombatNet, this);
                return;
            }

            // 기존 기획 유지:
            // MaxBullet 증가 시 증가분만큼 CurrentBullet도 증가.
            _combatNetState.ServerSetMaxAmmo(totalMaxBullet);
            return;
        }

        float beforeMaxBullet = MaxBullet;
        MaxBullet = totalMaxBullet;

        float increaseBullet = MaxBullet - beforeMaxBullet;

        if (increaseBullet > 0f)
        {
            CurrentBullet += increaseBullet;

            DebugTool.Log(
                $"[PlayerReload] 최대 탄창 증가: {beforeMaxBullet} -> {MaxBullet}",
                DebugType.Data,
                this
            );
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

        if (CanUseCombatNetState())
        {
            if (!CanWriteCombatAmmoState())
            {
                DebugTool.Log("[PlayerReload] 서버가 아니므로 탄약 사용을 무시합니다.", DebugType.CombatNet, this);
                return false;
            }

            return _combatNetState.ServerTryConsumeAmmo(1f);
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

        if (_reloadCoroutine != null)
        {
            return false;
        }

        if (CanUseCombatNetState())
        {
            if (!CanWriteCombatAmmoState())
            {
                DebugTool.Log("[PlayerReload] 서버가 아니므로 재장전 시작을 무시합니다.", DebugType.CombatNet, this);
                return false;
            }

            if (_combatNetState.IsReloading)
            {
                return false;
            }

            if (_combatNetState.CurrentAmmo >= _combatNetState.MaxAmmo)
            {
                return false;
            }

            _combatNetState.ServerSetReloading(true);
            _reloadCoroutine = StartCoroutine(ReloadRoutine());
            return true;
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
        if (CanUseCombatNetState())
        {
            yield return new WaitForSeconds(GetReloadTime());

            if (CanWriteCombatAmmoState())
            {
                _combatNetState.ServerFillAmmo();
            }

            _reloadCoroutine = null;
            yield break;
        }

        IsReloading = true;
        OnReloadStarted?.Invoke();

        yield return new WaitForSeconds(GetReloadTime());

        CurrentBullet = MaxBullet;
        IsReloading = false;
        _reloadCoroutine = null;

        OnAmmoChanged?.Invoke(CurrentBullet, MaxBullet);
        OnReloadCompleted?.Invoke();

        DebugTool.Log($"[PlayerReload] 재장전 완료: {CurrentBullet}/{MaxBullet}", DebugType.Data, this);
    }
    //추가
    private void SubscribeCombatNetState()
    {
        if (_combatNetState == null)
        {
            return;
        }

        _combatNetState.OnAmmoChanged += HandleNetAmmoChanged;
        _combatNetState.OnReloadStateChanged += HandleNetReloadStateChanged;
    }

    private void UnsubscribeCombatNetState()
    {
        if (_combatNetState == null)
        {
            return;
        }

        _combatNetState.OnAmmoChanged -= HandleNetAmmoChanged;
        _combatNetState.OnReloadStateChanged -= HandleNetReloadStateChanged;
    }

    private void HandleNetAmmoChanged(float currentAmmo, float maxAmmo)
    {
        CurrentBullet = currentAmmo;
        MaxBullet = maxAmmo;

        OnAmmoChanged?.Invoke(CurrentBullet, MaxBullet);

        DebugTool.Log(
            $"[PlayerReload] 네트워크 탄약 반영: {CurrentBullet}/{MaxBullet}",
            DebugType.CombatNet,
            this
        );
    }

    private void HandleNetReloadStateChanged(bool isReloading)
    {
        bool previousReloading = IsReloading;
        IsReloading = isReloading;

        if (IsReloading && !previousReloading)
        {
            OnReloadStarted?.Invoke();

            DebugTool.Log("[PlayerReload] 네트워크 재장전 시작 반영", DebugType.CombatNet, this);
            return;
        }

        if (!IsReloading && previousReloading)
        {
            OnReloadCompleted?.Invoke();

            DebugTool.Log("[PlayerReload] 네트워크 재장전 완료 반영", DebugType.CombatNet, this);
        }
    }

    private void SyncFromCombatNetState()
    {
        if (!CanReadCombatAmmoState())
        {
            return;
        }

        CurrentBullet = _combatNetState.CurrentAmmo;
        MaxBullet = _combatNetState.MaxAmmo;
        IsReloading = _combatNetState.IsReloading;

        OnAmmoChanged?.Invoke(CurrentBullet, MaxBullet);
    }

    private bool CanUseCombatNetState()
    {
        return _combatNetState != null && _combatNetState.IsSpawned;
    }

    private bool CanReadCombatAmmoState()
    {
        return CanUseCombatNetState() && (_combatNetState.IsServer || _combatNetState.IsOwner);
    }

    private bool CanWriteCombatAmmoState()
    {
        return CanUseCombatNetState() && _combatNetState.IsServer;
    }

    private bool TryGetUseAmmo(out bool useAmmo)
    {
        if (_statProvider != null)
        {
            useAmmo = _statProvider.UseBullet;
            return true;
        }

        if (_playerStats != null)
        {
            useAmmo = _playerStats.UseBullet;
            return true;
        }

        useAmmo = false;
        return false;
    }

    private bool TryGetMaxBullet(out float maxBullet)
    {
        if (_statProvider != null)
        {
            maxBullet = _statProvider.TotalMagazineCapacity;
            return true;
        }

        if (_playerStats != null)
        {
            maxBullet = _playerStats.TotalMagazineCapacity;
            return true;
        }

        maxBullet = 0f;
        return false;
    }

    private float GetReloadTime()
    {
        if (_statProvider != null)
        {
            return _statProvider.TotalReloadTime;
        }

        if (_playerStats != null)
        {
            return _playerStats.TotalReloadTime;
        }

        return 0f;
    }
}