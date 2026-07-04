using UnityEngine;
using Zenject;

namespace MeatMushrooms.Wolf.Components
{
    [RequireComponent(typeof(AudioSource))]
    public class WolfAudioController : MonoBehaviour
    {
        [Header("Аудиоклипы")]
        public AudioClip[] HowlClips;
        public AudioClip[] CombatGrowlClips;
        public AudioClip[] LowGrowlClips;
        public AudioClip[] EatClips;

        private AudioSource _audioSource;
        private WolfEventBus _eventBus; // Наша новая шина

        [Inject]
        private void Construct(WolfEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.spatialBlend = 1f; 
        }

        private void OnEnable()
        {
            if (_eventBus == null) return;
            // ПОДПИСЫВАЕМСЯ
            _eventBus.OnHowl += PlayHowl;
            _eventBus.OnCombatGrowl += PlayCombatGrowl;
            _eventBus.OnLowGrowl += PlayLowGrowl;
            _eventBus.OnEat += PlayEat;
        }

        private void OnDisable()
        {
            if (_eventBus == null) return;
            // ОТПИСЫВАЕМСЯ
            _eventBus.OnHowl -= PlayHowl;
            _eventBus.OnCombatGrowl -= PlayCombatGrowl;
            _eventBus.OnLowGrowl -= PlayLowGrowl;
            _eventBus.OnEat -= PlayEat;
        }

        private void PlayHowl() => PlayRandomClip(HowlClips, 0.9f, 1.1f);
        private void PlayCombatGrowl() => PlayRandomClip(CombatGrowlClips, 0.8f, 1.2f);
        private void PlayLowGrowl() => PlayRandomClip(LowGrowlClips, 0.9f, 1.1f);
        private void PlayEat() => PlayRandomClip(EatClips, 0.9f, 1.2f);

        private void PlayRandomClip(AudioClip[] clips, float minPitch, float maxPitch)
        {
            if (clips == null || clips.Length == 0) return;
            AudioClip clip = clips[Random.Range(0, clips.Length)];
            _audioSource.pitch = Random.Range(minPitch, maxPitch);
            _audioSource.PlayOneShot(clip);
        }
    }
}