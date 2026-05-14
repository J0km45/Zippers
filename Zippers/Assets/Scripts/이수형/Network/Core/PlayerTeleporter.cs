using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

/// <summary>
/// PlayerObject 의 NetworkTransform 을 호스트 명령으로 텔레포트 하기 위한 브릿지.
///
/// 배경: 4개 클래스 프리팹의 NetworkTransform 이 AuthorityMode = Owner 로 설정되어 있어
/// 호스트가 직접 nt.Teleport() 를 호출해도 다른 클라이언트가 owner 인 PlayerObject 에는
/// 적용되지 않음 (NGO 가 non-authority warning 후 무시). 결과적으로 호스트만 자기
/// PlayerObject 가 텔레포트되는 증상이 발생.
///
/// 해결: 호스트가 ClientRpc 로 owner 클라이언트에게 텔레포트 명령을 보내고,
/// owner 측에서 자기 NetworkTransform.Teleport 를 호출 (owner 권위로 정상 작동).
/// 결과적으로 owner 의 새 position 이 NetworkTransform 으로 모든 다른 클라에 sync.
///
/// 배치:
/// - 4개 플레이어 프리팹 (Melee/Rifle/Shotgun/Pistol) 루트에 부착
/// - 같은 GameObject 에 NetworkObject 와 NetworkTransform 이 있어야 함 (이미 부착됨)
/// </summary>
public class PlayerTeleporter : NetworkBehaviour
{
    private NetworkTransform _networkTransform;

    private void Awake()
    {
        _networkTransform = GetComponent<NetworkTransform>();
        if (_networkTransform == null)
        {
            DebugTool.Error("NetworkTransform 컴포넌트가 없음 - 같은 GameObject 에 부착 필요", DebugType.Network, this);
        }
    }

    /// <summary>
    /// 호스트가 호출하는 진입점. 이 PlayerObject 의 owner 클라이언트에게만
    /// targeted ClientRpc 를 보내 텔레포트 시킴.
    /// </summary>
    public void TeleportFromServer(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        if (!IsServer)
        {
            DebugTool.Warning("TeleportFromServer 거부: 호스트가 아님", DebugType.Network, this);
            return;
        }
        if (!IsSpawned)
        {
            DebugTool.Warning("TeleportFromServer 거부: NetworkObject spawn 안 됨", DebugType.Network, this);
            return;
        }

        TeleportClientRpc(position, rotation, scale, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { OwnerClientId }
            }
        });
    }

    /// <summary>
    /// Owner 클라이언트가 받아 자기 NetworkTransform 을 텔레포트.
    /// TargetClientIds 로 owner 만 수신하도록 호스트가 지정.
    /// </summary>
    [ClientRpc]
    private void TeleportClientRpc(Vector3 position, Quaternion rotation, Vector3 scale, ClientRpcParams rpcParams = default)
    {
        if (_networkTransform == null)
        {
            DebugTool.Error("NetworkTransform null - 텔레포트 불가", DebugType.Network, this);
            return;
        }

        // 본 클라이언트가 owner 이므로 NetworkTransform.Teleport 가 owner 권위로 정상 작동.
        // 변경된 position 은 NetworkTransform 의 sync 메커니즘으로 다른 모든 클라에 전파됨.
        _networkTransform.Teleport(position, rotation, scale);

        DebugTool.Log($"PlayerObject 텔레포트 완료: pos={position}", DebugType.Network, this);
    }
}
