using UnityEngine;
using Zenject;
using VacuumSim.Robotics.Signals; // Проверь, что неймспейс совпадает с твоим
using VacuumSim.Rules;

namespace VacuumSim.Robotics.Audio
{
    [RequireComponent(typeof(Rigidbody))]
    public class VacuumAudioController : MonoBehaviour
    {
        [Header("Источники звука")]
        [SerializeField] private AudioSource _movementSource; // Для гудения мотора (Loop)
        [SerializeField] private AudioSource _sfxSource;      // Для разовых эффектов (всасывание, удары)

        [Header("Аудиоклипы")]
        [SerializeField] private AudioClip _trashCollectClip;
        [Range(0f, 1f)] 
        [SerializeField] private float _trashVolume = 0.5f;
        [SerializeField] private AudioClip _collisionClip;
        [Range(0f, 1f)] 
        [SerializeField] private float _collisionVolume = 0.5f;

        [Inject] private SignalBus _signalBus;
        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            // Подписываемся на сигнал уборки мусора
            _signalBus.Subscribe<TrashCollectedSignal>(OnTrashCollected);
            _signalBus.Subscribe<GameOverSignal>(OnGameOver);
            
            if (_movementSource != null) 
            {
                _movementSource.loop = true;
                _movementSource.Play();
            }
        }

        private void Update()
        {
            // Динамическое изменение звука мотора от физической скорости робота
            if (_movementSource != null && _rb != null)
            {
                float currentSpeed = _rb.velocity.magnitude;
                
                // Если стоит - звук тихий, если едет - громче
                _movementSource.volume = Mathf.Lerp(_movementSource.volume, Mathf.Clamp(currentSpeed, 0.2f, 1f), Time.deltaTime * 5f);
                
                // Меняем тональность (Pitch): чем быстрее едет, тем выше "воет" мотор
                _movementSource.pitch = Mathf.Lerp(_movementSource.pitch, 1f + (currentSpeed * 0.15f), Time.deltaTime * 5f);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Срабатывает при физическом контакте (например, если робот врежется в кота или стену)
            if (_sfxSource != null && _collisionClip != null)
            {
                // Немного рандомизируем высоту звука удара, чтобы не звучало как пулемет
                _sfxSource.pitch = Random.Range(0.9f, 1.1f);
                _sfxSource.PlayOneShot(_collisionClip, _collisionVolume);
            }
        }

        private void OnTrashCollected(TrashCollectedSignal signal)
        {
            if (_sfxSource != null && _trashCollectClip != null)
            {
                _sfxSource.pitch = Random.Range(0.95f, 1.05f); 
                
                // Передаем нашу громкость вторым параметром!
                _sfxSource.PlayOneShot(_trashCollectClip, _trashVolume); 
            }
        }

        private void OnGameOver(GameOverSignal signal)
        {
            if (_movementSource != null)
            {
                _movementSource.Stop(); // Жестко вырубаем мотор
            }
        }

        private void OnDestroy()
        {
            if (_signalBus != null)
            {
                _signalBus.TryUnsubscribe<TrashCollectedSignal>(OnTrashCollected);
                _signalBus.TryUnsubscribe<GameOverSignal>(OnGameOver);
            }
        }
    }
}