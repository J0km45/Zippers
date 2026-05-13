using Audio;
using System;
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
    private float _startVolume = 0f;

    private bool _hasFocus = true;
    private bool _pausedByFocus;
    
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        
        BgmSourceInit(_audioSource);
    }

    private void Start()
    {
        _audioSource.clip = _titleAudioClip;
        
        DebugTool.Log("인트로 BGM 시작", DebugType.Audio);
        _startVolume = _audioSource.volume;
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

    private void OnApplicationFocus(bool hasFocus)
    {
        _hasFocus = hasFocus;

        if (_audioSource == null)
            return;

        if (!hasFocus)
        {
            if (_audioSource.isPlaying)
            {
                _audioSource.volume = 0;
                _pausedByFocus = true;
            }

            return;
        }

        if (_pausedByFocus)
        {
            _audioSource.volume = _startVolume;
            _pausedByFocus = false;
        }
    }

    private IEnumerator TitleBGMChange()
    {
        if (_fadeOutTime > _introTime)
        {
            DebugTool.Warning("페이드 아웃 시간은 인트로 출력 길이보다 짧아야 합니다.", DebugType.Audio);

            yield return WaitWhileFocused(_baseTime);
            
            DebugTool.Log("타이틀 BGM 시작", DebugType.Audio);
            PlayBGM(BGMType.Title);

            yield break;
        }
        
        float waitTime = _introTime - _fadeOutTime;
        
        yield return WaitWhileFocused(waitTime);
        
        yield return FadeVolume(_startVolume, 0f, _fadeOutTime);
        
        DebugTool.Log("타이틀 BGM 시작", DebugType.Audio);
        PlayBGM(BGMType.Title);
        yield return FadeVolume(0f, _startVolume, _fadeOutTime);
    }

    /// <summary>
    /// 다른 
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    private IEnumerator WaitWhileFocused(float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            if(_hasFocus)
                time += Time.unscaledDeltaTime;
            
            yield return null;
        }
    }

    private IEnumerator FadeVolume(float startVolume, float targetVolume, float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            if (_hasFocus)
            {
                time += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(time / duration);
                _audioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            }
            yield return null;
        }
        _audioSource.volume = targetVolume;
    }
}
