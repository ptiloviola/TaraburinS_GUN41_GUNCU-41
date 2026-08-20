using System;
using UnityEngine;
using DG.Tweening;
using TpsShooter.Player;

namespace TpsShooter.Environment
{
    public class ExtractionPoint : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _timeLimit = 15f;
        
        [Header("Animation Settings")]
        [Tooltip("Итоговый размер модели после появления")]
        [SerializeField] private Vector3 _targetScale = new Vector3(2f, 2f, 2f);
        [Tooltip("Время появления (DOScale)")]
        [SerializeField] private float _scaleDuration = 1f;
        
        [Tooltip("Сила эффекта пульсации")]
        [SerializeField] private Vector3 _punchScale = new Vector3(0.3f, 0.3f, 0.3f);
        [Tooltip("Длительность одного цикла пульсации")]
        [SerializeField] private float _punchDuration = 1f;
        [Tooltip("Количество вибраций при пульсации")]
        [SerializeField] private int _punchVibrato = 2;
        [Tooltip("Эластичность пульсации (от 0 до 1)")]
        [SerializeField] private float _punchElasticity = 0.5f;

        [Header("Visuals & Lighting")]
        [SerializeField] private Transform _visualModel; 
        [SerializeField] private Light _glowLight;
        [Tooltip("Максимальная интенсивность света")]
        [SerializeField] private float _glowIntensity = 5f;
        [Tooltip("Скорость набора интенсивности")]
        [SerializeField] private float _glowDuration = 1f;
        [Tooltip("Целевой цвет свечения")]
        [SerializeField] private Color _glowColor = Color.cyan;
        [Tooltip("Скорость пульсации цвета")]
        [SerializeField] private float _colorDuration = 2f;

        [Header("Audio")]
        [SerializeField] private AudioSource _sirenAudio;

        public event Action OnActivated;
        public event Action OnPlayerExtracted;
        public event Action OnTimeExpired;

        public float TimeRemaining { get; private set; }
        public bool IsActive { get; private set; }

        public void Activate()
        {
            IsActive = true;
            TimeRemaining = _timeLimit;
            gameObject.SetActive(true);

            OnActivated?.Invoke();

            if (_sirenAudio != null)
            {
                _sirenAudio.pitch = 1f; 
                _sirenAudio.Play();
            }

            if (_visualModel != null)
            {
                _visualModel.DOKill(); 
                _visualModel.localScale = Vector3.zero;
                
                _visualModel.DOScale(_targetScale, _scaleDuration).SetEase(Ease.OutBounce).OnComplete(() => 
                {
                    _visualModel.DOPunchScale(_punchScale, _punchDuration, _punchVibrato, _punchElasticity)
                                .SetLoops(-1, LoopType.Yoyo);
                });
            }

            if (_glowLight != null)
            {
                _glowLight.DOKill();
                _glowLight.intensity = 0f;
                _glowLight.DOIntensity(_glowIntensity, _glowDuration);
                _glowLight.DOColor(_glowColor, _colorDuration).SetLoops(-1, LoopType.Yoyo);
            }
        }

        private void Update()
        {
            if (!IsActive) return;

            TimeRemaining -= Time.deltaTime;

            if (_sirenAudio != null)
            {
                float timeRatio = 1f - (TimeRemaining / _timeLimit); 
                _sirenAudio.pitch = Mathf.Lerp(1f, 1.5f, timeRatio);
            }

            if (TimeRemaining <= 0)
            {
                IsActive = false;
                _sirenAudio?.Stop(); 
                OnTimeExpired?.Invoke();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsActive) return;

            if (other.GetComponent<PlayerFacade>() != null)
            {
                IsActive = false;
                _sirenAudio?.Stop(); 
                OnPlayerExtracted?.Invoke();
            }
        }

        private void OnDestroy()
        {
            _visualModel?.DOKill();
            _glowLight?.DOKill();
        }
    }
}