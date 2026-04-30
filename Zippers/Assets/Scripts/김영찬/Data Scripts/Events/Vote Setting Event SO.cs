using System;
using UnityEngine;

/// <summary>
/// 플레이어가 텔레포트 하는 이벤트 세팅
/// </summary>
[CreateAssetMenu(fileName = "Vote Setting Event SO", menuName = "Node Data/Event Data/Vote Setting Event SO")]
public class VoteSettingEventSO : EventSO
{
    private MapRow _nextMapRow;
    
    public event Action OnVoteSettingComplete;
    
    public override void EventEnter()
    { 
        OnVoteSettingComplete += _controller.SetDefaultEvent;
        VoteSetting();
    }

    public override void EventUpdate()
    {
        
    }

    public override void EventExit()
    {
        OnVoteSettingComplete -= _controller.SetDefaultEvent;
    }
    
    private void VoteSetting()
    {
        _nextMapRow = _controller.Controller.Manager.NodePathMaker.Path.Dequeue();
        for (int i = 0; i < _nextMapRow.RowData.Length; i++)
        {
            if(_nextMapRow.RowData[i] != NodeType.Empty) 
            {
                _controller.Controller.Data.SetNextMap((NodeStartDir)i+1, _controller.Controller.Manager.DataContainer.GetRandomMap(_nextMapRow.RowData[i]));
            }
        }
        
        _controller.Controller.TeleportSupporter.EnableBeaconAvailable();
        
        DebugTool.Log($"{_controller.Controller.gameObject.name}Vote Setting Complete",DebugType.Node, this);
        OnVoteSettingComplete?.Invoke();
    }
}
