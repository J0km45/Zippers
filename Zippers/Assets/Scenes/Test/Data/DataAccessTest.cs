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
    ClassUpgradeData upgrade;
    TeamUpgradeEntry teamUpgrade;

    void Start()
    {
        cls = LocalDataAccess.Instance.Game.GetClass(10001);
        stat = LocalDataAccess.Instance.Game.GetZombieStat(20001);
        infos = LocalDataAccess.Instance.Game.GetWaveInfo(0);
        spawns = LocalDataAccess.Instance.Game.GetWaveSpawns(30001);
        upgrade = LocalDataAccess.Instance.Game.GetUpgrade("Melee");
        teamUpgrade = LocalDataAccess.Instance.Game.GetTeamUpgrade(51001);
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

    public void CallPlayerUpgrade()
    {
        // ─── 공통 stat 접근 (캐스팅 없이) ───
        Debug.Log($"[Melee] 클래스 타입: {upgrade.ClassType}");
        Debug.Log($"[Melee] MaxHealth: Id={upgrade.MaxHealth.Id}, Name={upgrade.MaxHealth.UpgradeName}, ValuePerLevel={upgrade.MaxHealth.ValuePerLevel}, IsEnabled={upgrade.MaxHealth.IsEnabled}");
        Debug.Log($"[Melee] Damage: ValuePerLevel={upgrade.Damage.ValuePerLevel}, BaseCost={upgrade.Damage.BaseCost}");
        Debug.Log($"[Melee] AttackSpeed: ValuePerLevel={upgrade.AttackSpeed.ValuePerLevel}, MaxLevel={upgrade.AttackSpeed.MaxLevel}");

        // ─── Melee 전용 stat 접근 (캐스팅 후) ───
        var melee = (MeleeUpgradeData)upgrade;
        Debug.Log($"[Melee] DamageReduction: ValuePerLevel={melee.DamageReduction.ValuePerLevel}, IsEnabled={melee.DamageReduction.IsEnabled}");
        Debug.Log($"[Melee] SprintSpeed: ValuePerLevel={melee.SprintSpeed.ValuePerLevel}");

        // ─── 다른 클래스 (Rifle) 즉시 조회 + 캐스팅 ───
        var rifle = (RifleUpgradeData)LocalDataAccess.Instance.Game.GetUpgrade("Rifle");
        Debug.Log($"[Rifle] Damage: ValuePerLevel={rifle.Damage.ValuePerLevel}");
        Debug.Log($"[Rifle] MagazineCapacity: ValuePerLevel={rifle.MagazineCapacity.ValuePerLevel}");
        Debug.Log($"[Rifle] PierceCount: ValuePerLevel={rifle.PierceCount.ValuePerLevel}, IsEnabled={rifle.PierceCount.IsEnabled}");

        // ─── WeaponType enum 오버로드 ───
        var pistol = (PistolUpgradeData)LocalDataAccess.Instance.Game.GetUpgrade(WeaponType.Pistol);
        Debug.Log($"[Pistol] SightRange: ValuePerLevel={pistol.SightRange.ValuePerLevel}");
        Debug.Log($"[Pistol] CollectRange: ValuePerLevel={pistol.CollectRange.ValuePerLevel}");
    }

    public void CallTeamUpgrade()
    {
        // ─── 캐시된 엔트리 (51001 의료 보급) 모든 필드 출력 ───
        Debug.Log($"[TeamUpgrade] {teamUpgrade.UpgradeId} - {teamUpgrade.UpgradeName} (MaxLv {teamUpgrade.MaxLevel})");
        Debug.Log($"[TeamUpgrade] StatKey={teamUpgrade.StatKey}, ApplyType={teamUpgrade.ApplyType}, ValuePerLevel={teamUpgrade.ValuePerLevel}");
        Debug.Log($"[TeamUpgrade] BaseCost={teamUpgrade.BaseCost}, CostIncrease={teamUpgrade.CostIncrease}");
        Debug.Log($"[TeamUpgrade] Description: {teamUpgrade.Description}");

        // ─── 즉시 조회로 다른 ID 두 개 추가 출력 ───
        var survivalTraining = LocalDataAccess.Instance.Game.GetTeamUpgrade(51005);
        Debug.Log($"[TeamUpgrade] {survivalTraining.UpgradeId} - {survivalTraining.UpgradeName}, StatKey={survivalTraining.StatKey}, ApplyType={survivalTraining.ApplyType}, ValuePerLevel={survivalTraining.ValuePerLevel}");
        Debug.Log($"[TeamUpgrade] Description: {survivalTraining.Description}");

        var mobilityTraining = LocalDataAccess.Instance.Game.GetTeamUpgrade(51009);
        Debug.Log($"[TeamUpgrade] {mobilityTraining.UpgradeId} - {mobilityTraining.UpgradeName}, StatKey={mobilityTraining.StatKey}, ApplyType={mobilityTraining.ApplyType}, ValuePerLevel={mobilityTraining.ValuePerLevel}");
        Debug.Log($"[TeamUpgrade] Description: {mobilityTraining.Description}");

        // ─── 전체 ID 순회 (GetAllTeamUpgradeIds 시연) ───
        Debug.Log("[TeamUpgrade] === 전체 ID 목록 ===");
        foreach (int id in LocalDataAccess.Instance.Game.GetAllTeamUpgradeIds())
        {
            var entry = LocalDataAccess.Instance.Game.GetTeamUpgrade(id);
            Debug.Log($"[TeamUpgrade] {id}: {entry.UpgradeName} ({entry.StatKey})");
        }
    }



}
