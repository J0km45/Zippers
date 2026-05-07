using Unity.Collections;
using Unity.Netcode;

/// <summary>
/// 게임 씬에서 클라이언트별로 자동 spawn되는 PlayerObject에 부착되어 인게임 동기화 대상 데이터를 보관.
///
/// 현재는 닉네임 한 개만 NGO 동기화하지만, 다음 항목들이 추가될 받침대 역할을 의도:
/// - 팀(team) 식별자 - 팀전 모드 도입 시 팀 색상 / 매칭 / UI 표시에 활용
/// - 캐릭터 종류 / 외형 / 색상 - 게임 씬 진입 시 이 데이터 기반으로 시각적 분신 spawn
/// - level / avatar URL / 누적 스탯 - 외부 프로필(Cloud Save 등)에서 로드한 값을 NGO로 노출
/// - in-game 점수 - 호스트 권한이 더 적합하면 별도 SessionData로 분리하는 것도 검토
///
/// 새 항목 추가 시: NetworkVariable&lt;T&gt; 1개 + Owner의 OnNetworkSpawn에서 1회 write 가 기본 패턴.
/// 갱신 빈도가 잦거나 동기화 부하가 크면 RPC 또는 별도 매니저로 분리 검토
/// </summary>
public class PlayerData : NetworkBehaviour
{
    private readonly NetworkVariable<FixedString64Bytes> _playerName = new NetworkVariable<FixedString64Bytes>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    /// <summary>
    /// NGO 동기화된 플레이어 이름. Owner가 spawn 시 LobbyManager에서 한 번 미러링
    /// </summary>
    public string PlayerName => _playerName.Value.ToString();

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        // Lobby PlayerProperty의 이름을 in-game NetworkVariable로 한 번 미러링.
        // 이후 다른 클라들은 NetworkVariable 변경 이벤트로 자동 인지
        _playerName.Value = new FixedString64Bytes(LobbyManager.Instance.PlayerName);
    }
}
