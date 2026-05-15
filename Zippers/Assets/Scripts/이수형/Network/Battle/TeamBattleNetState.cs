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
    ///   - NetworkVariable&lt;BattleState&gt; 로 모든 클라에 동기화 (Everyone read)
    ///   - 모든 측(호스트 포함) NetworkVariable.OnValueChanged 콜백으로 OnBattleStarted/Cleared/Ended 발화
    ///
    /// 배치: GameScene 의 빈 GameObject 에 NetworkObject + 이 컴포넌트 부착. team singleton.
    ///
    /// ─────────────────────────────────────────────────────────────
    /// 진행 단계:
    ///   ✅ Step 5: BattleState NetworkVariable + NodeState 구독 + IBattleStateBus 노출
    ///   ⏳ Step 7: TeamBattleUpgradeEffect 가 NodeState 직접 구독 대신 IBattleStateBus 구독으로 전환
    /// ─────────────────────────────────────────────────────────────
    /// </summary>
    public class TeamBattleNetState : NetworkBehaviour, IBattleStateBus
    {
        public static TeamBattleNetState Instance { get; private set; }

        // ── BattleState NetworkVariable (Server write, Everyone read) ─────
        private readonly NetworkVariable<BattleState> _currentState =
            new NetworkVariable<BattleState>(
                BattleState.Ready,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        public NetworkVariable<BattleState> CurrentStateVar => _currentState;
        public bool IsBattle => _currentState.Value == BattleState.Battle;
        public BattleState CurrentState => _currentState.Value;

        // ── 이벤트 ───────────────────────────────────────────────────────
        public event Action OnBattleStarted;
        public event Action OnBattleCleared;
        public event Action OnBattleEnded;

        // ── 호스트 측 NodeState 구독 ─────────────────────────────────────
        private MapData _mapData;
        private bool _nodeStateBound;

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
        }

        public override void OnDestroy()
        {
            if (Instance == this) Instance = null;
            base.OnDestroy();
        }

        // ── NetworkBehaviour 라이프사이클 ──────────────────────────────

        public override void OnNetworkSpawn()
        {
            _currentState.OnValueChanged += HandleBattleStateChanged;

            // 호스트만 NodeState 구독 시도. _mapData 가 아직 준비 안 됐을 수 있어 Update lazy bind 보강.
            if (IsServer)
            {
                TryBindNodeState();
            }

            DebugTool.Log(
                $"TeamBattleNetState spawned: IsServer={IsServer}, State={_currentState.Value}",
                DebugType.EconomyNet, this);
        }

        public override void OnNetworkDespawn()
        {
            _currentState.OnValueChanged -= HandleBattleStateChanged;
            UnbindNodeState();

            DebugTool.Log("TeamBattleNetState despawned", DebugType.EconomyNet, this);
        }

        private void Update()
        {
            // 호스트가 NodeState 늦게 준비되는 케이스 (씬 로드 직후 등) 대응 — idempotent
            if (IsServer && !_nodeStateBound && IsSpawned)
            {
                TryBindNodeState();
            }
        }

        // ── NodeState 구독 / 해제 (호스트 전용) ──────────────────────────

        private void TryBindNodeState()
        {
            if (_nodeStateBound) return;

            if (_mapData == null)
            {
                _mapData = FindFirstObjectByType<MapData>();
            }
            if (_mapData == null || _mapData.NetworkMapData == null) return;

            _mapData.NetworkMapData.NodeState.OnValueChanged += HandleNodeStateChanged;
            _nodeStateBound = true;

            // 초기 상태 1회 정규화 — 이미 NodeState 가 다른 값으로 진행 중일 수 있음
            NodeState initial = _mapData.NetworkMapData.NodeState.Value;
            BattleState mapped = MapNodeStateToBattleState(initial);
            if (_currentState.Value != mapped)
            {
                _currentState.Value = mapped;
            }

            DebugTool.Log(
                $"NodeState 구독 시작: initial={initial} → BattleState={mapped}",
                DebugType.EconomyNet, this);
        }

        private void UnbindNodeState()
        {
            if (!_nodeStateBound) return;
            if (_mapData != null && _mapData.NetworkMapData != null)
            {
                _mapData.NetworkMapData.NodeState.OnValueChanged -= HandleNodeStateChanged;
            }
            _nodeStateBound = false;
        }

        // ── 호스트: NodeState 변화를 BattleState 로 정규화 ───────────────

        private void HandleNodeStateChanged(NodeState previous, NodeState current)
        {
            if (!IsServer) return;

            BattleState mapped = MapNodeStateToBattleState(current);
            if (_currentState.Value == mapped) return;

            DebugTool.Log(
                $"NodeState: {previous} → {current} (BattleState: {_currentState.Value} → {mapped})",
                DebugType.EconomyNet, this);

            _currentState.Value = mapped;
        }

        private static BattleState MapNodeStateToBattleState(NodeState state)
        {
            switch (state)
            {
                case NodeState.Ready: return BattleState.Ready;
                case NodeState.Battle: return BattleState.Battle;
                case NodeState.Clear: return BattleState.Clear;
                default: return BattleState.Ended;
            }
        }

        // ── NetworkVariable 변경 → 정규화 이벤트 발화 (호스트+클라 공통) ─

        private void HandleBattleStateChanged(BattleState previous, BattleState current)
        {
            DebugTool.Log(
                $"BattleState 동기화: {previous} → {current}",
                DebugType.EconomyNet, this);

            switch (current)
            {
                case BattleState.Battle:
                    OnBattleStarted?.Invoke();
                    break;

                case BattleState.Clear:
                    OnBattleCleared?.Invoke();
                    break;

                case BattleState.Ready:
                case BattleState.Ended:
                    OnBattleEnded?.Invoke();
                    break;
            }
        }

        // ── 디버그/스크립트성 강제 변경 (호스트 전용) ──────────────────

        /// <summary>
        /// NodeState 와 무관하게 호스트에서 BattleState 를 강제 변경.
        /// 디버그 메뉴 / 시나리오 스크립트 / 통합 테스트 등에서 사용.
        /// 일반 게임 흐름에서는 호출하지 말 것 — NodeState 와 분리되면 노드 시스템 흐름이 깨질 수 있음.
        /// </summary>
        public void ServerSetBattleState(BattleState state, string reason)
        {
            if (!IsServer)
            {
                DebugTool.Warning($"ServerSetBattleState 호스트 전용 (state={state}, reason={reason})", DebugType.EconomyNet, this);
                return;
            }
            if (_currentState.Value == state) return;

            DebugTool.Log(
                $"BattleState 강제 변경: {_currentState.Value} → {state} (reason={reason})",
                DebugType.EconomyNet, this);

            _currentState.Value = state;
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestSetBattleStateServerRpc(BattleState state, ServerRpcParams rpcParams = default)
        {
            ServerSetBattleState(state, $"client_rpc:{rpcParams.Receive.SenderClientId}");
        }
    }
}
