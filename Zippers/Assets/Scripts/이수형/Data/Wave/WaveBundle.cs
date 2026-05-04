using System.Collections.Generic;


public class WaveBundle
{

    public WaveInfoSO Info { get; }


    public IReadOnlyList<WaveSpawnEntry> SpawnEntries { get; }

    public WaveBundle(WaveInfoSO info, IReadOnlyList<WaveSpawnEntry> entries)
    {
        Info = info;
        SpawnEntries = entries ?? new List<WaveSpawnEntry>();
    }
}
