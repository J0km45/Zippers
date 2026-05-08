using TMPro;
using UnityEngine;

/// <summary>
/// DataLoadScene 의 UI 컨트롤러.
/// LocalDataAccess.Instance.Game.OnReady 구독 후 발화 시 RoomListScene 으로 전환.
///
/// 구독 시점은 Start() — 같은 씬의 모든 Awake 가 끝난 시점이라 LocalDataAccess.Instance 가 보장됨.
/// GameDataModule.OnReady 의 add 핸들러는 이미 IsReady 면 즉시 invoke 하므로,
/// 시트 로드가 구독보다 먼저 끝난 경우(캐시 등)도 race-free 하게 catch-up 됨.
///
/// DataManager / LocalDataAccess / GameDataModule 코드는 손대지 않음.
/// </summary>
public class DataLoadController : MonoBehaviour
{
    [SerializeField] private TMP_Text _statusText;

    private bool _isTransitioning;
    private bool _isSubscribed;

    private void Start()
    {
        if (LocalDataAccess.Instance == null)
        {
            DebugTool.Error(
                "LocalDataAccess.Instance 가 null - DataLoadScene 에 LocalDataAccess GameObject 가 없거나 Awake 순서 문제",
                DebugType.Data, this);
            SetStatus("데이터 시스템 미초기화. 씬 구성을 확인하세요.");
            return;
        }

        SetStatus("데이터 로딩 중...");
        LocalDataAccess.Instance.Game.OnReady += OnDataReady;
        _isSubscribed = true;
        // 위 add 핸들러가 이미 IsReady 면 즉시 OnDataReady 를 호출하므로 추가 폴링 불필요.
    }

    private void OnDestroy()
    {
        // LocalDataAccess 가 DontDestroyOnLoad 라 씬 전환 시 살아있고, 이 컨트롤러만 사라짐.
        // 누수 방지를 위해 안전하게 해제.
        if (_isSubscribed && LocalDataAccess.Instance != null)
        {
            LocalDataAccess.Instance.Game.OnReady -= OnDataReady;
        }
        _isSubscribed = false;
    }

    private void OnDataReady()
    {
        if (_isTransitioning) return;
        _isTransitioning = true;

        DebugTool.Log("데이터 준비 완료 - RoomListScene 으로 전환", DebugType.Network, this);
        SetStatus("완료. 방 목록으로 이동...");
        SceneLoader.LoadLocal(SceneId.RoomList);
    }

    private void SetStatus(string message)
    {
        if (_statusText != null) _statusText.text = message;
    }
}
