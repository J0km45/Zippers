using Audio;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BGMSO", menuName = "Zippers/Audio/BGMSO", order = 4)]
public class BGMSO : ZippersSO
{
    [Header("BGM 목록")]
    
    [Space(5)] [Header("타이틀 BGM")][Tooltip("Title Clip 필요")]
    [SerializeField] private List<AudioClip> _title;
    public List<AudioClip> Title => _title;
    
    [Space(5)][Header("로비 BGM")][Tooltip("Robby Clip 필요")]
    [SerializeField] private List<AudioClip> _robby;
    public List<AudioClip> Robby => _robby;
    
    [Space(5)][Header("전투 BGM")][Tooltip("Battle Clip 필요")]
    [SerializeField] private List<AudioClip> _battle;
    public List<AudioClip> Battle => _battle;
    
    [Space(5)][Header("보스 BGM")][Tooltip("Boss Clip 필요")]
    [SerializeField] private List<AudioClip> _boss;
    public List<AudioClip> Boss => _boss;
    
    [Space(5)][Header("상점 BGM")][Tooltip("Shop Clip 필요")]
    [SerializeField] private List<AudioClip> _shop;
    public List<AudioClip> Shop => _shop;
    
    [Space(5)][Header("노드 클리어 BGM")][Tooltip("Clear Clip 필요")]
    [SerializeField] private List<AudioClip> _clear;
    public List<AudioClip> Clear => _clear;
    
    [Space(5)][Header("노드 실패 BGM")][Tooltip("Fail Clip 필요")]
    [SerializeField] private List<AudioClip> _fail;
    public List<AudioClip> Fail => _fail;
    
    private Dictionary<BGMType, List<AudioClip>> _BGMDict = new Dictionary<BGMType, List<AudioClip>>();
    public Dictionary<BGMType, List<AudioClip>> BGMDict => _BGMDict;

    protected override void DictionaryInit()
    {
        _BGMDict.Clear();
        
        _BGMDict.Add(BGMType.Title, _title);
        _BGMDict.Add(BGMType.Robby, _robby);
        _BGMDict.Add(BGMType.Battle, _battle);
        _BGMDict.Add(BGMType.Boss, _boss);
        _BGMDict.Add(BGMType.Shop, _shop);
        _BGMDict.Add(BGMType.Clear, _clear);
        _BGMDict.Add(BGMType.Fail, _fail);
        
        DebugTool.Log("BGM 딕셔너리 초기화 완료", DebugType.Zombie);
    }
}
