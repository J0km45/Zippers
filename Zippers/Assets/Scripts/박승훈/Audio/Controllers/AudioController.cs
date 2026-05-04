using System.Collections.Generic;
using UnityEngine;

namespace Audio
{
    public class AudioController : MonoBehaviour
    {
        protected void PlaySFX(AudioSource source, List<AudioClip> clips)
        {
            if (!CheckAudioSource(source))
                return;
            
            if(!TryGetAudioClip(clips, out AudioClip clip))
                return;
            
            source.PlayOneShot(clip);
        }
        
        protected void PlaySFX(AudioSource source, AudioClip clip)
        {
            if (!CheckAudioSource(source) || !CheckAudioClip(clip))
                return;
            
            source.PlayOneShot(clip);
        }

        protected void PlayBGM(AudioSource source, List<AudioClip> clips)
        {
            if (!CheckAudioSource(source))
                return;

            if (!TryGetAudioClip(clips, out AudioClip clip))
                return;
            
            source.clip = clip;
            source.Play();
        }
        
        private AudioClip RandomPlay(List<AudioClip> clips)
        {
            int random = Random.Range(0, clips.Count);
            return clips[random];
        }

        protected void SfxSourceInit(AudioSource source)
        {
            if (!CheckAudioSource(source))
                return;
            
            source.playOnAwake = false;
            source.loop = false;
        }

        protected void BgmSourceInit(AudioSource source)
        {
            if (!CheckAudioSource(source))
                return;
            
            source.playOnAwake = false;
            source.loop = true;
        }

        private bool TryGetAudioClip(List<AudioClip> clips, out AudioClip clip)
        {
            if (!CheckAudioClipList(clips))
            {
                clip = null;
                return false;
            }
            
            AudioClip randomClip = RandomPlay(clips);
            if (!CheckAudioClip(randomClip))
            {
                clip = null;
                return false;
            }
            
            clip = randomClip;
            return true;
        }
        
        private bool CheckAudioSource(AudioSource source)
        {
            if(source == null)
            {
                DebugTool.Error("오디오 소스 컴포넌트가 없습니다.", DebugType.Missing, this);
                return false;
            }
            return true;
        }

        private bool CheckAudioClipList(List<AudioClip> clips)
        {
            if (clips == null || clips.Count == 0)
            {
                DebugTool.Warnning("오디오 클립 목록이 비어 있습니다.", DebugType.Missing, this);
                return false;
            }
            return true;
        }
        
        private bool CheckAudioClip(AudioClip clip)
        {
            if (clip == null)
            {
                DebugTool.Warning("오디오 클립을 찾을 수 없습니다.", DebugType.Missing, this);
                return false;
            }
            return true;
        }
    }
}