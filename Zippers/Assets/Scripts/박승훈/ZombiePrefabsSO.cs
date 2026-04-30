using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ZombiePrefabsSO", menuName = "Zippers/Zombie/Prefabs", order = 0)]
public class ZombiePrefabsSO : ZippersSO
{
    [Header("좀비 프리팹 목록")]

    [Space(5)]
    [Header("일반 좀비")]
    [Tooltip("일반 좀비 프리팹 추가")]
    [SerializeField]
    private List<GameObject> normalZombies;

    public List<GameObject> NormalZombies => normalZombies;

    [Space(5)]
    [Header("러너 좀비")]
    [Tooltip("러너 좀비 프리팹 추가")]
    [SerializeField]
    private List<GameObject> runnerZombies;

    public List<GameObject> RunnerZombies => runnerZombies;

    [Space(5)]
    [Header("원거리 좀비")]
    [Tooltip("원거리 좀비 프리팹 추가")]
    [SerializeField]
    private List<GameObject> rangedZombies;

    public List<GameObject> RangedZombies => rangedZombies;

    [Space(5)]
    [Header("정예 좀비")]
    [Tooltip("정예 좀비 프리팹 추가")]
    [SerializeField]
    private List<GameObject> eliteZombies;

    public List<GameObject> EliteZombies => eliteZombies;

    [Space(5)]
    [Header("보스 좀비")]
    [Tooltip("보스 좀비 프리팹 추가")]
    [SerializeField]
    private List<GameObject> bossZombies;

    public List<GameObject> BossZombies => bossZombies;

    private Dictionary<ZombieType, List<GameObject>> zombieDict = new Dictionary<ZombieType, List<GameObject>>();

    public IReadOnlyDictionary<ZombieType, List<GameObject>> ZombieDict => zombieDict;

    /// <summary>
    /// 지정한 좀비 타입에 등록된 프리팹 중 하나를 랜덤으로 반환한다.
    /// </summary>
    public GameObject GetZombiePrefab(ZombieType zombieType)
    {
        if (!zombieDict.TryGetValue(zombieType, out List<GameObject> zombieList))
        {
            DebugTool.Log($"해당 타입의 좀비 프리팹 리스트가 등록되지 않았습니다. Type: {zombieType}", DebugType.Zombie);
            return null;
        }

        if (zombieList == null || zombieList.Count <= 0)
        {
            DebugTool.Log($"해당 타입의 좀비 리스트가 비어있습니다. Type: {zombieType}", DebugType.Zombie);
            return null;
        }

        int rand = Random.Range(0, zombieList.Count);
        GameObject prefab = zombieList[rand];

        if (prefab == null)
        {
            DebugTool.Warning($"해당 타입의 좀비 리스트에 비어있는 프리팹이 있습니다. Type: {zombieType}, Index: {rand}", DebugType.Zombie);
            return null;
        }

        return prefab;
    }

    protected override void DictionaryInit()
    {
        zombieDict.Clear();

        zombieDict.Add(ZombieType.Normal, normalZombies);
        zombieDict.Add(ZombieType.Runner, runnerZombies);
        zombieDict.Add(ZombieType.Ranged, rangedZombies);
        zombieDict.Add(ZombieType.Elite, eliteZombies);
        zombieDict.Add(ZombieType.Boss, bossZombies);

        DebugTool.Log("좀비 프리팹 딕셔너리 초기화 완료", DebugType.Zombie);
    }
}