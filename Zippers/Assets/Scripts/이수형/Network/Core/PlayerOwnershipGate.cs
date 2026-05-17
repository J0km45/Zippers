using System.Text;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 플레이어 프리팹 루트에 부착되는 ownership 분리 컴포넌트.
///
/// 책임:
/// - 원격 플레이어(IsOwner=false)에선 입력/카메라 컴포넌트 disable.
/// - 본인 플레이어(IsOwner=true)에서만 활성화.
/// - "안 그러면 1번 유저가 움직일 때 4명이 다 같이 움직임" (마이그레이션 PDF Phase 1) 방지.
///
/// 동작 (Strategy 2 - one-frame leak 방지):
/// - Awake: 인스펙터에 등록된 모든 owner-only 컴포넌트/GameObject 를 일단 disable.
///   (이 시점엔 IsOwner 판단 불가 → 보수적으로 차단)
/// - OnNetworkSpawn: IsOwner==true 일 때만 다시 enable. 원격은 disable 유지.
///
/// 이렇게 하면 OnNetworkSpawn 호출 전에 한 프레임이라도 입력 Update 가 도는 일이 없음.
///
/// 인스펙터 설정 가이드 (player_control PDF 기준):
/// - _ownerOnlyBehaviours: 입력/로컬 전용 MonoBehaviour 들
///     PlayerController, PlayerMovement, PlayerLook,
///     PlayerAim, PlayerAimCal, PlayerCombat, PlayerReload,
///     ObstacleFadeTarget, PlayerUpgradeKeyInputTest 등
/// - _ownerOnlyGameObjects: 통째로 끌 GameObject 들
///     QuarterViewCamera 가 붙은 카메라 GameObject 등
///
/// 켜둬야 할 컴포넌트(PlayerAnimation / PlayerHealth 피격 연출 / 메시 / 이펙트)는
/// 인스펙터 배열에 안 넣으면 자동으로 활성 유지됨.
///
/// 주의:
/// - 이 컴포넌트는 NGO 로 spawn 된 인스턴스 전용. 씬에 raw 로 드롭하면 Awake 후 영원히 disable 유지됨.
/// - Phase 후반에 서버 권위 분리(PlayerHealth 등이 서버에서 데이터 변경)가 진행되면
///   일부 컴포넌트가 OFF 대상에서 빠지거나 새로 추가될 수 있음 - 인스펙터 재조정만 하면 됨.
/// </summary>
public class PlayerOwnershipGate : NetworkBehaviour
{
    [Header("Owner-Only Components")]
    [Tooltip("원격 플레이어에선 .enabled=false, 본인 플레이어에선 OnNetworkSpawn 시 .enabled=true.\n" +
             "예: PlayerController, PlayerMovement, PlayerAim, PlayerCombat 등 입력 관련 MonoBehaviour")]
    [SerializeField] private Behaviour[] _ownerOnlyBehaviours;

    [Header("Owner-Only GameObjects")]
    [Tooltip("원격 플레이어에선 SetActive(false), 본인 플레이어에선 SetActive(true).\n" +
             "예: 카메라 GameObject(QuarterViewCamera 가 붙어있는 오브젝트)")]
    [SerializeField] private GameObject[] _ownerOnlyGameObjects;

    [Header("Owner Or Server Components")]
    [Tooltip("소유자 클라이언트 또는 서버에서 켜져야 하는 컴포넌트입니다. 예: PlayerCombat, PlayerReload")]
    [SerializeField] private Behaviour[] _ownerOrServerBehaviours;

    // ─────────────────────────────────────────────────────────────────
    // Lifecycle
    // ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        // OnNetworkSpawn 이 Awake 보다 늦게 호출됨. IsOwner 판단 전에 보수적으로 다 끔.
        // 이렇게 안 하면 한 프레임 동안 원격 플레이어의 입력 컴포넌트가 살아있어
        // 내 입력이 남의 캐릭터로 새는 1-frame leak 가능.
        ApplyOwnership(false);
        ApplyOwnerOrServer(false);
    }

    public override void OnNetworkSpawn()
    {
        // IsOwner == true 면 다시 켜고, false 면 Awake 에서 끈 상태 유지.
        ApplyOwnership(IsOwner);
        ApplyOwnerOrServer(IsOwner || IsServer);

        DebugTool.Log(
            $"OwnershipGate 적용: IsOwner={IsOwner}, OwnerClientId={OwnerClientId}",
            DebugType.Network, this);

        // 로컬 플레이어인 경우에만 씬의 QuarterViewCamera 가 나를 추적하도록 바인딩.
        // 원격 플레이어는 카메라가 따라가면 안 되므로 아무 것도 안 함.
        if (IsOwner)
        {
            BindSceneCameraToSelf();
        }
    }

    public override void OnNetworkDespawn()
    {
        // 로컬 플레이어가 사라질 때 카메라가 빈 Transform 을 참조하지 않도록 정리.
        if (IsOwner)
        {
            UnbindSceneCameraIfMine();
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // Internal
    // ─────────────────────────────────────────────────────────────────

    private void BindSceneCameraToSelf()
    {
        QuarterViewCamera cam = FindFirstObjectByType<QuarterViewCamera>();
        if (cam == null)
        {
            DebugTool.Warning(
                "QuarterViewCamera 를 씬에서 찾을 수 없음 - 카메라 바인딩 skip (GameScene MainCamera 부착 확인)",
                DebugType.Network, this);
            return;
        }

        cam.Target = transform;

        // 같은 플레이어 프리팹에 있는 PlayerAim 을 자동 찾아 카메라에 연결.
        // 없으면 PlayerAim 은 그대로 두기 (조준 카메라 오프셋만 동작 안 함, 추적은 정상).
        PlayerAim aim = GetComponent<PlayerAim>();
        if (aim != null)
        {
            cam.PlayerAim = aim;
        }
        else
        {
            DebugTool.Warning(
                "PlayerAim 컴포넌트가 같은 프리팹에 없음 - cam.PlayerAim 미갱신",
                DebugType.Network, this);
        }

        DebugTool.Log(
            $"카메라 바인딩 완료: cam.Target={transform.name}, cam.PlayerAim={(aim != null ? aim.name : "null")}",
            DebugType.Network, this);
    }

    private void UnbindSceneCameraIfMine()
    {
        QuarterViewCamera cam = FindFirstObjectByType<QuarterViewCamera>();
        if (cam == null) return;

        // 다른 객체로 이미 옮겨갔다면 건드리지 않음 (마지막 로컬 플레이어 종료 시점에만 정리).
        if (cam.Target == transform)
        {
            cam.Target = null;
            cam.PlayerAim = null;
            DebugTool.Log("카메라 바인딩 해제 (로컬 플레이어 despawn)", DebugType.Network, this);
        }
    }

    private void ApplyOwnership(bool isOwner)
    {
        if (_ownerOnlyBehaviours != null)
        {
            for (int i = 0; i < _ownerOnlyBehaviours.Length; i++)
            {
                Behaviour b = _ownerOnlyBehaviours[i];
                if (b == null) continue;
                b.enabled = isOwner;
            }
        }

        if (_ownerOnlyGameObjects != null)
        {
            for (int i = 0; i < _ownerOnlyGameObjects.Length; i++)
            {
                GameObject go = _ownerOnlyGameObjects[i];
                if (go == null) continue;
                go.SetActive(isOwner);
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // Debug helpers
    // ─────────────────────────────────────────────────────────────────

    [ContextMenu("Dump Ownership State")]
    private void DumpOwnershipState()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"PlayerOwnershipGate: IsSpawned={IsSpawned}, IsOwner={IsOwner}, OwnerClientId={OwnerClientId}");

        int bCount = _ownerOnlyBehaviours?.Length ?? 0;
        sb.AppendLine($"  Behaviours ({bCount}):");
        for (int i = 0; i < bCount; i++)
        {
            Behaviour b = _ownerOnlyBehaviours[i];
            string desc = b == null ? "null" : $"{b.GetType().Name} on {b.gameObject.name} (enabled={b.enabled})";
            sb.AppendLine($"    [{i}] {desc}");
        }

        int gCount = _ownerOnlyGameObjects?.Length ?? 0;
        sb.AppendLine($"  GameObjects ({gCount}):");
        for (int i = 0; i < gCount; i++)
        {
            GameObject go = _ownerOnlyGameObjects[i];
            string desc = go == null ? "null" : $"{go.name} (activeSelf={go.activeSelf})";
            sb.AppendLine($"    [{i}] {desc}");
        }

        DebugTool.Log(sb.ToString(), DebugType.Network, this);
    }
    private void ApplyOwnerOrServer(bool isEnabled)
    {
        if (_ownerOrServerBehaviours == null)
        {
            return;
        }

        for (int i = 0; i < _ownerOrServerBehaviours.Length; i++)
        {
            Behaviour behaviour = _ownerOrServerBehaviours[i];

            if (behaviour == null)
            {
                continue;
            }

            behaviour.enabled = isEnabled;
        }
    }
}
