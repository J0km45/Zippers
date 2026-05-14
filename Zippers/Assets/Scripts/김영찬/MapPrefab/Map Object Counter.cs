using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 맵의 카운트 영역(BoxCollider) 안에 있는 플레이어 수를 호스트 권위로 추적해
/// MapData.NetworkMapData.AlivePlayerCount 에 반영.
///
/// 변경 사유 (2026-05-14):
///   기존엔 OnTriggerEnter/Exit 만으로 ±1 카운트했음.
///   문제: GameSpawnController 가 Start Map 의 spawn point 위치에 플레이어를 Instantiate 하면
///   이미 trigger 안쪽에 생성되어 OnTriggerEnter 가 누락되는 케이스가 발생함.
///   (Unity 의 trigger 이벤트는 "들어오는 모션" 기반이라 처음부터 안에 있으면 신뢰성 낮음.)
///   → AlivePlayerCount = 0 으로 유지 → TeleportSupporter.MinVoteWin = 0 →
///   한 명이 비콘에 가도 텔레포트 발화하는 버그.
///
/// 해결:
/// - HashSet&lt;int&gt; (Collider instanceID) 로 안에 있는 플레이어 추적.
/// - OnEnable 직후 코루틴에서 Physics.OverlapBox 로 기존에 안에 있는 플레이어 sweep → set 에 등록.
///   spawn/텔레포트가 한 두 프레임 늦을 수 있어 N 프레임 동안 재시도.
/// - OnTriggerEnter / OnTriggerExit 는 set 에 add/remove (HashSet 으로 중복 자연 차단).
/// - 카운트 갱신은 항상 set.Count 를 NetworkMapData.SetAlivePlayerCount 로 한 방에 박음 →
///   Plus/Minus 호출 누락/순서 의존 제거.
/// - OnDisable 에서 set 클리어 (맵 비활성화 시 깨끗이 리셋).
///   NetworkMapData 측 0 리셋은 MapController.ReadyForUse 가 책임 (기존 그대로).
///
/// 가정:
/// - 한 플레이어 = 하나의 unit 레이어 Collider. 만약 플레이어 프리팹이 unit 레이어 Collider 를
///   여러 개 가지면 중복 카운트됨 → 추적 키를 attachedRigidbody/NetworkObject 기반으로 바꿔야 함.
///   (현 시점 프리팹은 단일 collider 가정.)
/// </summary>
public class MapObjectCounter : MonoBehaviour
{
    [SerializeField] private BoxCollider _countArea;
    [SerializeField] private MapController _controller;
    [SerializeField] LayerMask _unitLayer;
    [SerializeField] bool isGizmoActive = true;

    [Header("Initial Sweep")]
    [Tooltip("OnEnable 직후 OverlapBox 스윕을 몇 프레임 동안 (재)시도할지. spawn/텔레포트가 한 두 프레임 늦을 수 있어 1~3 권장.")]
    [SerializeField] private int _initialSweepFrames = 3;

    // 호스트 측에서만 채워지는 추적 set. 키는 Collider 의 instanceID.
    private readonly HashSet<int> _trackedColliderIds = new HashSet<int>();

    // OverlapBox 결과 재사용 버퍼. 4명 + 약간 여유.
    private static readonly Collider[] _overlapBuffer = new Collider[16];

    private Coroutine _sweepRoutine;

    private void OnEnable()
    {
        // 호스트만 카운트 책임. 비호스트는 트리거 이벤트도 IsServer 가드로 무시되므로 sweep 도 불필요.
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;

        _sweepRoutine = StartCoroutine(InitialSweepCoroutine());
    }

    private void OnDisable()
    {
        if (_sweepRoutine != null)
        {
            StopCoroutine(_sweepRoutine);
            _sweepRoutine = null;
        }

        // 호스트만 set 클리어. NetworkMapData 측 0 리셋은 MapController.ReadyForUse 가 처리.
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            if (_trackedColliderIds.Count > 0)
            {
                DebugTool.Log(
                    $"{gameObject.name} ObjectCounter Disable - tracked clear ({_trackedColliderIds.Count}명)",
                    DebugType.Node, this);
                _trackedColliderIds.Clear();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;
        if (!IsInUnitLayer(other.gameObject.layer)) return;

        if (_trackedColliderIds.Add(other.GetInstanceID()))
        {
            SyncCount("TriggerEnter", other.gameObject.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;
        if (!IsInUnitLayer(other.gameObject.layer)) return;

        if (_trackedColliderIds.Remove(other.GetInstanceID()))
        {
            SyncCount("TriggerExit", other.gameObject.name);
        }
    }

    /// <summary>
    /// OnEnable 직후 N 프레임 동안 OverlapBox 로 안에 있는 플레이어를 강제로 잡는다.
    /// spawn/텔레포트가 OnEnable 보다 한 두 프레임 늦게 끝날 수 있어 여러 프레임 재시도.
    /// 이미 추적 중이면 HashSet 으로 중복 무시.
    /// </summary>
    private IEnumerator InitialSweepCoroutine()
    {
        int totalFrames = Mathf.Max(1, _initialSweepFrames);
        for (int i = 0; i < totalFrames; i++)
        {
            // 한 프레임 대기 후 sweep — 같은 프레임의 spawn 도 잡고 물리 갱신 후 OverlapBox 정확도 확보.
            yield return null;

            int added = SweepOnce();
            if (added > 0)
            {
                DebugTool.Log(
                    $"{gameObject.name} ObjectCounter Sweep[{i}] - {added}명 추가 (total {_trackedColliderIds.Count})",
                    DebugType.Node, this);
            }
        }

        _sweepRoutine = null;
    }

    /// <summary>
    /// _countArea 의 월드 영역으로 OverlapBox 스윕. unit 레이어인 collider 들을 set 에 등록.
    /// 카운트가 변하면 SyncCount 호출. 새로 추가된 collider 수를 반환.
    /// </summary>
    private int SweepOnce()
    {
        if (_countArea == null) return 0;

        // BoxCollider 의 center 는 로컬 좌표 → 월드 변환. size 는 로컬 크기 → lossyScale 로 월드 크기.
        Vector3 worldCenter = _countArea.transform.TransformPoint(_countArea.center);
        Vector3 halfExtents = Vector3.Scale(_countArea.size, _countArea.transform.lossyScale) * 0.5f;
        Quaternion orientation = _countArea.transform.rotation;

        // QueryTriggerInteraction.Collide: 플레이어 collider 가 trigger 든 non-trigger 든 잡기 위함.
        // _unitLayer 마스크로 _countArea 자체나 다른 collider 는 자연 제외.
        int hitCount = Physics.OverlapBoxNonAlloc(
            worldCenter, halfExtents, _overlapBuffer, orientation, _unitLayer.value, QueryTriggerInteraction.Collide);

        int newAdds = 0;
        for (int i = 0; i < hitCount; i++)
        {
            Collider c = _overlapBuffer[i];
            if (c == null) continue;
            // 마스크로 1차 필터됐지만 layerMask 와 layer 비트 비교 한 번 더 (defensive).
            if (!IsInUnitLayer(c.gameObject.layer)) continue;
            if (_trackedColliderIds.Add(c.GetInstanceID()))
            {
                newAdds++;
            }
        }

        if (newAdds > 0)
        {
            SyncCount("Sweep", $"+{newAdds}");
        }

        return newAdds;
    }

    private void SyncCount(string source, string detail)
    {
        // NetworkMapData 가 [0, 4] 클램프 + 동일값 skip.
        _controller.Data.NetworkMapData.SetAlivePlayerCount(_trackedColliderIds.Count);

        DebugTool.Log(
            $"{gameObject.name} ObjectCounter [{source}] {detail} → tracked={_trackedColliderIds.Count}",
            DebugType.Node, this);
    }

    /// <summary>
    /// LayerMask 와 layer 인덱스의 정확한 비트마스크 비교.
    /// 이전 코드 (other.gameObject.layer == _unitLayer) 는 int 와 bitmask 직접 비교라 항상 false 였음.
    /// </summary>
    private bool IsInUnitLayer(int layerIndex)
    {
        return (_unitLayer.value & (1 << layerIndex)) != 0;
    }

    private void OnDrawGizmos()
    {
        if(!isGizmoActive) return;
        if(_countArea == null) return;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(_countArea.center, _countArea.size);
    }
}
