using System;
using Unity.Netcode;
using UnityEngine;
using Zippers.Network.Contracts;

namespace Zippers.Network
{
    /// <summary>
    /// 팀 공용 자원·성장 상태(Supplies / 팀 업그레이드 레벨) 서버 권한 보관소.
    ///
    /// 소유: 이수형 (F · 자원·성장 라인)
    /// 구현: IResourceCommands (team Supplies 처리)
    ///
    /// 보관 데이터:
    ///   - Supplies (NetworkVariable&lt;float&gt;) — Server write, Everyone read
    ///   - teamUpgradeLevels (NetworkList&lt;UpgradeLevelEntry&gt;) — TODO Step 4
    ///
    /// 배치: GameScene 의 빈 GameObject 에 NetworkObject + 이 컴포넌트 부착. team singleton.
    ///
    /// ─────────────────────────────────────────────────────────────
    /// 진행 단계:
    ///   ✅ Step 1: Supplies NetworkVariable + 변경 콜백 + ServerRpc
    ///              TeamResourceManager 를 호환 래퍼로 변환 (별도 파일)
    ///   ⏳ Step 2: TeamResourceManager.Instance.~ 호출처 일괄 치환 → TeamResourceManager.cs 삭제
    ///   ⏳ Step 4: NetworkList&lt;UpgradeLevelEntry&gt; TeamUpgradeLevels 추가
    /// ─────────────────────────────────────────────────────────────
    /// </summary>
    public class TeamEconomyNetState : NetworkBehaviour, IResourceCommands
    {
        public static TeamEconomyNetState Instance { get; private set; }

        // ── Supplies (NetworkVariable, Server write, Everyone read) ────────
        private readonly NetworkVariable<float> _supplies =
            new NetworkVariable<float>(
                0f,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        /// <summary>현재 팀 Supplies NetworkVariable. 모든 클라이언트 read 가능.</summary>
        public NetworkVariable<float> Supplies => _supplies;

        /// <summary>편의 접근자. <c>Instance.CurrentSupplies</c> 로 값만 빠르게 조회.</summary>
        public float CurrentSupplies => _supplies.Value;

        // TODO Step 4: NetworkList<UpgradeLevelEntry> TeamUpgradeLevels

        // ── 이벤트 ─────────────────────────────────────────────────────
        /// <summary>(type, current, delta) — 서버 권한 변경이 모든 클라이언트에 도착한 뒤 발화.</summary>
        public event Action<ResourcesType, float, float> OnTeamResourceChanged;

#pragma warning disable 0067 // Step 4 에서 발화 코드 추가 예정
        public event Action<int, int> OnTeamUpgradeChanged;   // (upgradeId, level)
#pragma warning restore 0067

        // ── IResourceCommands 구현 ─────────────────────────────────────

        public bool ServerGrantResource(ulong clientId, ResourcesType type, float amount, string source)
        {
            if (!IsServer)
            {
                DebugTool.Warning(
                    $"ServerGrantResource 는 호스트에서만 호출 가능 (type={type}, amount={amount})",
                    DebugType.EconomyNet, this);
                return false;
            }

            if (type != ResourcesType.Supplies)
            {
                DebugTool.Log(
                    $"팀 재화로 관리하지 않는 타입: {type}",
                    DebugType.EconomyNet, this);
                return false;
            }

            if (amount <= 0f)
            {
                DebugTool.Log(
                    $"증가량 부적절: amount={amount}",
                    DebugType.EconomyNet, this);
                return false;
            }

            float before = _supplies.Value;
            _supplies.Value = before + amount;

            DebugTool.Log(
                $"Supplies Grant: +{amount} → {_supplies.Value} (source={source}, clientId={clientId})",
                DebugType.EconomyNet, this);
            return true;
        }

        public bool ServerSpendResource(ulong clientId, ResourcesType type, float amount, string reason)
        {
            if (!IsServer)
            {
                DebugTool.Warning(
                    $"ServerSpendResource 는 호스트에서만 호출 가능 (type={type}, amount={amount})",
                    DebugType.EconomyNet, this);
                return false;
            }

            if (type != ResourcesType.Supplies)
            {
                return false;
            }

            if (amount <= 0f)
            {
                return false;
            }

            if (_supplies.Value < amount)
            {
                DebugTool.Log(
                    $"Supplies 부족: 필요 {amount}, 보유 {_supplies.Value} (reason={reason})",
                    DebugType.EconomyNet, this);
                return false;
            }

            float before = _supplies.Value;
            _supplies.Value = before - amount;

            DebugTool.Log(
                $"Supplies Spend: -{amount} → {_supplies.Value} (reason={reason}, clientId={clientId})",
                DebugType.EconomyNet, this);
            return true;
        }

        // ── ServerRpc — 클라이언트가 호스트로 변경 요청 보낼 때 사용 ────────
        // 보통은 몬스터/픽업 등 서버 측 흐름에서 ServerGrantResource 직접 호출.
        // 클라 → 서버 요청이 필요한 경우(예: 디버그 UI에서 자기가 자기 자원 증감) 만 사용.

        [ServerRpc(RequireOwnership = false)]
        public void RequestGrantSuppliesServerRpc(float amount, ServerRpcParams rpcParams = default)
        {
            ulong sender = rpcParams.Receive.SenderClientId;
            ServerGrantResource(sender, ResourcesType.Supplies, amount, $"client_rpc:{sender}");
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestSpendSuppliesServerRpc(float amount, ServerRpcParams rpcParams = default)
        {
            ulong sender = rpcParams.Receive.SenderClientId;
            ServerSpendResource(sender, ResourcesType.Supplies, amount, $"client_rpc:{sender}");
        }

        // ── 호스트/클라 공통 헬퍼 ───────────────────────────────────────

        /// <summary>호스트/클라 모두 호출 가능. NetworkVariable 현재값 기반.</summary>
        public bool HasEnoughSupplies(float amount)
        {
            if (amount <= 0f)
            {
                return false;
            }
            return _supplies.Value >= amount;
        }

        // ── Singleton ──────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                DebugTool.Log(
                    "중복 인스턴스 감지 - 제거",
                    DebugType.EconomyNet, this);
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public override void OnDestroy()
        {
            if (Instance == this) Instance = null;
            base.OnDestroy();
        }

        // ── NetworkBehaviour 라이프사이클 ──────────────────────────────

        public override void OnNetworkSpawn()
        {
            _supplies.OnValueChanged += HandleSuppliesChanged;
            DebugTool.Log(
                $"TeamEconomyNetState spawned: IsServer={IsServer}, Supplies={_supplies.Value}",
                DebugType.EconomyNet, this);
        }

        public override void OnNetworkDespawn()
        {
            _supplies.OnValueChanged -= HandleSuppliesChanged;
            DebugTool.Log(
                "TeamEconomyNetState despawned",
                DebugType.EconomyNet, this);
        }

        private void HandleSuppliesChanged(float previous, float current)
        {
            float delta = current - previous;
            OnTeamResourceChanged?.Invoke(ResourcesType.Supplies, current, delta);
            DebugTool.Log(
                $"Supplies 동기화: {previous} → {current} (Δ {delta:+0.##;-0.##;0})",
                DebugType.EconomyNet, this);
        }
    }
}
