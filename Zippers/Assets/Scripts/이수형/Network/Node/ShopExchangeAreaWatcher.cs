using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Shop 노드의 ExchangeArea 트리거 진입/이탈을 감지하는 옵트인 컴포넌트.
///
/// 부착 위치:
///   - Shop 맵 프리팹의 ExchangeArea GameObject (이미 BoxCollider + ExchangeArea 가 있는 곳)
///   - 같은 GameObject 에 부착하면 동일 collider 이벤트를 같이 받음.
///
/// 동작:
///   - OnTriggerEnter / OnTriggerExit 에서 _unitLayer 검사
///   - NetworkObject.IsLocalPlayer 로 본인 캐릭터인지 식별 (다른 플레이어 진입은 무시)
///   - static 이벤트로 외부(UI 등)에 알림
///
/// 영찬님 영역의 ExchangeArea.cs 는 그대로 두고, 이 컴포넌트만 인스펙터에서 추가하면 동작.
///
/// 사용 예 (UI 측):
///     ShopExchangeAreaWatcher.OnLocalPlayerEntered += () =&gt; shopUI.Show();
///     ShopExchangeAreaWatcher.OnLocalPlayerExited  += () =&gt; shopUI.Hide();
/// </summary>
[RequireComponent(typeof(Collider))]
public class ShopExchangeAreaWatcher : MonoBehaviour
{
    [Header("플레이어 식별")]
    [Tooltip("ExchangeArea 와 동일한 레이어. 보통 'Unit' 레이어.")]
    [SerializeField] private LayerMask _unitLayer;

    /// <summary>본인(로컬 플레이어)이 ExchangeArea 에 진입했을 때 발화.</summary>
    public static event Action OnLocalPlayerEntered;

    /// <summary>본인(로컬 플레이어)이 ExchangeArea 에서 이탈했을 때 발화.</summary>
    public static event Action OnLocalPlayerExited;

    /// <summary>모든 플레이어(본인 + 원격) 진입 발화. 호스트 측 디버깅/카운팅용. 두 번째 인자 = clientId.</summary>
    public static event Action<ulong> OnAnyPlayerEntered;

    /// <summary>모든 플레이어 이탈 발화. 디버깅용.</summary>
    public static event Action<ulong> OnAnyPlayerExited;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsTargetLayer(other.gameObject)) return;

        NetworkObject no = other.GetComponentInParent<NetworkObject>();
        if (no == null) return;

        OnAnyPlayerEntered?.Invoke(no.OwnerClientId);
        DebugTool.Log($"[ShopExchangeArea] 진입 / clientId={no.OwnerClientId}, IsLocalPlayer={no.IsLocalPlayer}", DebugType.Node, this);

        if (no.IsLocalPlayer)
        {
            OnLocalPlayerEntered?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsTargetLayer(other.gameObject)) return;

        NetworkObject no = other.GetComponentInParent<NetworkObject>();
        if (no == null) return;

        OnAnyPlayerExited?.Invoke(no.OwnerClientId);
        DebugTool.Log($"[ShopExchangeArea] 이탈 / clientId={no.OwnerClientId}, IsLocalPlayer={no.IsLocalPlayer}", DebugType.Node, this);

        if (no.IsLocalPlayer)
        {
            OnLocalPlayerExited?.Invoke();
        }
    }

    private bool IsTargetLayer(GameObject go)
    {
        return ((1 << go.layer) & _unitLayer.value) != 0;
    }
}
