using Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BgmController : AudioController
{
    [Header("오디오 소스 컴포넌트")][Tooltip("자동 연결")]
    [SerializeField] private AudioSource _audioSource;

    [Header("BGM SO")]
    [SerializeField] private BGMSO _bgmSo;

    [Header("타이틀 오디오 클립")]
    [SerializeField] private AudioClip _titleAudioClip;

    [Header("인트로 오디오 출력 길이")] [Range(0.1f, 12.0f)]
    [SerializeField] private float _introTime = 12f;
    [Header("인트로 음악 페이드 아웃 타이밍")] [Tooltip("인트로 출력 길이 끝에서 n초")] [Range(0.1f, 12.0f)]
    [SerializeField] private float _fadeOutTime = 1f;

    private float _baseTime = 12f;
    
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        
        BgmSourceInit(_audioSource);
    }

    private void Start()
    {
        _audioSource.clip = _titleAudioClip;
        DebugTool.Log("인트로 BGM 시작", DebugType.Audio);
        _audioSource.Play();
        StartCoroutine(TitleBGMChange());
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

    private IEnumerator TitleBGMChange()
    {
        if (_fadeOutTime >= _introTime)
        {
            DebugTool.Error("페이드 아웃 시간은 인트로 출력 길이보다 짧아야 합니다.", DebugType.Audio);
            
            yield return YieldContainer.Seconds(_baseTime);
            DebugTool.Log("타이틀 BGM 시작", DebugType.Audio);
            PlayBGM(BGMType.Title);
            
            yield break;
        }
        
        float waitTime = _introTime - _fadeOutTime;
        
        yield return YieldContainer.Seconds(waitTime);
        float time = 0f;
        float startVolume = _audioSource.volume;

        while (time < _fadeOutTime)
        {
            time += Time.deltaTime;
            
            float t = time / _fadeOutTime;
            
            _audioSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }
        _audioSource.volume = 0f;
        
        DebugTool.Log("타이틀 BGM 시작", DebugType.Audio);
        _audioSource.volume = startVolume;
        PlayBGM(BGMType.Title);
        
        time = 0f;
        while (time < _fadeOutTime / 2f)
        {
            time += Time.deltaTime;
            
            float t = time / _fadeOutTime;
            
            _audioSource.volume = Mathf.Lerp(0f, startVolume, t);
            yield return null;
        }
    }
}
