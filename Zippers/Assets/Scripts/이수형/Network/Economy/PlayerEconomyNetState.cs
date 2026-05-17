using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Zippers.Network.Contracts;

namespace Zippers.Network
{
    /// <summary>
    /// 플레이어 한 명의 자원·성장 상태(개인 재화 / 개인 업그레이드) 서버 권한 보관소.
    ///
    /// 소유: 이수형 (F · 자원·성장 라인)
    /// 구현: IResourceCommands (per-player Scrap / InfectionSample 처리)
    ///
    /// 보관 데이터:
    ///   ✅ Scrap (NetworkVariable&lt;float&gt;) — Server write, Owner read
    ///   ✅ InfectionSample (NetworkVariable&lt;float&gt;) — Server write, Owner read
    ///   ✅ PersonalUpgradeLevels (NetworkList&lt;UpgradeLevelEntry&gt;) — Server write, Owner read
    ///
    /// 배치: 플레이어 프리팹에 부착 (PlayerCombatNetState, PlayerStats, PlayerUpgradeProvider 와 동일 객체)
    ///
    /// ─────────────────────────────────────────────────────────────
    /// 진행 단계:
    ///   ✅ Step 2: Scrap / InfectionSample NetworkVariable + ServerGrantResource/ServerSpendResource 실 구현
    ///   ✅ Step 3: PersonalUpgradeLevels NetworkList + RequestUpgradePersonalServerRpc + 호스트 검증
    /// ─────────────────────────────────────────────────────────────
    /// </summary>
    public class PlayerEconomyNetState : NetworkBehaviour, IResourceCommands
    {
        // ── Scrap / InfectionSample ──────────────────────────────────────
        private readonly NetworkVariable<float> _scrap =
            new NetworkVariable<float>(
                0f, NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<float> _infectionSample =
            new NetworkVariable<float>(
                0f, NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Server);

        public NetworkVariable<float> Scrap => _scrap;
        public NetworkVariable<float> InfectionSample => _infectionSample;
        public float CurrentScrap => _scrap.Value;
        public float CurrentInfectionSample => _infectionSample.Value;

        // ── PersonalUpgradeLevels (NetworkList) ─────────────────────────
        // Server write, Owner read — 다른 플레이어가 내 업그레이드 레벨을 볼 일은 없음.
        // 어떤 플레이어의 데미지가 강한지 등은 PlayerStats.TotalMinDamage 등 파생값으로 표현.
        private NetworkList<UpgradeLevelEntry> _personalUpgradeLevels;

        public NetworkList<UpgradeLevelEntry> PersonalUpgradeLevels => _personalUpgradeLevels;

        // ── 옆 컴포넌트 캐시 (entry 조회용, 호스트만 사용) ─────────────────
        private PlayerStats _playerStats;
        private PlayerUpgradeProvider _upgradeProvider;

        // ── 이벤트 ───────────────────────────────────────────────────────
        public event Action<ResourcesType, float, float> OnResourceChanged;
        public event Action<int, int> OnPersonalUpgradeChanged;   // (upgradeId, level)

        // ── Lifecycle ──────────────────────────────────────────────────

        private void Awake()
        {
            // NetworkList 는 Awake 에서 생성해야 NGO 가 인식
            _personalUpgradeLevels = new NetworkList<UpgradeLevelEntry>();

            _playerStats = GetComponent<PlayerStats>();
            _upgradeProvider = GetComponent<PlayerUpgradeProvider>();
        }

        public override void OnNetworkSpawn()
        {
            _scrap.OnValueChanged += HandleScrapChanged;
            _infectionSample.OnValueChanged += HandleInfectionSampleChanged;
            _personalUpgradeLevels.OnListChanged += HandlePersonalUpgradeListChanged;

            DebugTool.Log(
                $"PlayerEconomyNetState spawned: ownerClientId={OwnerClientId}, IsServer={IsServer}, IsOwner={IsOwner}",
                DebugType.EconomyNet, this);
        }

        public override void OnNetworkDespawn()
        {
            _scrap.OnValueChanged -= HandleScrapChanged;
            _infectionSample.OnValueChanged -= HandleInfectionSampleChanged;
            if (_personalUpgradeLevels != null)
            {
                _personalUpgradeLevels.OnListChanged -= HandlePersonalUpgradeListChanged;
            }

            DebugTool.Log(
                $"PlayerEconomyNetState despawned: ownerClientId={OwnerClientId}",
                DebugType.EconomyNet, this);
        }

        // ── IResourceCommands 구현 (Scrap / InfectionSample) ───────────

        public bool ServerGrantResource(ulong clientId, ResourcesType type, float amount, string source)
        {
            if (!IsServer)
            {
                DebugTool.Warning($"ServerGrantResource 는 호스트 전용 (type={type})", DebugType.EconomyNet, this);
                return false;
            }
            if (amount <= 0f) return false;
            if (clientId != OwnerClientId)
            {
                DebugTool.Warning($"clientId 불일치 — 요청={clientId}, 본인={OwnerClientId}", DebugType.EconomyNet, this);
                return false;
            }

            switch (type)
            {
                case ResourcesType.Scrap:
                    _scrap.Value += amount;
                    DebugTool.Log($"Scrap Grant: +{amount} → {_scrap.Value} (source={source})", DebugType.EconomyNet, this);
                    return true;

                case ResourcesType.InfectionSample:
                    _infectionSample.Value += amount;
                    DebugTool.Log($"InfectionSample Grant: +{amount} → {_infectionSample.Value} (source={source})", DebugType.EconomyNet, this);
                    return true;

                default:
                    return false;
            }
        }

        public bool ServerSpendResource(ulong clientId, ResourcesType type, float amount, string reason)
        {
            if (!IsServer) return false;
            if (amount <= 0f) return false;
            if (clientId != OwnerClientId) return false;

            switch (type)
            {
                case ResourcesType.Scrap:
                    if (_scrap.Value < amount)
                    {
                        DebugTool.Log($"Scrap 부족: 필요 {amount}, 보유 {_scrap.Value} (reason={reason})", DebugType.EconomyNet, this);
                        return false;
                    }
                    _scrap.Value -= amount;
                    DebugTool.Log($"Scrap Spend: -{amount} → {_scrap.Value} (reason={reason})", DebugType.EconomyNet, this);
                    return true;

                case ResourcesType.InfectionSample:
                    if (_infectionSample.Value < amount) return false;
                    _infectionSample.Value -= amount;
                    DebugTool.Log($"InfectionSample Spend: -{amount} → {_infectionSample.Value} (reason={reason})", DebugType.EconomyNet, this);
                    return true;

                default:
                    return false;
            }
        }

        // ── ServerRpc (자원) ───────────────────────────────────────────

        [ServerRpc(RequireOwnership = true)]
        public void RequestGrantResourceServerRpc(int typeInt, float amount, ServerRpcParams rpcParams = default)
        {
            ResourcesType type = (ResourcesType)typeInt;

            ServerGrantResourceToAllPlayers(
                type,
                amount,
                $"client_rpc:{rpcParams.Receive.SenderClientId}"
            );
        }

        [ServerRpc(RequireOwnership = true)]
        public void RequestSpendResourceServerRpc(int typeInt, float amount, ServerRpcParams rpcParams = default)
        {
            ServerSpendResource(rpcParams.Receive.SenderClientId, (ResourcesType)typeInt, amount, "client_rpc");
        }

        // ── 자원 헬퍼 ──────────────────────────────────────────────────

        public bool HasEnoughResource(ResourcesType type, float amount)
        {
            if (amount <= 0f) return false;
            switch (type)
            {
                case ResourcesType.Scrap: return _scrap.Value >= amount;
                case ResourcesType.InfectionSample: return _infectionSample.Value >= amount;
                default: return false;
            }
        }

        public float GetResourceAmount(ResourcesType type)
        {
            switch (type)
            {
                case ResourcesType.Scrap: return _scrap.Value;
                case ResourcesType.InfectionSample: return _infectionSample.Value;
                default: return 0f;
            }
        }

        // ── 개인 업그레이드 — 조회 ─────────────────────────────────────

        /// <summary>업그레이드 id 의 현재 레벨. 없으면 0.</summary>
        public int GetPersonalLevel(int upgradeId)
        {
            if (_personalUpgradeLevels == null) return 0;
            for (int i = 0; i < _personalUpgradeLevels.Count; i++)
            {
                if (_personalUpgradeLevels[i].UpgradeId == upgradeId)
                {
                    return _personalUpgradeLevels[i].Level;
                }
            }
            return 0;
        }

        private int IndexOfUpgrade(int upgradeId)
        {
            for (int i = 0; i < _personalUpgradeLevels.Count; i++)
            {
                if (_personalUpgradeLevels[i].UpgradeId == upgradeId) return i;
            }
            return -1;
        }

        // ── 개인 업그레이드 — 호스트 권한 변경 ──────────────────────────

        /// <summary>
        /// 호스트에서 호출. entry 검증(IsEnabled / MaxLevel / Scrap 비용) 후 차감 + 레벨 +1.
        /// 성공 시 NetworkList 갱신 → OnListChanged → OnPersonalUpgradeChanged 발화.
        /// </summary>
        public bool ServerTryUpgradePersonal(int upgradeId)
        {
            if (!IsServer)
            {
                DebugTool.Warning($"ServerTryUpgradePersonal 는 호스트 전용 (id={upgradeId})", DebugType.EconomyNet, this);
                return false;
            }

            UpgradeEntry entry = ResolveEntry(upgradeId);
            if (entry == null)
            {
                DebugTool.Log($"업그레이드 id={upgradeId} 의 entry 를 찾지 못함", DebugType.EconomyNet, this);
                return false;
            }

            if (!entry.IsEnabled)
            {
                DebugTool.Log($"업그레이드 id={upgradeId} 비활성 (IsEnabled=false)", DebugType.EconomyNet, this);
                return false;
            }

            int currentLevel = GetPersonalLevel(upgradeId);
            if (currentLevel >= entry.MaxLevel)
            {
                DebugTool.Log($"업그레이드 id={upgradeId} 최대 레벨 도달 ({currentLevel}/{entry.MaxLevel})", DebugType.EconomyNet, this);
                return false;
            }

            int cost = entry.BaseCost + entry.CostIncrease * currentLevel;
            if (_scrap.Value < cost)
            {
                DebugTool.Log($"업그레이드 id={upgradeId} Scrap 부족 (필요 {cost}, 보유 {_scrap.Value})", DebugType.EconomyNet, this);
                return false;
            }

            // 차감 + 레벨 적용 — 두 NetworkVariable / NetworkList 변경이 거의 동시에 동기화됨
            _scrap.Value -= cost;
            int nextLevel = currentLevel + 1;
            int idx = IndexOfUpgrade(upgradeId);
            if (idx < 0)
            {
                _personalUpgradeLevels.Add(new UpgradeLevelEntry(upgradeId, nextLevel));
            }
            else
            {
                _personalUpgradeLevels[idx] = new UpgradeLevelEntry(upgradeId, nextLevel);
            }

            DebugTool.Log(
                $"개인 업그레이드 적용: id={upgradeId}, level={currentLevel}→{nextLevel}, cost={cost}, scrap={_scrap.Value}",
                DebugType.EconomyNet, this);
            return true;
        }

        [ServerRpc(RequireOwnership = true)]
        public void RequestUpgradePersonalServerRpc(int upgradeId, ServerRpcParams rpcParams = default)
        {
            ServerTryUpgradePersonal(upgradeId);
        }

        /// <summary>
        /// upgradeId → UpgradeEntry. 호스트 측에서 검증할 때 사용.
        /// PlayerStats.WeaponType 기반으로 PlayerUpgradeProvider.AbleUpgrade 를 호출해 매칭.
        /// IsEnabled=false 인 entry 는 AbleUpgrade 결과에 안 들어가므로 직접 ClassUpgradeData 순회로 보완.
        /// </summary>
        private UpgradeEntry ResolveEntry(int upgradeId)
        {
            if (_playerStats == null) _playerStats = GetComponent<PlayerStats>();
            if (_upgradeProvider == null) _upgradeProvider = GetComponent<PlayerUpgradeProvider>();

            if (_playerStats == null || _upgradeProvider == null) return null;
            if (LocalDataAccess.Instance == null || LocalDataAccess.Instance.Game == null) return null;

            // AbleUpgrade 는 IsEnabled=true 만 반환 — 우리는 IsEnabled=false 도 알아야 하므로 직접 순회
            ClassUpgradeData classData = LocalDataAccess.Instance.Game.GetUpgrade(_playerStats.WeaponType);
            if (classData == null) return null;

            return FindEntryInClassData(classData, upgradeId);
        }

        private static UpgradeEntry FindEntryInClassData(ClassUpgradeData data, int id)
        {
            // Common 6종
            UpgradeEntry hit = null;
            if ((hit = MatchId(data.MaxHealth, id)) != null) return hit;
            if ((hit = MatchId(data.Stamina, id)) != null) return hit;
            if ((hit = MatchId(data.StaminaRegen, id)) != null) return hit;
            if ((hit = MatchId(data.Damage, id)) != null) return hit;
            if ((hit = MatchId(data.AttackSpeed, id)) != null) return hit;
            if ((hit = MatchId(data.MoveSpeed, id)) != null) return hit;

            // Class-specific
            switch (data)
            {
                case MeleeUpgradeData m:
                    if ((hit = MatchId(m.DamageReduction, id)) != null) return hit;
                    if ((hit = MatchId(m.SprintSpeed, id)) != null) return hit;
                    break;
                case RifleUpgradeData r:
                    if ((hit = MatchId(r.MagazineCapacity, id)) != null) return hit;
                    if ((hit = MatchId(r.ReloadTime, id)) != null) return hit;
                    if ((hit = MatchId(r.PierceCount, id)) != null) return hit;
                    if ((hit = MatchId(r.BulletDistance, id)) != null) return hit;
                    break;
                case ShotgunUpgradeData s:
                    if ((hit = MatchId(s.MagazineCapacity, id)) != null) return hit;
                    if ((hit = MatchId(s.ReloadTime, id)) != null) return hit;
                    if ((hit = MatchId(s.KnockbackPower, id)) != null) return hit;
                    if ((hit = MatchId(s.ProjectileCount, id)) != null) return hit;
                    break;
                case PistolUpgradeData p:
                    if ((hit = MatchId(p.MagazineCapacity, id)) != null) return hit;
                    if ((hit = MatchId(p.ReloadTime, id)) != null) return hit;
                    if ((hit = MatchId(p.SightRange, id)) != null) return hit;
                    if ((hit = MatchId(p.CollectRange, id)) != null) return hit;
                    break;
            }

            return null;
        }

        private static UpgradeEntry MatchId(UpgradeEntry entry, int id)
        {
            return entry != null && entry.Id == id ? entry : null;
        }

        // ── 변경 콜백 → 이벤트 발화 ──────────────────────────────────

        private void HandleScrapChanged(float previous, float current)
        {
            float delta = current - previous;
            OnResourceChanged?.Invoke(ResourcesType.Scrap, current, delta);
            DebugTool.Log($"Scrap 동기화: {previous} → {current} (Δ {delta:+0.##;-0.##;0})", DebugType.EconomyNet, this);
        }

        private void HandleInfectionSampleChanged(float previous, float current)
        {
            float delta = current - previous;
            OnResourceChanged?.Invoke(ResourcesType.InfectionSample, current, delta);
            DebugTool.Log($"InfectionSample 동기화: {previous} → {current} (Δ {delta:+0.##;-0.##;0})", DebugType.EconomyNet, this);
        }

        private void HandlePersonalUpgradeListChanged(NetworkListEvent<UpgradeLevelEntry> ev)
        {
            // 우리 케이스에선 Add 또는 Value (덮어쓰기) 만 발생.
            switch (ev.Type)
            {
                case NetworkListEvent<UpgradeLevelEntry>.EventType.Add:
                case NetworkListEvent<UpgradeLevelEntry>.EventType.Value:
                case NetworkListEvent<UpgradeLevelEntry>.EventType.Insert:
                    OnPersonalUpgradeChanged?.Invoke(ev.Value.UpgradeId, ev.Value.Level);
                    DebugTool.Log(
                        $"개인 업그레이드 동기화: id={ev.Value.UpgradeId}, level={ev.Value.Level} (eventType={ev.Type})",
                        DebugType.EconomyNet, this);
                    break;

                default:
                    // RemoveAt / Clear 는 사용 안 함
                    break;
            }
        }
        public static bool ServerGrantResourceToAllPlayers(ResourcesType type, float amount, string source)
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            {
                DebugTool.Warning(
                    $"전체 플레이어 자원 지급은 서버에서만 가능 (type={type})",
                    DebugType.EconomyNet
                );
                return false;
            }

            if (amount <= 0f)
            {
                return false;
            }

            if (type == ResourcesType.Supplies)
            {
                DebugTool.Warning(
                    "Supplies는 TeamEconomyNetState에서 처리해야 합니다.",
                    DebugType.EconomyNet
                );
                return false;
            }

            int appliedCount = 0;

            foreach (NetworkObject networkObject in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
            {
                if (networkObject == null)
                {
                    continue;
                }

                if (!networkObject.TryGetComponent(out PlayerEconomyNetState economyNetState))
                {
                    continue;
                }

                economyNetState.ServerGrantResource(
                    economyNetState.OwnerClientId,
                    type,
                    amount,
                    source
                );

                appliedCount++;
            }

            DebugTool.Log(
                $"전체 플레이어 자원 지급 완료 / Type: {type}, Amount: {amount}, Count: {appliedCount}, Source: {source}",
                DebugType.EconomyNet
            );

            return appliedCount > 0;
        }
    }
}
