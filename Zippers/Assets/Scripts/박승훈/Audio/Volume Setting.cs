using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class VolumeSetting : MonoBehaviour
{
    [SerializeField] private Slider _masterVolumeSlider;
    [SerializeField] private Slider _bgmVolumeSlider;
    [SerializeField] private Slider _sfxVolumeSlider;
    [SerializeField] private Slider _uiVolumeSlider;

    [SerializeField] private TMP_Text masterText;
    [SerializeField] private TMP_Text bgmText;
    [SerializeField] private TMP_Text sfxText;
    [SerializeField] private TMP_Text uiText;

    [SerializeField][Range(1f, 1.5f)] private float _maxSliderValue = 1.2f;
    private const float _minSliderValue = 0.0001f;

    private void OnEnable()
    {
        _masterVolumeSlider.onValueChanged.AddListener(MasterVolumeSetting);
        _bgmVolumeSlider.onValueChanged.AddListener(MasterVolumeSetting);
        _sfxVolumeSlider.onValueChanged.AddListener(MasterVolumeSetting);
        _uiVolumeSlider.onValueChanged.AddListener(MasterVolumeSetting);
    }
    
    private void Start()
    {
        SliderInit();
        VolumeInit();
    }

    private void OnDisable()
    {
        _masterVolumeSlider.onValueChanged.RemoveListener(MasterVolumeSetting);
        _bgmVolumeSlider.onValueChanged.RemoveListener(MasterVolumeSetting);
        _sfxVolumeSlider.onValueChanged.RemoveListener(MasterVolumeSetting);
        _uiVolumeSlider.onValueChanged.RemoveListener(MasterVolumeSetting);
    }

    private void VolumeInit()
    {
        _masterVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.MasterVolume);
        _bgmVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.BGMVolume);
        _sfxVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.SfxVolume);
        _uiVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.UIVolume);

        SetText(masterText, _masterVolumeSlider);
        SetText(bgmText, _bgmVolumeSlider);
        SetText(sfxText, _sfxVolumeSlider);
        SetText(uiText, _uiVolumeSlider);
    }

    private void SliderInit()
    {
        SliderMinMaxSetting(_masterVolumeSlider);
        SliderMinMaxSetting(_bgmVolumeSlider);
        SliderMinMaxSetting(_sfxVolumeSlider);
        SliderMinMaxSetting(_uiVolumeSlider);
    }

    public void MasterVolumeSetting(float value)
    {
        AudioManager.Instance.SetMasterVolume(value);
        SetText(masterText, _masterVolumeSlider);
        DebugTool.Log($"마스터 볼륨 변경 : {value}", DebugType.UI, this);
    }

    public void BgmVolumeSetting(float value)
    {
        AudioManager.Instance.SetBGMVolume(value);
        SetText(bgmText, _bgmVolumeSlider);
        DebugTool.Log($"BGM 볼륨 변경 : {value}", DebugType.UI, this);
    }

    public void SfxVolumeSetting(float value)
    {
        AudioManager.Instance.SetSFXVolume(value);
        SetText(sfxText, _sfxVolumeSlider);
        DebugTool.Log($"SFX 볼륨 변경 : {value}", DebugType.UI, this);
    }

    public void UIVolumeSetting(float value)
    {
        AudioManager.Instance.SetUIVolume(value);
        SetText(uiText, _uiVolumeSlider);
        DebugTool.Log($"UI 볼륨 변경 : {value}", DebugType.UI, this);
    }

    private void SliderMinMaxSetting(Slider slider)
    {
        slider.maxValue = _maxSliderValue;
        slider.minValue = _minSliderValue;        
    }

    private void SetText(TMP_Text tmp, Slider slider)
        => tmp.text = $"{((int)(slider.value * 100)).ToString()}";
}
