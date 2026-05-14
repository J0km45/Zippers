using UnityEngine;

/// <summary>
/// Map 입장 시 모든 플레이어가 이동 되어있는지 체크하는 이벤트.
///
/// (2026-05-14) TODO 채움:
/// - 기존엔 AlivePlayerCount.OnValueChanged 가 한 번이라도 발화하면 즉시 Battle 로 전환 →
///   1명만 도착해도 전투 시작되는 결함.
/// - 변경: SessionPlayerStateController.ConnectedPlayerCount 와 비교하여 curCount 가
///   세션 접속 인원에 도달했을 때만 Battle 진입.
/// - 텔레포트 + 부활 시퀀스가 끝나면 ConnectedPlayerCount 만큼이 새 맵에 도착한다는 가정.
///
/// 추가: EventEnter 에서 즉시 1회 sync — subscribe 이전에 이미 인원이 가득 찬 경우
/// (Start Map 의 sweep 결과 등) OnValueChanged 가 안 와서 영원히 대기하는 데드락 방지.
/// </summary>
[CreateAssetMenu(fileName = "PlayerCheckEvent SO", menuName = "Node Data/Event Data/Player Check Event SO")]
public class PlayerCheckEventSO : EventSO
{
    public override void EventEnter()
    {
        NetworkMapData data = _controller.Controller.Data.NetworkMapData;
        data.AlivePlayerCount.OnValueChanged += PlayerCheck;

        // 초기 sync — 이미 모두 도착해 있을 수 있는 케이스 (Start Map 의 sweep 으로 4 가 미리 셋된 경우 등).
        // OnValueChanged 는 변동 시에만 발화하므로 명시적으로 한 번 호출.
        PlayerCheck(0, data.AlivePlayerCount.Value);
    }

    public override void EventUpdate()
    {

    }

    public override void EventExit()
    {
        _controller.Controller.Data.NetworkMapData.AlivePlayerCount.OnValueChanged -= PlayerCheck;
    }

    private void PlayerCheck(int preCount, int curCount)
    {
        // 세션 접속 인원만큼 도착하지 않았으면 대기.
        // SessionPlayerStateController 가 없으면 (씬 셋업 누락) 안전을 위해 기존 동작 (즉시 통과) 유지 + 에러 로그.
        SessionPlayerStateController session = SessionPlayerStateController.Instance;
        if (session != null)
        {
            int expected = session.ConnectedPlayerCount.Value;
            if (curCount < expected)
            {
                DebugTool.Log(
                    $"{_controller.Controller.gameObject.name} PlayerCheck 대기: {curCount}/{expected} 도착",
                    DebugType.Node, _controller.Controller);
                return;
            }

            DebugTool.Log(
                $"{_controller.Controller.gameObject.name} PlayerCheck 통과: {curCount}/{expected} → Battle 진입",
                DebugType.Node, _controller.Controller);
        }
        else
        {
            DebugTool.Error(
                $"{_controller.Controller.gameObject.name} SessionPlayerStateController.Instance 가 null - " +
                "PlayerCheck 비교 불가, 기존 동작(즉시 통과) 폴백. GameScene 에 컴포넌트 배치 확인.",
                DebugType.Node, _controller.Controller);
        }

        _controller.Controller.Data.NetworkMapData.SetNodeState(NodeState.Battle);
        _controller.SetDefaultEvent();
    }
}
