using System.Collections.Generic;
using UnityEngine;

namespace Audio
{
    public class WeaponSFXController : AudioController
    {
        [Header("무기 오디오 소스 컴포넌트")]
        [SerializeField] private AudioSource _audioSource;

        [Space(5)] [Header("무기 SFX SO")]
        [SerializeField] private WeaponSFXSO _weaponSfxso;

        private void Start()
        {
            SfxSourceInit(_audioSource);
        }

        public void PlayWeaponSfx(WeaponType type)
        {
            List<AudioClip> clips = _weaponSfxso.WeaponSfxDict[type];
            PlaySFX(_audioSource, clips);
            DebugTool.Log($"무기 {type.ToString()} 공격 SFX 재생", DebugType.Audio, this);
        }

        public void PlayReloadSfx(WeaponType type)
        {
            if (!_weaponSfxso.ReloadSfxDict.TryGetValue(type, out AudioClip clip))
            {
                DebugTool.Log($"{type} 재장전 SFX가 없습니다.", DebugType.Audio, this);
                return;
            }
            
            PlaySFX(_audioSource, clip);
            DebugTool.Log($"플레이어 {type.ToString()} 재장전 SFX 재생", DebugType.Audio, this);
        }
    }
}
