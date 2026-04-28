using System.Collections.Generic;
using UnityEngine;

namespace Audio
{
    public class ZombieSfxController : AudioController
    {
        [Header("좀비 오디오 소스 컴포넌트")]
        [SerializeField] private AudioSource _audioSource;
        [Space(5)] [Header("좀비 SFX SO")]
        [SerializeField] private ZombieSFXSO _zombieSFXSO;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            
            _zombieSFXSO.ZombieSfxDictInit();
        }

        public void PlayMoveSfx()
        {
            List<AudioClip> clips = _zombieSFXSO.MoveSfx;
            PlaySFX(_audioSource,clips);
            DebugTool.Log("좀비 이동 SFX 재생", DebugType.Audio, this);
        }

        public void PlayAttackSfx(ZombieType type)
        {
            List<AudioClip> clips = _zombieSFXSO.AttackSfxDict[type];
            PlaySFX(_audioSource,clips);
            DebugTool.Log($"좀비 공격 {type.ToString()} SFX 재생", DebugType.Audio, this);
        }

        public void PlayHitSfx()
        {
            List<AudioClip> clips = _zombieSFXSO.HitSfx;
            PlaySFX(_audioSource,clips);
            DebugTool.Log("좀비 피격 SFX 재생", DebugType.Audio, this);
        }

        public void PlayDeathSfx()
        {
            List<AudioClip> clips = _zombieSFXSO.DeathSFX;
            PlaySFX(_audioSource,clips);
            DebugTool.Log("좀비 죽음 SFX 재생", DebugType.Audio, this);
        }

        public void PlayDropResourcesSfx(ResourcesType type)
        {
            List<AudioClip> clips = _zombieSFXSO.ResourcesSfxDict[type];
            PlaySFX(_audioSource,clips);
            DebugTool.Log($"좀비 자원 {type.ToString()} 드랍 SFX 재생", DebugType.Audio, this);
        }
    }
}
