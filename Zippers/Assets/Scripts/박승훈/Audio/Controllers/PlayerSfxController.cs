using System.Collections.Generic;
using UnityEngine;

namespace Audio
{
    public class PlayerSfxController : AudioController
    {
        [Header("캐릭터 오디오 소스 컴포넌트")]
        [SerializeField] private AudioSource _audioSource;
        [Space(5)] [Header("캐릭터 SFX SO")]
        [SerializeField] private CharacterSFXSO _characterSfxso;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _characterSfxso.DictionaryInit();
        }
        
        private void Start()
            => SfxSourceInit(_audioSource);

        public void PlayWalkingSfx()
        {
            List<AudioClip> clips = _characterSfxso.WalkingSfx;
            PlaySFX(_audioSource, clips);
            DebugTool.Log("플레이어 걷기 SFX 재생", DebugType.Audio, this);
        }

        public void PlaySprintSfx()
        {
            List<AudioClip> clips = _characterSfxso.RunningSfx;
            PlaySFX(_audioSource, clips);
            DebugTool.Log("플레이어 달리기 SFX 재생", DebugType.Audio, this);
        }

        public void PlayMaleHitSfx()
        {
            List<AudioClip> clips = _characterSfxso.MaleHitSfx;
            PlaySFX(_audioSource, clips);
            DebugTool.Log("플레이어 피격 SFX 재생", DebugType.Audio, this);
        }

        public void PlayFemaleHitSfx()
        {
            List<AudioClip> clips = _characterSfxso.FemaleHitSfx;
            PlaySFX(_audioSource, clips);
            DebugTool.Log("플레이어 피격 SFX 재생", DebugType.Audio, this);
        }

        public void PlayMaleDeathSfx()
        {
            List<AudioClip> clips = _characterSfxso.MaleDeathSfx;
            PlaySFX(_audioSource, clips);
            DebugTool.Log("플레이어 죽음 SFX 재생", DebugType.Audio, this);
        }

        public void PlayFemaleDeathSfx()
        {
            List<AudioClip> clips = _characterSfxso.FemaleDeathSfx;
            PlaySFX(_audioSource, clips);
            DebugTool.Log("플레이어 죽음 SFX 재생", DebugType.Audio, this);
        }

        public void PlayGrabSfx(ResourcesType type)
        {
            List<AudioClip> clips = _characterSfxso.ResourcesSFXDict[type];
            PlaySFX(_audioSource, clips);
            DebugTool.Log($"플레이어 {type.ToString()} 줍기 SFX 재생", DebugType.Audio, this);
        }
    }
}
