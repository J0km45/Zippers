using UnityEngine;

/// <summary>
/// 플레이어가 텔레포트 하는 이벤트 세팅
/// </summary>
[CreateAssetMenu(fileName = "Vote Setting Event SO", menuName = "Node Data/Event Data/Vote Setting Event SO")]
public class VoteSettingEventSO : EventSO
{
    private MapRow _nextMapRow;
    
    public override void EventEnter()
    { 
        VoteSetting();
    }

    public override void EventUpdate()
    {
        
    }

    public override void EventExit()
    {
        
    }
    
    private void VoteSetting()
    {
        // 방어: Path 가 비어있으면 (정상 흐름에선 일어나지 않지만) crash 대신 경고 로그 + 기본 이벤트로 복귀.
        // 정상 흐름에선 MapController.WaitCoroutine 이 NodePathMaker.HasMadePath 를 대기하므로
        // 이 경로엔 들어오지 않음.
        var path = _controller.Controller.Manager.NodePathMaker?.Path;
        if (path == null || path.Count == 0)
        {
            DebugTool.Warning($"{_controller.Controller.gameObject.name} VoteSetting: Path 가 비어있음 - skip (NodeManager 초기화 타이밍 확인 필요)",
                              DebugType.Node, this);
            _controller.SetDefaultEvent();
            return;
        }

        _nextMapRow = path.Dequeue();
        for (int i = 0; i < _nextMapRow.RowData.Length; i++)
        {
            if(_nextMapRow.RowData[i] != NodeType.Empty)
            {
                _controller.Controller.Data.SetNextMap((NodeStartDir)i+1, _controller.Controller.Manager.DataContainer.GetRandomMap(_nextMapRow.RowData[i]));
            }
        }

        _controller.Controller.TeleportSupporter.EnableBeaconAvailable();

        DebugTool.Log($"{_controller.Controller.gameObject.name}Vote Setting Complete",DebugType.Node, this);
        _controller.SetDefaultEvent();
    }
}
