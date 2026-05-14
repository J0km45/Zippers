using System;
using Unity.Netcode;
using UnityEngine;
using Zippers.Network.Contracts;

namespace Zippers.Network
{
    /// <summary>
    /// 플레이어 한 명의 전투 상태(체력 / 사망 / 탄약 / 재장전 / 스테미나) 서버 권한 보관소.
    ///
    /// 소유: 최완용 (B · 전투 라인)
    /// 의존: IPlayerStatProvider (PlayerStats, F 소유) — MaxHealth / MaxStamina / MaxAmmo 캐시 갱신용
    /// 구현: IPlayerStatusReader, IPlayerCombatCommands
    ///
    /// 배치: 플레이어 프리팹에 부착 (NetworkObject 와 함께, PlayerEconomyNetState 와 같은 객체)
    ///
    /// ─────────────────────────────────────────────────────────────
    /// Day 0 단계 — 빈 스켈레톤. 향후 단계 진행 시 채울 항목:
    ///   Step 1: NetworkVariable&lt;float&gt; CurrentHealth, NetworkVariable&lt;bool&gt; IsDead 추가
    ///           PlayerHealth.TakeDamage / Heal / Die 를 ServerRpc / 내부 호출로 위임
    ///   Step 2: IPlayerStatusReader / IPlayerCombatCommands 의 실 구현 노출
    ///   Step 3: NetworkVariable&lt;float&gt; CurrentAmmo, NetworkVariable&lt;bool&gt; IsReloading 추가
    ///   Step 5: NetworkVariable&lt;float&gt; CurrentStamina 추가
    ///   Step 6: PlayerHealth.Die 시 SessionPlayerStateController.NotifyPlayerHPDeath 호출 연결
    ///   Step 7: IPlayerStatProvider.OnStatsRecalculated 구독 → MaxHealth/Ammo/Stamina 캐시 갱신
    /// ─────────────────────────────────────────────────────────────
    /// </summary>
    public class PlayerCombatNetState : NetworkBehaviour, IPlayerStatusReader, IPlayerCombatCommands
    {
        // ── IPlayerStatusReader 구현 (TODO) ────────────────────────────────
        // NetworkBehaviour.OwnerClientId 가 IPlayerStatusReader.OwnerClientId 자동 만족.

        // TODO Step 1: NetworkVariable<float> _currentHealth, NetworkVariable<bool> _isDead 로 교체
        public float CurrentHealth => 0f;
        public float MaxHealth => 0f;          // TODO Step 7: IPlayerStatProvider.TotalMaxHealth 캐시
        public bool IsDead => false;
        // TODO Step 5
        public float CurrentStamina => 0f;
        // TODO Step 3
        public float CurrentAmmo => 0f;
        public bool IsReloading => false;

        // 이벤트 — Step 1~5 에서 NetworkVariable.OnValueChanged 콜백을 통해 발화
#pragma warning disable 0067 // 빈 스켈레톤이라 발화 코드 아직 없음. Step 진행 시 제거.
        public event Action<float, float> OnHealthChanged;
        public event Action OnPlayerDied;
        public event Action OnPlayerRevived;
#pragma warning restore 0067

        // ── IPlayerCombatCommands 구현 (TODO Step 1~6) ─────────────────────
        public void ServerApplyHeal(float amount, string source)
        {
            // TODO Step 1: IsServer 가드 + IsDead 가드 + CurrentHealth 증가 + 이벤트
            DebugTool.Log($"[PlayerCombatNetState] ServerApplyHeal stub: amount={amount}, source={source}", DebugType.CombatNet, this);
        }

        public void ServerApplyDamage(float damage, ulong attackerClientId, string source)
        {
            // TODO Step 1: IsServer 가드 + 데미지 적용 + 사망 처리
            DebugTool.Log($"[PlayerCombatNetState] ServerApplyDamage stub: damage={damage}, attacker={attackerClientId}, source={source}", DebugType.CombatNet, this);
        }

        public void ServerKill(string reason)
        {
            // TODO Step 1: IsServer 가드 + 강제 사망 처리
            DebugTool.Log($"[PlayerCombatNetState] ServerKill stub: reason={reason}", DebugType.CombatNet, this);
        }

        public void ServerRevive(float healthRatio)
        {
            // TODO Step 1: IsServer 가드 + 부활 처리
            DebugTool.Log($"[PlayerCombatNetState] ServerRevive stub: healthRatio={healthRatio}", DebugType.CombatNet, this);
        }

        // ── NetworkBehaviour 라이프사이클 ──────────────────────────────────
        public override void OnNetworkSpawn()
        {
            DebugTool.Log(
                $"PlayerCombatNetState spawned: ownerClientId={OwnerClientId}, IsServer={IsServer}, IsOwner={IsOwner}",
                DebugType.CombatNet, this);
        }

        public override void OnNetworkDespawn()
        {
            DebugTool.Log(
                $"PlayerCombatNetState despawned: ownerClientId={OwnerClientId}",
                DebugType.CombatNet, this);
        }
    }
}
