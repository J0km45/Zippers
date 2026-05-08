using UnityEngine;

/// <summary>
/// 좀비가 스폰되도록 설정하는 이벤트
/// </summary>
[CreateAssetMenu(fileName = "MonsterSpawnEvent SO", menuName = "Node Data/Event Data/Monster Spawn Event SO")]
public class MonsterSpawnEventSO : EventSO
{
    private WaveManager _waveManager;
    private ZombieSpawnManager _spawnManager;
    
    
    public override void EventEnter()
    {
        Init();
        
        _controller.SetDefaultEvent();
    }

    public override void EventUpdate()
    {
        
    }

    public override void EventExit()
    {
        
    }

    private void Init()
    {
        _waveManager = FindFirstObjectByType<WaveManager>();
        _spawnManager = FindFirstObjectByType<ZombieSpawnManager>();
        
        _spawnManager.SetSpawnPoint(_controller.Controller.Data.MonsterSpawnPoints);
        _controller.Controller.Manager.AddBattleCount();
        _waveManager.StartBattleNode(_controller.Controller.Manager.BattleCount);
        
        DebugTool.Log($"{_controller.Controller.gameObject.name} Monster Spawner Linked And Spawn Start", DebugType.Node, this);
    }
}
