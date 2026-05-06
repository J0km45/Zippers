using System;
using System.Collections.Generic;
using System.Linq;


public class GameDataModule
{

    private Dictionary<int, PlayerClassDataSO> _classes;
    private Dictionary<int, ZombieStatSO>      _zombieStats;
    private Dictionary<int, WaveInfoSO>        _waveInfos;
    private WaveSpawnTableSO                   _waveSpawnTable;

    // ─── 상태 ────────────────────────────────────────────────
    public bool IsReady { get; private set; }

    private event Action _onReady;


    public event Action OnReady
    {
        add
        {
            _onReady += value;
            if (IsReady) value?.Invoke();
        }
        remove { _onReady -= value; }
    }

    // ─── 등록 (DataManager가 호출) ────────────────────────────
    public void RegisterClasses(Dictionary<int, PlayerClassDataSO> dict)
    {
        _classes = dict;
        DebugTool.Log($"[GameDataModule] Classes 등록 ({dict?.Count ?? 0}건)", DebugType.Data);
    }

    public void RegisterZombieStats(Dictionary<int, ZombieStatSO> dict)
    {
        _zombieStats = dict;
        DebugTool.Log($"[GameDataModule] ZombieStats 등록 ({dict?.Count ?? 0}건)", DebugType.Data);
    }

    public void RegisterWaveInfos(Dictionary<int, WaveInfoSO> dict)
    {
        _waveInfos = dict;
        DebugTool.Log($"[GameDataModule] WaveInfos 등록 ({dict?.Count ?? 0}건)", DebugType.Data);
    }

    public void RegisterWaveSpawnTable(WaveSpawnTableSO table)
    {
        _waveSpawnTable = table;
        DebugTool.Log(
            $"[GameDataModule] WaveSpawnTable 등록 (그룹 {table?.GroupCount ?? 0}개)",
            DebugType.Data);
    }


    public void MarkReady()
    {
        if (IsReady)
        {
            DebugTool.Warning("[GameDataModule] 이미 Ready 상태에서 MarkReady() 재호출", DebugType.Data);
            return;
        }
        IsReady = true;
        DebugTool.Log("[GameDataModule] 모든 데이터 준비 완료 (IsReady = true)", DebugType.Data);
        _onReady?.Invoke();
    }

    // ─── 단일 조회 ────────────────────────────────────────────
    public PlayerClassDataSO GetClass(int classId)
    {
        if (!CheckReady(nameof(GetClass), classId)) return null;
        if (_classes == null || !_classes.TryGetValue(classId, out var data))
        {
            DebugTool.Warning($"[GameDataModule] ClassId {classId} 없음", DebugType.Data);
            return null;
        }
        return data;
    }

    public ZombieStatSO GetZombieStat(int zombieId)
    {
        if (!CheckReady(nameof(GetZombieStat), zombieId)) return null;
        if (_zombieStats == null || !_zombieStats.TryGetValue(zombieId, out var data))
        {
            DebugTool.Warning($"[GameDataModule] ZombieId {zombieId} 없음", DebugType.Data);
            return null;
        }
        return data;
    }

    public WaveInfoSO GetWaveInfo(int waveId)
    {
        if (!CheckReady(nameof(GetWaveInfo), waveId)) return null;
        if (_waveInfos == null || !_waveInfos.TryGetValue(waveId, out var data))
        {
            DebugTool.Warning($"[GameDataModule] WaveId {waveId} 없음 (Info)", DebugType.Data);
            return null;
        }
        return data;
    }

    public List<WaveSpawnEntry> GetWaveSpawns(int waveId)
    {
        if (!CheckReady(nameof(GetWaveSpawns), waveId)) return new List<WaveSpawnEntry>();
        if (_waveSpawnTable == null)
        {
            DebugTool.Warning("[GameDataModule] WaveSpawnTable 미등록", DebugType.Data);
            return new List<WaveSpawnEntry>();
        }
        return _waveSpawnTable.GetEntries(waveId);
    }

    // ─── 외래키 조인 ──────────────────────────────────

    public WaveBundle GetWave(int waveId)
    {
        if (!CheckReady(nameof(GetWave), waveId)) return null;
        var info = GetWaveInfoSilent(waveId);
        if (info == null)
        {
            DebugTool.Warning($"[GameDataModule] WaveId {waveId} 없음 (GetWave)", DebugType.Data);
            return null;
        }
        var spawns = _waveSpawnTable != null
            ? _waveSpawnTable.GetEntries(waveId)
            : new List<WaveSpawnEntry>();
        return new WaveBundle(info, spawns);
    }

    // ─── 전체 ID 순회 ────────────────────────────────────
    public IEnumerable<int> GetAllClassIds()
        => _classes?.Keys ?? Enumerable.Empty<int>();

    public IEnumerable<int> GetAllZombieIds()
        => _zombieStats?.Keys ?? Enumerable.Empty<int>();

    public IEnumerable<int> GetAllWaveIds()
        => _waveInfos?.Keys ?? Enumerable.Empty<int>();


    private bool CheckReady(string methodName, int id)
    {
        if (IsReady) return true;
        DebugTool.Warning(
            $"[GameDataModule] 미준비 상태에서 {methodName}({id}) 호출",
            DebugType.Data);
        return false;
    }


    private WaveInfoSO GetWaveInfoSilent(int waveId)
    {
        return _waveInfos != null && _waveInfos.TryGetValue(waveId, out var data) ? data : null;
    }
}
