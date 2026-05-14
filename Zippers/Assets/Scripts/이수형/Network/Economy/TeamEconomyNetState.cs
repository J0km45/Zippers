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
    /// 보관 데이터 (TODO 단계 진행 시 NetworkVariable 로 구현):
    ///   - Supplies (NetworkVariable&lt;float&gt;)
    ///   - teamUpgradeLevels (NetworkList&lt;UpgradeLevelEntry&gt;)
    ///
    /// 배치: GameScene 의 빈 GameObject 에 NetworkObject + 이 컴포넌트 부착. team singleton.
    ///
    /// ─────────────────────────────────────────────────────────────
    /// Day 0 단계 — 빈 스켈레톤. 향후 단계 진행 시 채울 항목:
    ///   Step 1: NetworkVariable&lt;float&gt; Supplies 추가
    ///           TeamResourceManager.cs 를 호환 래퍼로 변환 (내부 _supplies 제거 → Instance.Supplies.Value 위임)
    ///   Step 2: TeamResourceManager.Instance.~ 호출처 일괄 치환 → TeamResourceManager.cs 삭제
    ///           ServerGrantResource / ServerSpendResource 실 구현
    ///   Step 4: NetworkList&lt;UpgradeLevelEntry&gt; TeamUpgradeLevels 추가
    ///           TeamUpgradeData.Upgrade 를 ServerRpc 로 검증 후 NetworkList 갱신
    ///           OnTeamUpgradeChanged 이벤트 → PlayerStats(전 플레이어).OnStatsRecalculated 트리거
    /// ─────────────────────────────────────────────────────────────
    /// </summary>
    public class TeamEconomyNetState : NetworkBehaviour, IResourceCommands
    {
        public static TeamEconomyNetState Instance { get; private set; }

        // TODO Step 1: NetworkVariable<float> Supplies (Server write, Everyone read)
        // TODO Step 4: NetworkList<UpgradeLevelEntry> TeamUpgradeLevels (Server write, Everyone read)

        // 이벤트 — Step 1, 4 에서 NetworkVariable.OnValueChanged / NetworkList.OnListChanged 에서 발화
#pragma warning disable 0067 // 빈 스켈레톤이라 발화 코드 아직 없음. Step 진행 시 제거.
        public event Action<ResourcesType, float, float> OnTeamResourceChanged;    // (type, current, delta)
        public event Action<int, int> OnTeamUpgradeChanged;                         // (upgradeId, level)
#pragma warning restore 0067

        // ── IResourceCommands 구현 (TODO Step 1~2) ────────────────────────
        public bool ServerGrantResource(ulong clientId, ResourcesType type, float amount, string source)
        {
            // TODO Step 1~2: IsServer 가드 + Supplies 증가 (clientId 는 로그용)
            DebugTool.Log($"[TeamEconomyNetState] ServerGrantResource stub: type={type}, amount={amount}, source={source}", DebugType.EconomyNet, this);
            return false;
        }

        public bool ServerSpendResource(ulong clientId, ResourcesType type, float amount, string reason)
        {
            // TODO Step 1~2: IsServer 가드 + Supplies 보유량 검증 + 차감
            DebugTool.Log($"[TeamEconomyNetState] ServerSpendResource stub: type={type}, amount={amount}, reason={reason}", DebugType.EconomyNet, this);
            return false;
        }

        // ── Singleton ──────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                DebugTool.Log("[TeamEconomyNetState] 중복 인스턴스 감지 - 제거", DebugType.EconomyNet, this);
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

        // ── NetworkBehaviour 라이프사이클 ──────────────────────────────────
        public override void OnNetworkSpawn()
        {
            DebugTool.Log($"TeamEconomyNetState spawned: IsServer={IsServer}", DebugType.EconomyNet, this);
        }

        public override void OnNetworkDespawn()
        {
            DebugTool.Log("TeamEconomyNetState despawned", DebugType.EconomyNet, this);
        }
    }
}
