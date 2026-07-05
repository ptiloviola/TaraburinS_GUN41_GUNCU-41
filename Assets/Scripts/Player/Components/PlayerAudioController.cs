using UnityEngine;

namespace MeatMushrooms.Player.Components
{
    [RequireComponent(typeof(AudioSource))]
    public class PlayerAudioController : MonoBehaviour
    {
        [Header("Звуки шагов")]
        public AudioClip[] WalkSteps;
        public AudioClip[] RunSteps;
        
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.spatialBlend = 0f; 
        }

        public void PlayFootstepWalkSound()
        {
            PlayRandomClip(WalkSteps, 0.9f, 1.1f, 0.5f);
        }

        public void PlayFootstepRunSound()
        {
            PlayRandomClip(RunSteps, 0.95f, 1.2f, 0.8f);
        }

        private void PlayRandomClip(AudioClip[] clips, float minPitch, float maxPitch, float volume)
        {
            if (clips == null || clips.Length == 0) return;
            
            AudioClip clip = clips[Random.Range(0, clips.Length)];
            _audioSource.pitch = Random.Range(minPitch, maxPitch);
            _audioSource.PlayOneShot(clip, volume);
        }
    }
}