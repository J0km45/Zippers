using System.Collections.Generic;
using UnityEngine;

namespace Audio
{
    public class AudioController : MonoBehaviour
    {
        protected void PlaySFX(AudioSource source, List<AudioClip> clips)
        {
            AudioClip clip = RandomPlay(clips);
            source.PlayOneShot(clip);
        }
        
        protected void PlaySFX(AudioSource source, AudioClip clip)
            => source.PlayOneShot(clip);

        protected void PlayBGM(AudioSource source, List<AudioClip> clips)
        {
            AudioClip clip = RandomPlay(clips);
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
            source.playOnAwake = false;
            source.loop = false;
        }

        protected void BgmSourceInit(AudioSource source)
        {
            source.playOnAwake = false;
            source.loop = true;
        }
    }
}