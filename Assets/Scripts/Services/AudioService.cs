using UnityEngine;
using UnityEngine.Audio;

namespace Services
{
    public class AudioService
    {
        private readonly AudioMixerGroup _sfxGroup;

        public AudioService(AudioMixerGroup sfxGroup)
        {
            _sfxGroup = sfxGroup;
        }

        public void PlaySound(AudioSource source, AudioClip clip, bool randomizePitch = false)
        {
            if (clip == null || source == null) return;

            source.outputAudioMixerGroup = _sfxGroup;
            source.pitch = randomizePitch ? Random.Range(0.9f, 1.1f) : 1f;
            source.PlayOneShot(clip);
        }

        public void PlaySoundAtPoint(AudioClip clip, Vector3 position, bool randomizePitch = false)
        {
            if (clip == null) return;

            GameObject tempAudioObj = new GameObject("TempAudio_" + clip.name);
            tempAudioObj.transform.position = position;
            
            AudioSource tempSource = tempAudioObj.AddComponent<AudioSource>();
            tempSource.clip = clip;
            tempSource.spatialBlend = 1f;

            tempSource.outputAudioMixerGroup = _sfxGroup;
            tempSource.pitch = randomizePitch ? Random.Range(0.9f, 1.1f) : 1f;
            
            tempSource.Play();
            
            Object.Destroy(tempAudioObj, clip.length + 0.1f);
        }
    }
}