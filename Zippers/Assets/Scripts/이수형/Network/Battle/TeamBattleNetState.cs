using System;
using Unity.Netcode;
using UnityEngine;
using Zippers.Network.Contracts;

namespace Zippers.Network
{
    /// <summary>
    /// 팀 공용 전투 상태(IsBattle / BattleState) 서버 권한 보관소 및 이벤트 버스.
    ///
    /// 소유: 이수형 (F · 자원·성장 라인)
    /// 구현: IBattleStateBus
    ///
    /// 동작:
    ///   - 호스트가 김영찬님의 MapData.NetworkMapData.NodeState 변화를 구독
    ///   - NodeState → BattleState 정규화 매핑 (BattleState.cs 주석 참고)
    ///   - NetworkVariable&lt;BattleState&gt; 로 모든 클라에 동기화
    ///   - 클라이언트 측에서 OnValueChanged 콜백으로 OnBattleStarted / Cleared / Ended 이벤트 발화
    ///
    /// 배치: GameScene 의 빈 GameObject 에 NetworkObject + 이 컴포넌트 부착. team singleton.
    ///
    /// ─────────────────────────────────────────────────────────────
    /// Day 0 단계 — 빈 스켈레톤. 향후 단계 진행 시 채울 항목:
    ///   Step 5: NetworkVariable&lt;BattleState&gt; CurrentBattleState 추가
    ///           호스트가 NodeState.OnValueChanged 구독 → BattleState 정규화 → NetworkVariable 갱신
    ///           OnValueChanged 콜백에서 OnBattleStarted/Cleared/Ended 이벤트 발화
    ///   Step 7: TeamBattleUpgradeEffect 가 NodeState 직접 구독 대신 IBattleStateBus 구독으로 전환
    /// ─────────────────────────────────────────────────────────────
    /// </summary>
    public class TeamBattleNetState : NetworkBehaviour, IBattleStateBus
    {
        public static TeamBattleNetState Instance { get; private set; }

        // TODO Step 5: NetworkVariable<BattleState> _currentBattleState 추가

        public bool IsBattle => CurrentState == BattleState.Battle;
        public BattleState CurrentState => BattleState.Ready;    // TODO Step 5: NetworkVariable.Value

        // 이벤트 — Step 5 에서 NetworkVariable.OnValueChanged 에서 정규화 발화
#pragma warning disable 0067 // 빈 스켈레톤이라 발화 코드 아직 없음. Step 진행 시 제거.
        public event Action OnBattleStarted;
        public event Action OnBattleCleared;
        public event Action OnBattleEnded;
#pragma warning restore 0067

        // ── Singleton ──────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                DebugTool.Log("[TeamBattleNetState] 중복 인스턴스 감지 - 제거", DebugType.EconomyNet, this);
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
            DebugTool.Log($"TeamBattleNetState spawned: IsServer={IsServer}", DebugType.EconomyNet, this);
            // TODO Step 5: 호스트만 NodeState.OnValueChanged 구독
        }

        public override void OnNetworkDespawn()
        {
            DebugTool.Log("TeamBattleNetState despawned", DebugType.EconomyNet, this);
            // TODO Step 5: NodeState.OnValueChanged 구독 해제
        }
    }
}
