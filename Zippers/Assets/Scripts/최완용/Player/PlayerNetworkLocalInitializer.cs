using Unity.Netcode;
using UnityEngine;

// 네트워크에서 로컬 플레이어 전용 기능을 초기화한다.
// 내 캐릭터만 마우스 회전, 조준 마커, 테스트 입력을 사용한다.
// 카메라 Target 연결은 QuarterViewCamera에서 담당한다.
public class PlayerNetworkLocalInitializer : NetworkBehaviour
{
    [Header("로컬 전용 컴포넌트")]
    [SerializeField] private PlayerLook _playerLook;
    [SerializeField] private PlayerAimMarker _playerAimMarker;
    [SerializeField] private PlayerUpgradeKeyInputTest _playerUpgradeKeyInputTest;

    private void Awake()
    {
        if (_playerLook == null)
        {
            _playerLook = GetComponent<PlayerLook>();
        }

        if (_playerAimMarker == null)
        {
            _playerAimMarker = GetComponent<PlayerAimMarker>();
        }

        if (_playerUpgradeKeyInputTest == null)
        {
            _playerUpgradeKeyInputTest = GetComponent<PlayerUpgradeKeyInputTest>();
        }
    }

    private void Start()
    {
        // 네트워크 없이 싱글 테스트할 때는 기존처럼 로컬 기능을 사용한다.
        if (!IsNetworkGameRunning())
        {
            SetLocalComponentsActive(true);
            DebugTool.Log("[PlayerNetworkLocalInitializer] 싱글 테스트 로컬 기능 활성화", DebugType.Network, this);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner || IsLocalPlayer)
        {
            EnableLocalPlayerObjects();
            return;
        }

        DisableRemotePlayerObjects();
    }

    public override void OnNetworkDespawn()
    {
        DisableRemotePlayerObjects();
    }

    private void EnableLocalPlayerObjects()
    {
        SetLocalComponentsActive(true);

        DebugTool.Log("[PlayerNetworkLocalInitializer] 로컬 플레이어 기능 활성화", DebugType.Network, this);
    }

    private void DisableRemotePlayerObjects()
    {
        SetLocalComponentsActive(false);

        DebugTool.Log("[PlayerNetworkLocalInitializer] 원격 플레이어 로컬 기능 비활성화", DebugType.Network, this);
    }

    private void SetLocalComponentsActive(bool isActive)
    {
        if (_playerLook != null)
        {
            _playerLook.enabled = isActive;
        }

        if (_playerAimMarker != null)
        {
            _playerAimMarker.enabled = isActive;
        }

        if (_playerUpgradeKeyInputTest != null)
        {
            _playerUpgradeKeyInputTest.enabled = isActive;
        }
    }

    private bool IsNetworkGameRunning()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
    }
}