using System;
using Unity.Netcode;
using UnityEngine;
using Zippers.Network.Contracts;

namespace Zippers.Network
{
    /// <summary>
    /// 팀 공용 자원·성장 상태(Supplies / 팀 업그레이드 레벨 / 상점 구매 기록) 서버 권한 보관소.
    ///
    /// 소유: 이수형 (F · 자원·성장 라인)
    /// 구현: IResourceCommands (team Supplies 처리)
    ///
    /// 보관 데이터:
    ///   ✅ Supplies (NetworkVariable&lt;float&gt;) — Server write, Everyone read
    ///   ✅ TeamUpgradeLevels (NetworkList&lt;UpgradeLevelEntry&gt;) — Server write, Everyone read
    ///   ✅ CurrentShopPurchases (NetworkList&lt;int&gt;) — Server write, Everyone read
    ///                                                  같은 상점에서 동일 팀 업그레이드 재구매 방지
    ///
    /// 배치: GameScene 의 빈 GameObject 에 NetworkObject + 이 컴포넌트 부착. team singleton.
    ///
    /// ─────────────────────────────────────────────────────────────
    /// 진행 단계:
    ///   ✅ Step 1: Supplies NetworkVariable + ServerRpc + TeamResourceManager 호환 래퍼
    ///   ✅ Step 4: TeamUpgradeLevels / CurrentShopPurchases NetworkList + ServerRpc + 호스트 검증
    ///              일회성 업그레이드(ClearHeal/Battle*) vs 영구 업그레이드 분기
    /// ─────────────────────────────────────────────────────────────
    /// </summary>
    public class TeamEconomyNetState : NetworkBehaviour, IResourceCommands
    {
        public static TeamEconomyNetState Instance { get; private set; }

        // ── Supplies (Everyone read — 팀 공용) ─────────────────────────
        private readonly NetworkVariable<float> _supplies =
            new NetworkVariable<float>(
                0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public NetworkVariable<float> Supplies => _supplies;
        public float CurrentSupplies => _supplies.Value;

        // ── TeamUpgradeLevels (Everyone read) ──────────────────────────
        private NetworkList<UpgradeLevelEntry> _teamUpgradeLevels;
        public NetworkList<UpgradeLevelEntry> TeamUpgradeLevels => _teamUpgradeLevels;

        // ── CurrentShopPurchases (Everyone read) ───────────────────────
        // 같은 상점에서 동일 팀 업그레이드 재구매 방지용. 상점 진입 시 초기화.
        private NetworkList<int> _currentShopPurchases;
        public NetworkList<int> CurrentShopPurchases => _currentShopPurchases;

        // ── 이벤트 ───────────────────────────────────────────────────────
        public event Action<ResourcesType, float, float> OnTeamResourceChanged;
        public event Action<int, int> OnTeamUpgradeChanged;   // (upgradeId, level)

        // ── Singleton ──────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                DebugTool.Log("중복 인스턴스 - 제거", DebugType.EconomyNet, this);
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // NetworkList 는 Awake 에서 생성해야 NGO 가 인식
            _teamUpgradeLevels = new NetworkList<UpgradeLevelEntry>();
            _currentShopPurchases = new NetworkList<int>();
        }

        public override void OnDestroy()
        {
            if (Instance == this) Instance = null;
            base.OnDestroy();
        }

        public override void OnNetworkSpawn()
        {
            _supplies.OnValueChanged += HandleSuppliesChanged;
            _teamUpgradeLevels.OnListChanged += HandleTeamUpgradeListChanged;

            DebugTool.Log(
                $"TeamEconomyNetState spawned: IsServer={IsServer}, Supplies={_supplies.Value}",
                DebugType.EconomyNet, this);
        }

        public override void OnNetworkDespawn()
        {
            _supplies.OnValueChanged -= HandleSuppliesChanged;
            if (_teamUpgradeLevels != null)
            {
                _teamUpgradeLevels.OnListChanged -= HandleTeamUpgradeListChanged;
            }

            DebugTool.Log("TeamEconomyNetState despawned", DebugType.EconomyNet, this);
        }

        // ── IResourceCommands 구현 (Supplies) ───────────────────────────

        public bool ServerGrantResource(ulong clientId, ResourcesType type, float amount, string source)
        {
            if (!IsServer)
            {
                DebugTool.Warning($"ServerGrantResource 호스트 전용 (type={type})", DebugType.EconomyNet, this);
                return false;
            }
            if (type != ResourcesType.Supplies) return false;
            if (amount <= 0f) return false;

            _supplies.Value += amount;
            DebugTool.Log($"Supplies Grant: +{amount} → {_supplies.Value} (source={source})", DebugType.EconomyNet, this);
            return true;
        }

        public bool ServerSpendResource(ulong clientId, ResourcesType type, float amount, string reason)
        {
            if (!IsServer) return false;
            if (type != ResourcesType.Supplies) return false;
            if (amount <= 0f) return false;
            if (_supplies.Value < amount)
            {
                DebugTool.Log($"Supplies 부족: 필요 {amount}, 보유 {_supplies.Value} (reason={reason})", DebugType.EconomyNet, this);
                return false;
            }
            _supplies.Value -= amount;
            DebugTool.Log($"Supplies Spend: -{amount} → {_supplies.Value} (reason={reason})", DebugType.EconomyNet, this);
            return true;
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestGrantSuppliesServerRpc(float amount, ServerRpcParams rpcParams = default)
        {
            ServerGrantResource(rpcParams.Receive.SenderClientId, ResourcesType.Supplies, amount, $"client_rpc:{rpcParams.Receive.SenderClientId}");
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestSpendSuppliesServerRpc(float amount, ServerRpcParams rpcParams = default)
        {
            ServerSpendResource(rpcParams.Receive.SenderClientId, ResourcesType.Supplies, amount, $"client_rpc:{rpcParams.Receive.SenderClientId}");
        }

        public bool HasEnoughSupplies(float amount)
        {
            if (amount <= 0f) return false;
            return _supplies.Value >= amount;
        }

        // ── 팀 업그레이드 — 조회 ────────────────────────────────────────

        public int GetTeamLevel(int upgradeId)
        {
            if (_teamUpgradeLevels == null) return 0;
            for (int i = 0; i < _teamUpgradeLevels.Count; i++)
            {
                if (_teamUpgradeLevels[i].UpgradeId == upgradeId)
                {
                    return _teamUpgradeLevels[i].Level;
                }
            }
            return 0;
        }

        private int IndexOfTeamUpgrade(int upgradeId)
        {
            for (int i = 0; i < _teamUpgradeLevels.Count; i++)
            {
                if (_teamUpgradeLevels[i].UpgradeId == upgradeId) return i;
            }
            return -1;
        }

        public bool IsPurchasedInCurrentShop(int upgradeId)
        {
            if (_currentShopPurchases == null) return false;
            for (int i = 0; i < _currentShopPurchases.Count; i++)
            {
                if (_currentShopPurchases[i] == upgradeId) return true;
            }
            return false;
        }

        // ── 팀 업그레이드 — 호스트 권한 변경 ────────────────────────────

        /// <summary>
        /// 호스트에서 호출. entry 검증 + Supplies 차감 + 레벨 갱신 + 구매 기록.
        /// 일회성 업그레이드(ClearHeal/Battle*)는 level=1 고정, 영구는 누적.
        /// </summary>
        public bool ServerTryUpgradeTeam(int upgradeId)
        {
            if (!IsServer)
            {
                DebugTool.Warning($"ServerTryUpgradeTeam 호스트 전용 (id={upgradeId})", DebugType.EconomyNet, this);
                return false;
            }

            TeamUpgradeEntry entry = ResolveTeamEntry(upgradeId);
            if (entry == null)
            {
                DebugTool.Log($"팀 업그레이드 id={upgradeId} entry 조회 실패", DebugType.EconomyNet, this);
                return false;
            }

            // 같은 상점 재구매 방지
            if (IsPurchasedInCurrentShop(upgradeId))
            {
                DebugTool.Log($"팀 업그레이드 id={upgradeId} 같은 상점에서 이미 구매됨", DebugType.EconomyNet, this);
                return false;
            }

            int currentLevel = GetTeamLevel(upgradeId);
            bool isOneShot = IsOneShotBattleStat(entry.StatKey);

            // 영구만 MaxLevel 검사
            if (!isOneShot && currentLevel >= entry.MaxLevel)
            {
                DebugTool.Log($"팀 업그레이드 id={upgradeId} 최대 레벨 도달 ({currentLevel}/{entry.MaxLevel})", DebugType.EconomyNet, this);
                return false;
            }

            int cost = entry.BaseCost + entry.CostIncrease * currentLevel;
            if (_supplies.Value < cost)
            {
                DebugTool.Log($"팀 업그레이드 id={upgradeId} Supplies 부족 (필요 {cost}, 보유 {_supplies.Value})", DebugType.EconomyNet, this);
                return false;
            }

            // 차감 + 레벨 적용
            _supplies.Value -= cost;
            int nextLevel = isOneShot ? 1 : currentLevel + 1;

            int idx = IndexOfTeamUpgrade(upgradeId);
            if (idx < 0)
            {
                _teamUpgradeLevels.Add(new UpgradeLevelEntry(upgradeId, nextLevel));
            }
            else
            {
                _teamUpgradeLevels[idx] = new UpgradeLevelEntry(upgradeId, nextLevel);
            }

            _currentShopPurchases.Add(upgradeId);

            DebugTool.Log(
                $"팀 업그레이드 적용: id={upgradeId}, key={entry.StatKey}, level={currentLevel}→{nextLevel}, cost={cost}, supplies={_supplies.Value}",
                DebugType.EconomyNet, this);
            return true;
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestUpgradeTeamServerRpc(int upgradeId, ServerRpcParams rpcParams = default)
        {
            ServerTryUpgradeTeam(upgradeId);
        }

        // ── 상점 / 전투 종료 시 호스트 처리 ─────────────────────────────

        /// <summary>다음 상점 진입 시 호출. 현재 상점 구매 기록 초기화.</summary>
        public void ServerResetCurrentShopPurchases()
        {
            if (!IsServer) return;
            _currentShopPurchases.Clear();
            DebugTool.Log("현재 상점 구매 기록 초기화", DebugType.EconomyNet, this);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestResetCurrentShopPurchasesServerRpc()
        {
            ServerResetCurrentShopPurchases();
        }

        /// <summary>전투 종료 시 호출. 일회성 팀 업그레이드(ClearHeal/Battle*) 레벨을 0으로.</summary>
        public void ServerClearOneTimeBattleUpgrades()
        {
            if (!IsServer) return;
            if (LocalDataAccess.Instance == null || LocalDataAccess.Instance.Game == null) return;

            int cleared = 0;
            foreach (int upgradeId in LocalDataAccess.Instance.Game.GetAllTeamUpgradeIds())
            {
                TeamUpgradeEntry entry = LocalDataAccess.Instance.Game.GetTeamUpgrade(upgradeId);
                if (entry == null) continue;
                if (!IsOneShotBattleStat(entry.StatKey)) continue;

                int idx = IndexOfTeamUpgrade(upgradeId);
                if (idx >= 0 && _teamUpgradeLevels[idx].Level != 0)
                {
                    _teamUpgradeLevels[idx] = new UpgradeLevelEntry(upgradeId, 0);
                    cleared++;
                }
            }

            DebugTool.Log($"일회성 팀 업그레이드 초기화 완료 (count={cleared})", DebugType.EconomyNet, this);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestClearOneTimeBattleUpgradesServerRpc()
        {
            ServerClearOneTimeBattleUpgrades();
        }

        // ── Entry 조회 ────────────────────────────────────────────────

        private TeamUpgradeEntry ResolveTeamEntry(int upgradeId)
        {
            if (LocalDataAccess.Instance == null || LocalDataAccess.Instance.Game == null) return null;
            return LocalDataAccess.Instance.Game.GetTeamUpgrade(upgradeId);
        }

        private static bool IsOneShotBattleStat(TeamUpgradeStatKey key)
        {
            return key == TeamUpgradeStatKey.ClearHeal
                || key == TeamUpgradeStatKey.BattleDamage
                || key == TeamUpgradeStatKey.BattleMoveSpeed
                || key == TeamUpgradeStatKey.BattleAttackSpeed;
        }

        // ── 변경 콜백 → 이벤트 발화 ──────────────────────────────────

        private void HandleSuppliesChanged(float previous, float current)
        {
            float delta = current - previous;
            OnTeamResourceChanged?.Invoke(ResourcesType.Supplies, current, delta);
            DebugTool.Log($"Supplies 동기화: {previous} → {current} (Δ {delta:+0.##;-0.##;0})", DebugType.EconomyNet, this);
        }

        private void HandleTeamUpgradeListChanged(NetworkListEvent<UpgradeLevelEntry> ev)
        {
            switch (ev.Type)
            {
                case NetworkListEvent<UpgradeLevelEntry>.EventType.Add:
                case NetworkListEvent<UpgradeLevelEntry>.EventType.Value:
                case NetworkListEvent<UpgradeLevelEntry>.EventType.Insert:
                    OnTeamUpgradeChanged?.Invoke(ev.Value.UpgradeId, ev.Value.Level);
                    DebugTool.Log(
                        $"팀 업그레이드 동기화: id={ev.Value.UpgradeId}, level={ev.Value.Level} (eventType={ev.Type})",
                        DebugType.EconomyNet, this);
                    break;

                default:
                    break;
            }
        }
    }
}
