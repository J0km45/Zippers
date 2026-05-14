using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Phase E: 투표함 트리거 디텍터.
/// 더 이상 자체 카운트(_votePlayer) 보유 안 함. 단순히 호스트의 OnTrigger 콜백을
/// TeleportSupporter 에 전달하는 host-only forwarder.
///
/// 카운트 권위는 TeleportSupporter 의 NetworkVariable&lt;int&gt; 4개에 있음 — 모든 클라가
/// 동일한 카운트를 보고, UI 가 OnValueChanged 로 갱신.
///
/// 호스트의 PhysX 가 NetworkTransform 으로 동기화된 모든 PlayerObject 의 위치를 보므로
/// 호스트의 OnTriggerEnter/Exit 한 곳에서 판정하면 충분 (ServerRpc 불필요).
/// </summary>
public class TeleportBallotBox : MonoBehaviour
{
    [Header("References")]
    [Tooltip("이 투표함이 속한 TeleportSupporter (보통 부모 GameObject). 미할당 시 GetComponentInParent 로 자동 탐색.")]
    [SerializeField] private TeleportSupporter _supporter;

    [Header("Settings")]
    [Tooltip("이 투표함이 대표하는 텔레포트 방향. _supporter 의 어느 NetworkVariable 을 갱신할지 결정.")]
    [SerializeField] private NodeStartDir _direction;

    private void Awake()
    {
        if (_supporter == null)
        {
            _supporter = GetComponentInParent<TeleportSupporter>();
            if (_supporter == null)
            {
                DebugTool.Error("TeleportSupporter 를 찾을 수 없음 - 부모 계층 확인", DebugType.Node, this);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // host-only 처리. 비호스트는 즉시 return.
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;
        if (_supporter == null) return;
        if (!TryGetClientId(other, out ulong clientId)) return;
        _supporter.OnPlayerEnterBoxServer(clientId, _direction);
    }

    private void OnTriggerExit(Collider other)
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;
        if (_supporter == null) return;
        if (!TryGetClientId(other, out ulong clientId)) return;
        _supporter.OnPlayerExitBoxServer(clientId, _direction);
    }

    /// <summary>
    /// 트리거를 발생시킨 Collider 의 NetworkObject 에서 OwnerClientId 추출.
    /// 플레이어가 아닌 오브젝트(몬스터 등) 가 트리거하면 false 반환 → 카운트 안 됨.
    /// </summary>
    private bool TryGetClientId(Collider other, out ulong clientId)
    {
        NetworkObject no = other.GetComponentInParent<NetworkObject>();
        if (no != null && no.IsPlayerObject)
        {
            clientId = no.OwnerClientId;
            return true;
        }
        clientId = 0;
        return false;
    }
}
