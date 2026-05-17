using UnityEngine;

[CreateAssetMenu(fileName = "UISFXSO", menuName = "Zippers/Audio/UISFXSO", order = 5)]
public class UISFXSO : ZippersSO
{
    [Header("웨이브 효과음")]
    [SerializeField] private AudioClip _StageStart;
    public AudioClip StageStart => _StageStart;
    [SerializeField] private AudioClip _nextWave;
    public AudioClip NextWave => _nextWave;
    [SerializeField] private AudioClip _StageClear;
    public AudioClip StageClear => _StageClear;
    
    protected override void DictionaryInit()
    {
    }
}
