using UnityEngine;

namespace Bowling.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class PinAudio : MonoBehaviour
    {
        [SerializeField] private AudioClip _pinHitClip;
        [SerializeField] private float _maxHitForce = 10f; // Кегли легче шара, порог удара ниже

        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            float hitForce = collision.relativeVelocity.magnitude;

            if (hitForce > 1f)
            {
                float hitVolume = Mathf.Clamp01(hitForce / _maxHitForce);
                
                // Чтобы звук десятков падающих кеглей не сливался в кашу,
                // мы чуть-чуть меняем тональность (Pitch) случайным образом при каждом ударе!
                _audioSource.pitch = Random.Range(0.8f, 1.2f);
                
                _audioSource.PlayOneShot(_pinHitClip, hitVolume);
            }
        }
    }
}