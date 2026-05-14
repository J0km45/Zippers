using System;
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
    /// 보관 데이터 (TODO 단계 진행 시 NetworkVariable 로 구현):
    ///   - Scrap (NetworkVariable&lt;float&gt;)
    ///   - InfectionSample (NetworkVariable&lt;float&gt;)
    ///   - personalUpgradeLevels (NetworkList&lt;UpgradeLevelEntry&gt;)
    ///
    /// 배치: 플레이어 프리팹에 부착 (PlayerCombatNetState 와 동일 객체)
    ///
    /// ─────────────────────────────────────────────────────────────
    /// Day 0 단계 — 빈 스켈레톤. 향후 단계 진행 시 채울 항목:
    ///   Step 2: NetworkVariable&lt;float&gt; Scrap, InfectionSample 추가
    ///           PlayerResourceCollector 의 _scrap / _infectionSample 위임
    ///           ServerGrantResource / ServerSpendResource 실 구현
    ///   Step 3: NetworkList&lt;UpgradeLevelEntry&gt; PersonalUpgradeLevels 추가
    ///           PlayerIngameData.Upgrade 를 ServerRpc 로 검증 후 NetworkList 갱신
    ///           OnPersonalUpgradeChanged 이벤트 발화 → PlayerStats.OnStatsRecalculated
    /// ─────────────────────────────────────────────────────────────
    /// </summary>
    public class PlayerEconomyNetState : NetworkBehaviour, IResourceCommands
    {
        // TODO Step 2: NetworkVariable<float> Scrap (Server write, Owner+Server read)
        // TODO Step 2: NetworkVariable<float> InfectionSample (Server write, Owner+Server read)
        // TODO Step 3: NetworkList<UpgradeLevelEntry> PersonalUpgradeLevels (Server write, Owner+Server read)

        // 이벤트 — Step 2~3 에서 NetworkVariable.OnValueChanged / NetworkList.OnListChanged 에서 발화
#pragma warning disable 0067 // 빈 스켈레톤이라 발화 코드 아직 없음. Step 진행 시 제거.
        public event Action<ResourcesType, float, float> OnResourceChanged;       // (type, current, delta)
        public event Action<int, int> OnPersonalUpgradeChanged;                    // (upgradeId, level)
#pragma warning restore 0067

        // ── IResourceCommands 구현 (TODO Step 2) ──────────────────────────
        public bool ServerGrantResource(ulong clientId, ResourcesType type, float amount, string source)
        {
            // TODO Step 2: IsServer 가드 + 본인 OwnerClientId 비교 + Scrap/InfectionSample 증가
            DebugTool.Log($"[PlayerEconomyNetState] ServerGrantResource stub: type={type}, amount={amount}, source={source}", DebugType.EconomyNet, this);
            return false;
        }

        public bool ServerSpendResource(ulong clientId, ResourcesType type, float amount, string reason)
        {
            // TODO Step 2: IsServer 가드 + 보유량 검증 + 차감
            DebugTool.Log($"[PlayerEconomyNetState] ServerSpendResource stub: type={type}, amount={amount}, reason={reason}", DebugType.EconomyNet, this);
            return false;
        }

        // ── NetworkBehaviour 라이프사이클 ──────────────────────────────────
        public override void OnNetworkSpawn()
        {
            DebugTool.Log(
                $"PlayerEconomyNetState spawned: ownerClientId={OwnerClientId}, IsServer={IsServer}, IsOwner={IsOwner}",
                DebugType.EconomyNet, this);
        }

        public override void OnNetworkDespawn()
        {
            DebugTool.Log(
                $"PlayerEconomyNetState despawned: ownerClientId={OwnerClientId}",
                DebugType.EconomyNet, this);
        }
    }
}
