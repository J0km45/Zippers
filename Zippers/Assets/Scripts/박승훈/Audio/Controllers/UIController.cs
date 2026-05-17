using UnityEngine.UI;
using UnityEngine;

namespace Audio
{
    public class UIController : AudioController
    {
        [Header("UI 오디오 소스 컴포넌트")]
        [SerializeField] private AudioSource _audioSource;
        
        [Space(5)] [Header("UI SFX SO")]
        [SerializeField] private UISFXSO _uiSFXSO;

        private void Awake()
            => _audioSource = GetComponent<AudioSource>();

        private void Start()
            => SfxSourceInit(_audioSource);

        public void PlayBattleStart()
        {
            AudioClip clip = _uiSFXSO.StageClear;
            PlaySFX(_audioSource, clip);
            DebugTool.Log("스테이지 시작 SFX 재생", DebugType.Audio, this);
        }

        public void PlayBattleClear()
        {
            AudioClip clip = _uiSFXSO.StageClear;
            PlaySFX(_audioSource, clip);
            DebugTool.Log("스테이지 클리어 SFX 재생", DebugType.Audio, this);
        }

        public void PlayNextWave()
        {
            AudioClip clip = _uiSFXSO.NextWave;
            PlaySFX(_audioSource, clip);
            DebugTool.Log("다음 SFX 재생", DebugType.Audio, this);
        }
    }
}
