using System;
using Unity.Netcode;
using UnityEngine;
using Zippers.Network;

/// <summary>
/// [MIGRATION · Step 3] 호환 래퍼.
///
/// 원래 이 컴포넌트가 자체적으로 _upgradeLevel Dictionary 를 들고 개인 업그레이드를 관리했지만,
/// 멀티 전환을 위해 데이터를 PlayerEconomyNetState 의 NetworkList&lt;UpgradeLevelEntry&gt; 로 옮겼다.
///
/// 외부 시그니처는 모두 보존 (호출 코드 변경 불필요):
///   - GetLevel(UpgradeEntry) / GetLevel(int)
///   - UpgradeCost(UpgradeEntry)
///   - GetValue(UpgradeEntry)               — 누적 보너스(레벨 * ValuePerLevel)
///   - CanUpgrade(UpgradeEntry)              — UI 표시용 검증
///   - Upgrade(UpgradeEntry)                 — 호스트 검증 요청 (낙관 반환)
///   - UpgradeChange 이벤트
///
/// 내부 동작:
///   - 모든 레벨 조회/변경은 PlayerEconomyNetState 로 위임
///   - Scrap 비용 차감도 PlayerEconomyNetState.ServerTryUpgradePersonal 내부에서 처리
///   - PlayerEconomyNetState.OnPersonalUpgradeChanged 를 받아 UpgradeChange 이벤트로 재방출
/// </summary>
public class PlayerIngameData : MonoBehaviour
{
    /// <summary>업그레이드 적용 시 호출. (entry, newLevel)</summary>
    public event Action<UpgradeEntry, int> UpgradeChange;

    private PlayerEconomyNetState _economyNetState;
    private PlayerStats _playerStats;
    private bool _eventBound;

    private void Awake()
    {
        _economyNetState = GetComponent<PlayerEconomyNetState>();
        _playerStats = GetComponent<PlayerStats>();

        if (_economyNetState == null)
        {
            DebugTool.Log("[PlayerIngameData] PlayerEconomyNetState 가 같은 GameObject 에 없습니다.", DebugType.Data, this);
        }
    }

    private void OnDisable()
    {
        UnbindEvents();
    }

    private void OnDestroy()
    {
        UnbindEvents();
    }

    private void Update()
    {
        TryBindEvents();   // NetworkBehaviour spawn 시점을 기다려 lazy bind
    }

    private void TryBindEvents()
    {
        if (_eventBound) return;
        if (_economyNetState == null) return;

        _economyNetState.OnPersonalUpgradeChanged += HandlePersonalUpgradeChanged;
        _eventBound = true;
    }

    private void UnbindEvents()
    {
        if (!_eventBound) return;
        if (_economyNetState != null)
        {
            _economyNetState.OnPersonalUpgradeChanged -= HandlePersonalUpgradeChanged;
        }
        _eventBound = false;
    }

    // ── 조회 ────────────────────────────────────────────────────────

    public int GetLevel(UpgradeEntry entry)
    {
        if (entry == null) return 0;
        return GetLevel(entry.Id);
    }

    public int GetLevel(int upgradeId)
    {
        if (_economyNetState == null) return 0;
        return _economyNetState.GetPersonalLevel(upgradeId);
    }

    public int UpgradeCost(UpgradeEntry entry)
    {
        if (entry == null) return 0;
        int currentLevel = GetLevel(entry);
        return entry.BaseCost + entry.CostIncrease * currentLevel;
    }

    public float GetValue(UpgradeEntry entry)
    {
        if (entry == null) return 0f;
        int currentLevel = GetLevel(entry);
        return entry.ValuePerLevel * currentLevel;
    }

    // ── 검증 (UI 표시용 — 호스트도 같은 로직으로 다시 검증) ──────────

    public bool CanUpgrade(UpgradeEntry entry)
    {
        if (entry == null) return false;
        if (!entry.IsEnabled) return false;

        int currentLevel = GetLevel(entry);
        if (currentLevel >= entry.MaxLevel) return false;

        int cost = UpgradeCost(entry);
        if (_economyNetState == null) return false;
        if (!_economyNetState.HasEnoughResource(ResourcesType.Scrap, cost))
        {
            DebugTool.Log($"업그레이드 비용 부족 / 필요 Scrap: {cost}", DebugType.Data, this);
            return false;
        }

        return true;
    }

    // ── 업그레이드 요청 (서버 권한) ─────────────────────────────────

    /// <summary>
    /// 호스트에 업그레이드 구매를 요청한다.
    /// 반환값은 "요청 송신 성공" 의미 — 실제 결과는 UpgradeChange 이벤트로 통보됨.
    /// 호스트 검증(IsEnabled / MaxLevel / Scrap 비용)에서 실패하면 NetworkList 갱신 안 됨.
    /// </summary>
    public bool Upgrade(UpgradeEntry entry)
    {
        if (entry == null) return false;
        if (_economyNetState == null)
        {
            DebugTool.Log("[PlayerIngameData] PlayerEconomyNetState 미준비", DebugType.Data, this);
            return false;
        }

        // 클라이언트 측 사전 검사 — 명백히 불가한 케이스는 RPC 보내지 않음
        if (!CanUpgrade(entry))
        {
            return false;
        }

        if (IsServer())
        {
            return _economyNetState.ServerTryUpgradePersonal(entry.Id);
        }

        if (!_economyNetState.IsOwner)
        {
            DebugTool.Log("[PlayerIngameData] Upgrade: IsOwner=false - 무시", DebugType.Data, this);
            return false;
        }

        _economyNetState.RequestUpgradePersonalServerRpc(entry.Id);
        return true;   // 요청 송신. 결과는 UpgradeChange 이벤트로.
    }

    // ── NetworkList 변경 → UpgradeChange 이벤트 재방출 ──────────────

    private void HandlePersonalUpgradeChanged(int upgradeId, int newLevel)
    {
        UpgradeEntry entry = ResolveEntry(upgradeId);
        if (entry == null)
        {
            DebugTool.Log($"[PlayerIngameData] UpgradeChange: id={upgradeId} entry 조회 실패 (이벤트 전달 불가)", DebugType.Data, this);
            return;
        }

        UpgradeChange?.Invoke(entry, newLevel);
    }

    /// <summary>
    /// id → UpgradeEntry. UI 갱신 콜백에서 entry 객체가 필요해서 다시 시트에서 찾아옴.
    /// PlayerStats.WeaponType + LocalDataAccess.GetUpgrade 로 ClassUpgradeData 순회 매칭.
    /// </summary>
    private UpgradeEntry ResolveEntry(int upgradeId)
    {
        if (_playerStats == null) _playerStats = GetComponent<PlayerStats>();
        if (_playerStats == null) return null;
        if (LocalDataAccess.Instance == null || LocalDataAccess.Instance.Game == null) return null;

        ClassUpgradeData classData = LocalDataAccess.Instance.Game.GetUpgrade(_playerStats.WeaponType);
        if (classData == null) return null;

        // Common 6
        if (classData.MaxHealth != null && classData.MaxHealth.Id == upgradeId) return classData.MaxHealth;
        if (classData.Stamina != null && classData.Stamina.Id == upgradeId) return classData.Stamina;
        if (classData.StaminaRegen != null && classData.StaminaRegen.Id == upgradeId) return classData.StaminaRegen;
        if (classData.Damage != null && classData.Damage.Id == upgradeId) return classData.Damage;
        if (classData.AttackSpeed != null && classData.AttackSpeed.Id == upgradeId) return classData.AttackSpeed;
        if (classData.MoveSpeed != null && classData.MoveSpeed.Id == upgradeId) return classData.MoveSpeed;

        // Class-specific
        switch (classData)
        {
            case MeleeUpgradeData m:
                if (m.DamageReduction != null && m.DamageReduction.Id == upgradeId) return m.DamageReduction;
                if (m.SprintSpeed != null && m.SprintSpeed.Id == upgradeId) return m.SprintSpeed;
                break;
            case RifleUpgradeData r:
                if (r.MagazineCapacity != null && r.MagazineCapacity.Id == upgradeId) return r.MagazineCapacity;
                if (r.ReloadTime != null && r.ReloadTime.Id == upgradeId) return r.ReloadTime;
                if (r.PierceCount != null && r.PierceCount.Id == upgradeId) return r.PierceCount;
                if (r.BulletDistance != null && r.BulletDistance.Id == upgradeId) return r.BulletDistance;
                break;
            case ShotgunUpgradeData s:
                if (s.MagazineCapacity != null && s.MagazineCapacity.Id == upgradeId) return s.MagazineCapacity;
                if (s.ReloadTime != null && s.ReloadTime.Id == upgradeId) return s.ReloadTime;
                if (s.KnockbackPower != null && s.KnockbackPower.Id == upgradeId) return s.KnockbackPower;
                if (s.ProjectileCount != null && s.ProjectileCount.Id == upgradeId) return s.ProjectileCount;
                break;
            case PistolUpgradeData p:
                if (p.MagazineCapacity != null && p.MagazineCapacity.Id == upgradeId) return p.MagazineCapacity;
                if (p.ReloadTime != null && p.ReloadTime.Id == upgradeId) return p.ReloadTime;
                if (p.SightRange != null && p.SightRange.Id == upgradeId) return p.SightRange;
                if (p.CollectRange != null && p.CollectRange.Id == upgradeId) return p.CollectRange;
                break;
        }
        return null;
    }

    private static bool IsServer()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;
    }
}
