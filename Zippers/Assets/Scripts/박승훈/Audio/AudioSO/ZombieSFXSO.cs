using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ZombieSFXSO", menuName = "Zippers/SO/Audio/ZombieSFXSO", order = 2)]
public class ZombieSFXSO : ZippersSO
{
    [Header("좀비 SFX 목록")]
    
    [Space(5)][Header("Move SFX")][Tooltip("Walking Clip 필요")]
    [SerializeField] private List<AudioClip> moveSfx = new List<AudioClip>();
    public List<AudioClip> MoveSfx => moveSfx;
    
    [Space(5)][Header("Boss Move SFX")][Tooltip("Boss Walking Clip 필요")]
    [SerializeField] private List<AudioClip> bossMoveSfx = new List<AudioClip>();
    public List<AudioClip> BossMoveSfx => bossMoveSfx;
    
    [Space(5)][Header("Normal Groan SFX")][Tooltip("Normal Groan Clip 필요")]
    [SerializeField] private List<AudioClip> groanSfx = new List<AudioClip>();
    public List<AudioClip> GroanSfx => groanSfx;
    
    [Space(5)][Header("Boss Groan SFX")][Tooltip("Boss Groan Clip 필요")]
    [SerializeField] private List<AudioClip> bossGroanSfx = new List<AudioClip>();
    public List<AudioClip> BossGroanSfx => bossGroanSfx;
    
    [Space(5)][Header("Normal Attack SFX")][Tooltip("Attack Clip 필요")]
    [SerializeField] private List<AudioClip> normalSfx = new List<AudioClip>();
    public List<AudioClip> NormalSfx => normalSfx;
    
    [Space(5)][Header("Ranged Attack SFX")][Tooltip("Attack Clip 필요")]
    [SerializeField] private List<AudioClip> rangedSfx = new List<AudioClip>();
    public List<AudioClip> RangedSfx => rangedSfx;
    
    [Space(5)][Header("Boss Attack SFX")][Tooltip("Attack Clip 필요")]
    [SerializeField] private List<AudioClip> bossSfx = new List<AudioClip>();
    public List<AudioClip> BossSfx => bossSfx;
    
    [Space(5)][Header("Hit SFX")][Tooltip("Hit Clip 필요")]
    [SerializeField] private List<AudioClip> hitSfx = new List<AudioClip>();
    public List<AudioClip> HitSfx => hitSfx;
    
    [Space(5)][Header("Death SFX")][Tooltip("Death Clip 필요")]
    [SerializeField] private List<AudioClip> deathSFX = new List<AudioClip>();
    public List<AudioClip> DeathSFX => deathSFX;
    
    [Space(5)][Header("Drop Scrap SFX")][Tooltip("Drop Scrap Clip 필요")]
    [SerializeField] private List<AudioClip> scrapSfx = new List<AudioClip>();
    public List<AudioClip> ScrapSfx => scrapSfx;
    
    [Space(5)][Header("Drop Supplies SFX")][Tooltip("Drop Scrap Clip 필요")]
    [SerializeField] private List<AudioClip> supplySfx = new List<AudioClip>();
    public List<AudioClip> SupplySfx => supplySfx;
    
    [Space(5)][Header("Drop Samples SFX")][Tooltip("Drop Scrap Clip 필요")]
    [SerializeField] private List<AudioClip> sampleSfx = new List<AudioClip>();
    public List<AudioClip> SampleSfx => sampleSfx;
    
    private readonly Dictionary<ResourcesType, List<AudioClip>> _resourcesSfxDict = new Dictionary<ResourcesType, List<AudioClip>>();
    public Dictionary<ResourcesType, List<AudioClip>> ResourcesSfxDict => _resourcesSfxDict;
    
    private readonly Dictionary<ZombieType, List<AudioClip>> _attackSfxDict = new Dictionary<ZombieType, List<AudioClip>>();
    public Dictionary<ZombieType, List<AudioClip>> AttackSfxDict => _attackSfxDict;

    public override void DictionaryInit()
    {
        _resourcesSfxDict.Clear();
        _attackSfxDict.Clear();
        
        _resourcesSfxDict.Add(ResourcesType.Scrap, scrapSfx);
        _resourcesSfxDict.Add(ResourcesType.Supplies, supplySfx);
        _resourcesSfxDict.Add(ResourcesType.InfectionSample, sampleSfx);
        
        _attackSfxDict.Add(ZombieType.Normal, normalSfx);
        _attackSfxDict.Add(ZombieType.Runner, normalSfx);
        _attackSfxDict.Add(ZombieType.Ranged, rangedSfx);
        _attackSfxDict.Add(ZombieType.Elite, normalSfx);
        _attackSfxDict.Add(ZombieType.Boss, bossSfx);
    }
}
