using Audio;
using System.Collections.Generic;
using UnityEngine;

public class BgmController : AudioController
{
    [Header("오디오 소스 컴포넌트")][Tooltip("자동 연결")]
    [SerializeField] private AudioSource _audioSource;

    [Header("BGM SO")]
    [SerializeField] private BGMSO _bgmSo;
    
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        
        BgmSourceInit(_audioSource);
        
        _bgmSo.DictionaryInit();
    }

    public void PlayBGM(BGMType type)
    {
        List<AudioClip> clips = _bgmSo.BGMDict[type];
        PlayBGM(_audioSource, clips);
    }
    
    public void PlayBGM(int type)
    {
        List<AudioClip> clips = _bgmSo.BGMDict[(BGMType)type];
        PlayBGM(_audioSource, clips);
    }
}
