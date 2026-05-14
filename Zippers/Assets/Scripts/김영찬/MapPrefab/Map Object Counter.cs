using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 맵 안의 카운트 영역(BoxCollider)에 플레이어 진입/이탈을 감지해
/// MapData.NetworkMapData.AlivePlayerCount 를 갱신.
/// 트리거 처리는 모든 클라가 자기 물리로 수행하지만, NetworkVariable 쓰기는
/// 호스트만 권위 — IsServer 가드로 비호스트는 즉시 return.
/// 플레이어 PlayerObject 가 destroy 되면 Unity 가 OnTriggerExit 를 발화해주므로
/// disconnect → NGO 자동 despawn → 자동 minus 의 흐름으로 사망 처리 완성.
/// </summary>
public class MapObjectCounter : MonoBehaviour
{
    [SerializeField] private BoxCollider _countArea;
    [SerializeField] private MapController _controller;
    [SerializeField] LayerMask _unitLayer;
    [SerializeField] bool isGizmoActive = true;

    private void OnTriggerEnter(Collider other)
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;
        if (!IsInUnitLayer(other.gameObject.layer)) return;
        _controller.Data.NetworkMapData.PlusAlivePlayerCount();
    }

    private void OnTriggerExit(Collider other)
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;
        if (!IsInUnitLayer(other.gameObject.layer)) return;
        _controller.Data.NetworkMapData.MinusAlivePlayerCount();
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
