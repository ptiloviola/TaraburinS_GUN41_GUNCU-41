using UnityEngine;
using Zenject;
using VacuumSim.Robotics.Signals;
using VacuumSim.Rules;

namespace VacuumSim.Robotics.Audio
{
    [RequireComponent(typeof(Rigidbody))]
    public class VacuumAudioController : MonoBehaviour
    {
        [Header("Источники звука")]
        [SerializeField] private AudioSource _movementSource; 
        [SerializeField] private AudioSource _sfxSource;

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
            if (_movementSource != null && _rb != null)
            {
                float currentSpeed = _rb.velocity.magnitude;
                
                _movementSource.volume = Mathf.Lerp(_movementSource.volume, Mathf.Clamp(currentSpeed, 0.2f, 1f), Time.deltaTime * 5f);
                
                _movementSource.pitch = Mathf.Lerp(_movementSource.pitch, 1f + (currentSpeed * 0.15f), Time.deltaTime * 5f);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_sfxSource != null && _collisionClip != null)
            {
                _sfxSource.pitch = Random.Range(0.9f, 1.1f);
                _sfxSource.PlayOneShot(_collisionClip, _collisionVolume);
            }
        }

        private void OnTrashCollected(TrashCollectedSignal signal)
        {
            if (_sfxSource != null && _trashCollectClip != null)
            {
                _sfxSource.pitch = Random.Range(0.95f, 1.05f); 
                
                _sfxSource.PlayOneShot(_trashCollectClip, _trashVolume); 
            }
        }

        private void OnGameOver(GameOverSignal signal)
        {
            if (_movementSource != null)
            {
                _movementSource.Stop();
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