using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 캐릭터 관련 SFX를 관리하기 위한 SO
/// </summary>
[CreateAssetMenu(fileName = "CharacterSFXSO", menuName = "Zippers/SO/Audio/CharacterSFXSO", order = 0)]
public class CharacterSFXSO : ScriptableObject
{
    [Header("캐릭터 SFX 목록")]
    
    [Space(5)][Header("Walking")][Tooltip("Walking Clip 필요")]
    [SerializeField] private List<AudioClip> walkingSfx = new List<AudioClip>();
    public List<AudioClip> WalkingSfx => walkingSfx;
    
    [Space(5)][Header("Running")][Tooltip("Running Clip 필요")]
    [SerializeField] private List<AudioClip> runningSfx = new List<AudioClip>();
    public List<AudioClip> RunningSfx => runningSfx;
    
    [Space(5)][Header("Hit")][Tooltip("Running Clip 필요")]
    [SerializeField] private List<AudioClip> hitSfx = new List<AudioClip>();
    public List<AudioClip> HitSfx => hitSfx;
    
    [Space(5)][Header("Death")][Tooltip("Death Clip 필요")]
    [SerializeField] private List<AudioClip> deathSfx = new List<AudioClip>();
    public List<AudioClip> DeathSfx => deathSfx;
    
    [Space(5)][Header("Grab Scraps")][Tooltip("Grab Scraps clip 필요")]
    [SerializeField] private List<AudioClip> scrapSFX = new List<AudioClip>();
    public List<AudioClip> ScrapSFX => scrapSFX;
    
    [Space(5)][Header("Grab Supplies")][Tooltip("Grab Supplies clip필요")]
    [SerializeField] private List<AudioClip> supplySFX = new List<AudioClip>();
    public List<AudioClip> SupplySFX => supplySFX;
    
    [Space(5)][Header("Grab Infection Sample")][Tooltip("Grab Infection Sample clip 필요")]
    [SerializeField] private List<AudioClip> sampleSFX = new List<AudioClip>();
    public List<AudioClip> SampleSFX => sampleSFX;
    
    private Dictionary<ResourcesType, List<AudioClip>> resourcesSFXDict = new Dictionary<ResourcesType, List<AudioClip>>();
    public Dictionary<ResourcesType, List<AudioClip>> ResourcesSFXDict => resourcesSFXDict;

    public void ResourcesSFXInit()
    {
        resourcesSFXDict.Clear();
        
        resourcesSFXDict.Add(ResourcesType.Scrap, scrapSFX);
        resourcesSFXDict.Add(ResourcesType.Supplies, supplySFX);
        resourcesSFXDict.Add(ResourcesType.InfectionSample, sampleSFX);
    }
}

