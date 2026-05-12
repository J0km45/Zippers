using System;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// [TEST ONLY] UI 없이 GameScene 까지 단독 호스트 흐름을 자동 실행하는 부트스트랩.
///
/// 배경:
/// - LobbyScene / RoomListScene UI 가 아직 미완성이라 정식 흐름으로 GameScene 진입 불가.
/// - UI 가 호출하던 LobbyManager / AuthService / SceneLoader 메서드를 코드로 직접 호출해 우회.
///
/// 사용법:
/// 1) AuthScene 에 빈 GameObject 만들고 이 컴포넌트 부착.
/// 2) 인스펙터에서 _testClass 선택 (Melee/Rifle/Shotgun/Pistol).
/// 3) _autoStartOnPlay 켜고 Play → 자동 실행.
///    또는 컴포넌트 우클릭 [Start Solo Host Test] ContextMenu 로 수동 실행.
///
/// 자동 실행 흐름:
/// 1) AuthService.InitializeAsync      (UGS Core + 익명 로그인)
/// 2) LobbyManager.SetLocalPlayerInfo  (테스트 클래스 + Ready=true)
/// 3) LobbyManager.CreateSessionAsync  (호스트 단독 세션 생성, NGO host start, Relay 연결)
/// 4) LobbyManager.WaitForOwnSlotAsync (호스트 본인 슬롯 부여 대기)
/// 5) SceneLoader.LoadNetworked        (NGO 동기화 GameScene 로드)
///
/// 검증되는 것:
/// - NGO host start + ConnectionApproval → PlayerSessionBridge 매핑
/// - LobbyManager 세션 + SessionProperty Slots 부여
/// - GameSpawnController.SpawnOne (호스트 자신만 spawn)
/// - PlayerOwnershipGate (IsOwner=true 분기), QuarterViewCamera 바인딩
///
/// 검증 안 되는 것 (멀티 클라이언트 필요):
/// - IsOwner=false 분기 (원격 플레이어 입력/카메라 차단 확인)
/// - 클래스 중복 검증, 슬롯 충돌, 재진입
///
/// 주의:
/// - 정식 빌드/플레이 직전엔 반드시 _autoStartOnPlay = false 또는 이 GameObject 비활성/제거.
/// - 컴포넌트 자체는 Test 폴더에 격리되어 있으니 PR 누락 시 코드 자체는 빌드 포함되어도 무해.
/// </summary>
public class TestSoloHostBootstrap : MonoBehaviour
{
    [Header("Test Settings")]
    [Tooltip("호스트가 사용할 클래스. None 이면 GameSpawnController 가드에 걸려 스폰 skip 됨.")]
    [SerializeField] private PlayerClass _testClass = PlayerClass.Melee;

    [Tooltip("Play 누르면 자동 실행. ⚠ 정식 빌드/플레이 시엔 반드시 OFF.")]
    [SerializeField] private bool _autoStartOnPlay = false;

    [Tooltip("다른 매니저(LobbyManager 등)의 Awake/Start 가 끝날 시간을 벌어주는 지연 (초).")]
    [SerializeField, Min(0f)] private float _delayBeforeStart = 0.5f;

    private bool _isRunning;

    // ─────────────────────────────────────────────────────────────────
    // Entry points
    // ─────────────────────────────────────────────────────────────────

    private async void Start()
    {
        if (!_autoStartOnPlay) return;
        await RunSoloHost();
    }

    [ContextMenu("Start Solo Host Test")]
    private async void StartFromContextMenu()
    {
        if (!Application.isPlaying)
        {
            DebugTool.Warning("Play 모드가 아님 - ContextMenu 무시", DebugType.Network, this);
            return;
        }
        await RunSoloHost();
    }

    // ─────────────────────────────────────────────────────────────────
    // Main flow
    // ─────────────────────────────────────────────────────────────────

    private async Task RunSoloHost()
    {
        if (_isRunning)
        {
            DebugTool.Warning("이미 실행 중 - 중복 호출 무시", DebugType.Network, this);
            return;
        }
        _isRunning = true;

        try
        {
            DebugTool.Log("=== Solo Host 테스트 시작 ===", DebugType.Network, this);

            if (_delayBeforeStart > 0f)
            {
                await Task.Delay((int)(_delayBeforeStart * 1000));
            }

            // Preflight checks - 흔한 설정 누락 미리 잡기
            if (!RunPreflightChecks()) return;

            // 1) Auth 초기화 + 익명 로그인
            DebugTool.Log("[1/5] Auth 초기화 + 익명 로그인", DebugType.Network, this);
            await AuthService.InitializeAsync();

            // 2) 로컬 PlayerInfo 세팅
            //    SlotIndex = -1 (호스트가 SessionProperty 에 직접 0번 배정).
            //    IsReady = true (TryStartGameAsHostAsync 의 AreNonHostPlayersReady 게이트는 안 거치므로
            //      이 값은 정보용. SceneLoader.LoadNetworked 는 직접 호출하니까).
            DebugTool.Log($"[2/5] SetLocalPlayerInfo: class={_testClass}, ready=true", DebugType.Network, this);
            LobbyManager.Instance.SetLocalPlayerInfo(new PlayerInfo(-1, _testClass, true));

            // 3) 세션 생성 (호스트 단독). Relay + NGO host start 가 내부에서 함께 발생.
            string roomName = $"TestSolo_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
            DebugTool.Log($"[3/5] CreateSessionAsync: roomName={roomName}", DebugType.Network, this);
            bool created = await LobbyManager.Instance.CreateSessionAsync(roomName);
            if (!created)
            {
                DebugTool.Error("세션 생성 실패 - 중단", DebugType.Network, this);
                return;
            }

            // 4) 호스트 자신 슬롯 부여 대기 (보통 1초 미만)
            DebugTool.Log("[4/5] WaitForOwnSlotAsync (최대 5초)", DebugType.Network, this);
            int slot = await LobbyManager.Instance.WaitForOwnSlotAsync(5f);
            if (slot < 0)
            {
                DebugTool.Error("호스트 슬롯 부여 타임아웃 - 중단", DebugType.Network, this);
                return;
            }
            DebugTool.Log($"슬롯 부여 완료: SlotIndex={slot}", DebugType.Network, this);

            // 5) GameScene 동기화 로드 (NGO 가 호스트 + 클라이언트 전원 자동 전파)
            DebugTool.Log("[5/5] SceneLoader.LoadNetworked(SceneId.Game)", DebugType.Network, this);
            if (!SceneLoader.LoadNetworked(SceneId.Game))
            {
                DebugTool.Error("GameScene 로드 요청 실패", DebugType.Network, this);
                return;
            }

            DebugTool.Log("=== Solo Host 부트스트랩 완료. GameScene 로드 진행 중. 이후 GameSpawnController 로그 확인. ===",
                DebugType.Network, this);
        }
        catch (Exception e)
        {
            DebugTool.Error($"Solo Host 테스트 예외: {e.Message}\n{e.StackTrace}", DebugType.Network, this);
        }
        finally
        {
            _isRunning = false;
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // Preflight
    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// 흔한 설정 누락을 미리 진단. false 면 RunSoloHost 중단.
    /// </summary>
    private bool RunPreflightChecks()
    {
        bool ok = true;

        if (_testClass == PlayerClass.None)
        {
            DebugTool.Error("_testClass = None - GameSpawnController 가드에 걸려 스폰 skip 됨. " +
                            "Melee/Rifle/Shotgun/Pistol 중 선택", DebugType.Network, this);
            ok = false;
        }

        if (LobbyManager.Instance == null)
        {
            DebugTool.Error("LobbyManager.Instance 없음 - AuthScene 에 LobbyManager 배치 필요", DebugType.Network, this);
            ok = false;
        }

        if (PlayerSessionBridge.Instance == null)
        {
            DebugTool.Error("PlayerSessionBridge.Instance 없음 - AuthScene 에 PlayerSessionBridge 배치 필요", DebugType.Network, this);
            ok = false;
        }

        if (Unity.Netcode.NetworkManager.Singleton == null)
        {
            DebugTool.Error("NetworkManager.Singleton 없음 - 씬에 NetworkManager 배치 필요", DebugType.Network, this);
            ok = false;
        }

        return ok;
    }
}
