using System.Collections.Generic;
using UnityEngine;

public class DataAccessTest : MonoBehaviour
{
    // 여기서 생성하면 안됨! 반드시 Awake()나 Start() 일떄 또는 그 후에 발동되는 다른 함수에서 생성해야 함
    //PlayerClassDataSO cls = LocalDataAccess.Instance.Game.GetClass(10001); <-- X
    //ZombieStatSO stat = LocalDataAccess.Instance.Game.GetZombieStat(20001); <-- X
    //WaveInfoSO info = LocalDataAccess.Instance.Game.GetWaveInfo(30001); <-- X
    //List<WaveSpawnEntry> spawns = LocalDataAccess.Instance.Game.GetWaveSpawns(30001); <-- X
    PlayerClassDataSO cls;
    ZombieStatSO stat;
    List<WaveInfoSO> infos;
    List<WaveSpawnEntry> spawns;

    void Start()
    {
        cls = LocalDataAccess.Instance.Game.GetClass(10001);
        stat = LocalDataAccess.Instance.Game.GetZombieStat(20001);
        infos = LocalDataAccess.Instance.Game.GetWaveInfo(0); 
        spawns = LocalDataAccess.Instance.Game.GetWaveSpawns(30001);
    }

    public void CallPlayerClass()
    {
        Debug.Log(cls.WeaponType);

        Debug.Log(cls.MaxHealth);

        PlayerClassDataSO cls2 = LocalDataAccess.Instance.Game.GetClass(10002);

        Debug.Log(cls2.WeaponType);
        Debug.Log(cls2.MaxHealth);

    }
    public void CallZombieStat()
    {
        Debug.Log(stat.ZombieType);
        Debug.Log(stat.MaxHealth);
        ZombieStatSO stat2 = LocalDataAccess.Instance.Game.GetZombieStat(20002);
        Debug.Log(stat2.ZombieType);
        Debug.Log(stat2.MaxHealth);
    }

    public void CallWaveInfo()
    {
        Debug.Log($"BattleNode 0, WaveIndex 0: NextWaveDelay = {infos[0].NextWaveDelay}");
        Debug.Log($"BattleNode 0, WaveIndex 1: NextWaveDelay = {infos[1].NextWaveDelay}");

        List<WaveInfoSO> infos2 = LocalDataAccess.Instance.Game.GetWaveInfo(2);    // BattleNodeIndex 2
        Debug.Log($"BattleNode 2, WaveIndex 0: WaveId = {infos2[0].WaveId}");
        Debug.Log($"BattleNode 2, WaveIndex 1: WaveId = {infos2[1].WaveId}");
        Debug.Log($"BattleNode 2, WaveIndex 2: WaveId = {infos2[2].WaveId}");
    }

    public void CallWaveSpawn()
    {
        
        Debug.Log($"웨이브 인덱스: 30001, 그룹 인덱스 0, SpawnId = {spawns[0].SpawnId}");
        Debug.Log($"웨이브 인덱스: 30001, 그룹 인덱스 0, GroupIndex = {spawns[0].GroupIndex}");
        Debug.Log($"웨이브 인덱스: 30001, 그룹 인덱스 0, Count = {spawns[0].Count}");

        Debug.Log($"웨이브 인덱스: 30001, 그룹 인덱스 1, SpawnId = {spawns[1].SpawnId}");
        Debug.Log($"웨이브 인덱스: 30001, 그룹 인덱스 1, GroupIndex = {spawns[1].GroupIndex}");
        Debug.Log($"웨이브 인덱스: 30001, 그룹 인덱스 1, Count = {spawns[1].Count}");

        Debug.Log($"웨이브 인덱스: 30001, 그룹 인덱스 2, SpawnId = {spawns[2].SpawnId}");
        Debug.Log($"웨이브 인덱스: 30001, 그룹 인덱스 2, GroupIndex = {spawns[2].GroupIndex}");
        Debug.Log($"웨이브 인덱스: 30001, 그룹 인덱스 2, Count = {spawns[2].Count}");
        List<WaveSpawnEntry> spawns2 = LocalDataAccess.Instance.Game.GetWaveSpawns(30002);

        Debug.Log($"웨이브 인덱스: 30002, 그룹 인덱스 0, SpawnId = {spawns2[0].SpawnId}");
        Debug.Log($"웨이브 인덱스: 30002, 그룹 인덱스 0, GroupIndex = {spawns2[0].GroupIndex}");
        Debug.Log($"웨이브 인덱스: 30002, 그룹 인덱스 0, Count = {spawns2[0].Count}");

        Debug.Log($"웨이브 인덱스: 30002, 그룹 인덱스 1, SpawnId = {spawns2[1].SpawnId}");
        Debug.Log($"웨이브 인덱스: 30002, 그룹 인덱스 1, GroupIndex = {spawns2[1].GroupIndex}");
        Debug.Log($"웨이브 인덱스: 30002, 그룹 인덱스 1, Count = {spawns2[1].Count}");

    }



}
