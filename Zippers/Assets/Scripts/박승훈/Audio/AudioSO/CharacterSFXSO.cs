using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 캐릭터 관련 SFX를 관리하기 위한 SO
/// </summary>
[CreateAssetMenu(fileName = "CharacterSFXSO", menuName = "Zippers/SO/Audio/CharacterSFXSO", order = 0)]
public class CharacterSFXSO : ZippersSO
{
    [Header("캐릭터 SFX 목록")]
    
    [Space(5)][Header("Walking")][Tooltip("Walking Clip 필요")]
    [SerializeField] private List<AudioClip> walkingSfx = new List<AudioClip>();
    public List<AudioClip> WalkingSfx => walkingSfx;
    
    [Space(5)][Header("Running")][Tooltip("Running Clip 필요")]
    [SerializeField] private List<AudioClip> runningSfx = new List<AudioClip>();
    public List<AudioClip> RunningSfx => runningSfx;
    
    [Space(5)][Header("Male Hit")][Tooltip("Male Hit Clip 필요")]
    [SerializeField] private List<AudioClip> maleHitSfx = new List<AudioClip>();
    public List<AudioClip> MaleHitSfx => maleHitSfx;
    
    [Space(5)][Header("Female Hit")][Tooltip("Female Hit Clip 필요")]
    [SerializeField] private List<AudioClip> femaleHitSfx = new List<AudioClip>();
    public List<AudioClip> FemaleHitSfx => femaleHitSfx;
    
    [Space(5)][Header("Male Death")][Tooltip("Male Death Clip 필요")]
    [SerializeField] private List<AudioClip> maleDeathSfx = new List<AudioClip>();
    public List<AudioClip> MaleDeathSfx => maleDeathSfx;
    
    [Space(5)][Header("Female Death")][Tooltip("Female Death Clip 필요")]
    [SerializeField] private List<AudioClip> femaleDeathSfx = new List<AudioClip>();
    public List<AudioClip> FemaleDeathSfx => femaleDeathSfx;
    
    [Space(5)][Header("Grab Scraps")][Tooltip("Grab Scraps clip 필요")]
    [SerializeField] private List<AudioClip> scrapSfx = new List<AudioClip>();
    public List<AudioClip> ScrapSFX => scrapSfx;
    
    [Space(5)][Header("Grab Supplies")][Tooltip("Grab Supplies clip필요")]
    [SerializeField] private List<AudioClip> supplySfx = new List<AudioClip>();
    public List<AudioClip> SupplySFX => supplySfx;
    
    [Space(5)][Header("Grab Infection Sample")][Tooltip("Grab Infection Sample clip 필요")]
    [SerializeField] private List<AudioClip> sampleSfx = new List<AudioClip>();
    public List<AudioClip> SampleSFX => sampleSfx;
    
    private Dictionary<ResourcesType, List<AudioClip>> resourcesSFXDict = new Dictionary<ResourcesType, List<AudioClip>>();
    public Dictionary<ResourcesType, List<AudioClip>> ResourcesSFXDict => resourcesSFXDict;

    protected override void DictionaryInit()
    {
        resourcesSFXDict.Clear();
        
        resourcesSFXDict.Add(ResourcesType.Scrap, scrapSfx);
        resourcesSFXDict.Add(ResourcesType.Supplies, supplySfx);
        resourcesSFXDict.Add(ResourcesType.InfectionSample, sampleSfx);
    }
}

