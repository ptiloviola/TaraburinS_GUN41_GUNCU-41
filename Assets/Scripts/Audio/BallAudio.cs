using UnityEngine;

namespace Bowling.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class BallAudio : MonoBehaviour
    {
        [Header("Аудиофайлы")]
        [SerializeField] private AudioClip _rollClip; // Звук качения (зацикленный)
        [SerializeField] private AudioClip _hitClip;  // Звук удара о кеглю

        [Header("Настройки динамики")]
        [SerializeField] private float _maxSpeed = 20f; // При какой скорости звук будет на максимуме
        [SerializeField] private float _maxHitForce = 15f; // Сила максимального удара

        private AudioSource _audioSource;
        private Vector3 _lastPosition;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            
            // Настраиваем "Динамик" для качения
            _audioSource.clip = _rollClip;
            _audioSource.loop = true;  // Зацикливаем гул
            _audioSource.Play();       // Запускаем сразу, но громкость пока будет 0
        }

        private void Start()
        {
            _lastPosition = transform.position;
        }

        private void Update()
        {
            // Вычисляем реальную физическую скорость в метрах в секунду
            float currentSpeed = (transform.position - _lastPosition).magnitude / Time.deltaTime;
            _lastPosition = transform.position;

            // 1. ДИНАМИЧЕСКАЯ ГРОМКОСТЬ: Чем быстрее едет, тем громче гудит (от 0 до 1)
            _audioSource.volume = Mathf.Clamp01(currentSpeed / _maxSpeed);

            // 2. ДИНАМИЧЕСКИЙ ТОН (Pitch): Придаем звуку "ускорение"
            // Базовый питч = 0.5 (низкий гул), при разгоне повышается до 1.5
            _audioSource.pitch = Mathf.Clamp(0.5f + (currentSpeed / _maxSpeed), 0.5f, 1.5f);
        }

        private void OnCollisionEnter(Collision collision)
        {
            // relativeVelocity показывает, с какой силой столкнулись два объекта
            float hitForce = collision.relativeVelocity.magnitude;

            // Игнорируем микро-касания, чтобы не было "спама" звуком
            if (hitForce > 1f)
            {
                // Высчитываем громкость удара (слабый удар = тихий звук)
                float hitVolume = Mathf.Clamp01(hitForce / _maxHitForce);
                
                // PlayOneShot воспроизводит звук удара ПОВЕРХ зацикленного гула качения!
                _audioSource.PlayOneShot(_hitClip, hitVolume);
            }
        }
    }
}