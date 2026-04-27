using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private float _masterVolume = 0.5f;  // 전체
    [SerializeField] private float _bgmVolume = 0.5f;     // 배경음악
    [SerializeField] private float _sfxVolume = 0.5f;   // 특수 효과음
    [SerializeField] private float _uiVolume = 0.5f;      // UI 효과음
    
    private AudioSource _bgmSource;
    private AudioSource _sfxSource;
    private AudioSource _uiSource;
    
    public AudioSource BgmSource => _bgmSource;
    public AudioSource SfxSource => _sfxSource;
    public AudioSource UiSource => _uiSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public float MasterVolume
    {
        get { return _masterVolume; }
        set
        {
            _masterVolume = Mathf.Clamp01(value);
            if (_bgmSource != null)
                _bgmSource.volume = _bgmVolume * _masterVolume;
            if (_sfxSource != null)
                _sfxSource.volume = _sfxVolume * _masterVolume;
            if (_uiSource != null)
                _uiSource.volume = _uiVolume * _masterVolume;
        }
    }

    public float BgmVolume
    {
        get { return _bgmVolume; }
        set
        {
            _bgmVolume = Mathf.Clamp01(value);
            if (_bgmSource != null)
                _bgmSource.volume = _bgmVolume * _masterVolume;
        }
    }

    public float SfxVolume
    {
        get { return _sfxVolume; }
        set
        {
            _sfxVolume = Mathf.Clamp01(value);
            if (_sfxSource != null)
                _sfxSource.volume = _sfxVolume * _masterVolume;
        }
    }

    public float UIVolume
    {
        get { return _uiVolume; }
        set
        {
            _uiVolume = Mathf.Clamp01(value);
            if (_uiSource != null)
                _uiSource.volume = _uiVolume * _masterVolume;
        }
    }

    // BGM 재생
    public void PlayBGM(AudioSource source, AudioClip clip)
    {
        if (source == null) return;
        if (_bgmSource.clip == clip && _bgmSource.isPlaying) return;
        _bgmSource = source;
        source.volume = _bgmVolume *_masterVolume;
        source.clip = clip;
        source.Play();
    }

    // 전투 효과음 재생
    public void PlaySFX(AudioSource source, AudioClip clip)
    {
        if (source == null) return;
        _sfxSource = source;
        source.volume = _sfxVolume *_masterVolume;
        source.PlayOneShot(clip);
    }

    // UI 효과음 재생
    public void PlayUI(AudioSource source, AudioClip clip)
    {
        if (source == null) return;
        _uiSource = source;
        source.volume = _uiVolume *_masterVolume;
        source.PlayOneShot(clip);
    }
}