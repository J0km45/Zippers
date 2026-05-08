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
        
        // ToDo : _spawnManager에 스폰 포인트 지정하는 메서드 생성되면 삽입
        
        _controller.Controller.Manager.AddBattleCount();

        _waveManager.StartBattleNode(_controller.Controller.Manager.BattleCount);
    }
}
