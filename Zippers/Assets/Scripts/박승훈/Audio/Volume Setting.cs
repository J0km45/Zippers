using UnityEngine;
using UnityEngine.UI;

public class VolumeSetting : MonoBehaviour
{
    [SerializeField] private Slider _masterVolumeSlider;
    [SerializeField] private Slider _bgmVolumeSlider;
    [SerializeField] private Slider _sfxVolumeSlider;
    [SerializeField] private Slider _uiVolumeSlider;

    [SerializeField][Range(1f, 1.5f)] private float _maxSliderValue = 1.2f;
    private const float _minSliderValue = 0.0001f;
    
    private void Start()
    {
        SliderInit();
        VolumeInit();
    }

    private void VolumeInit()
    {
        _masterVolumeSlider.value = AudioManager.Instance.MasterVolume;
        _bgmVolumeSlider.value = AudioManager.Instance.BGMVolume;
        _sfxVolumeSlider.value = AudioManager.Instance.SfxVolume;
        _uiVolumeSlider.value = AudioManager.Instance.UIVolume;
    }

    private void SliderInit()
    {
        SliderMinMaxSetting(_masterVolumeSlider);
        SliderMinMaxSetting(_bgmVolumeSlider);
        SliderMinMaxSetting(_sfxVolumeSlider);
        SliderMinMaxSetting(_uiVolumeSlider);
    }

    public void MasterVolumeSetting()
    {
        AudioManager.Instance.SetMasterVolume(_masterVolumeSlider.value);
        DebugTool.Log($"마스터 볼륨 변경 : {_masterVolumeSlider.value}", DebugType.UI, this);
    }

    public void BgmVolumeSetting()
    {
        AudioManager.Instance.SetBGMVolume(_bgmVolumeSlider.value);
        DebugTool.Log($"BGM 볼륨 변경 : {_bgmVolumeSlider.value}", DebugType.UI, this);
    }

    public void SfxVolumeSetting()
    {
        AudioManager.Instance.SetSFXVolume(_sfxVolumeSlider.value);
        DebugTool.Log($"SFX 볼륨 변경 : {_sfxVolumeSlider.value}", DebugType.UI, this);
    }

    public void UIVolumeSetting()
    {
        AudioManager.Instance.SetUIVolume(_uiVolumeSlider.value);
        DebugTool.Log($"UI 볼륨 변경 : {_uiVolumeSlider.value}", DebugType.UI, this);
    }

    private void SliderMinMaxSetting(Slider slider)
    {
        slider.maxValue = _maxSliderValue;
        slider.minValue = _minSliderValue;        
    }
}
