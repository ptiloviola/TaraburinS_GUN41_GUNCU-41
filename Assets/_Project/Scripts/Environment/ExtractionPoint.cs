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
        [Tooltip("Сила эффекта пульсации")]
        [SerializeField] private Vector3 _punchScale = new Vector3(0.3f, 0.3f, 0.3f);
        
        [Header("Visuals")]
        [SerializeField] private Transform _visualModel; 
        [SerializeField] private Light _glowLight;

        // <--- ДОБАВЛЕНО АУДИО --->
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

            // Включаем сирену
            if (_sirenAudio != null)
            {
                _sirenAudio.pitch = 1f; // Базовый тон
                _sirenAudio.Play();
            }

            if (_visualModel != null)
            {
                _visualModel.DOKill(); 
                _visualModel.localScale = Vector3.zero;
                
                _visualModel.DOScale(_targetScale, 1f).SetEase(Ease.OutBounce).OnComplete(() => 
                {
                    _visualModel.DOPunchScale(_punchScale, 1f, 2, 0.5f).SetLoops(-1, LoopType.Yoyo);
                });
            }

            if (_glowLight != null)
            {
                _glowLight.DOKill();
                _glowLight.intensity = 0f;
                _glowLight.DOIntensity(5f, 1f);
                _glowLight.DOColor(Color.cyan, 2f).SetLoops(-1, LoopType.Yoyo);
            }
        }

        private void Update()
        {
            if (!IsActive) return;

            TimeRemaining -= Time.deltaTime;

            // <--- НАГНЕТАНИЕ НАПРЯЖЕНИЯ (ПОВЫШАЕМ ПИТЧ С 1.0 ДО 1.5 К КОНЦУ) --->
            if (_sirenAudio != null)
            {
                float timeRatio = 1f - (TimeRemaining / _timeLimit); // От 0 до 1
                _sirenAudio.pitch = Mathf.Lerp(1f, 1.5f, timeRatio);
            }

            if (TimeRemaining <= 0)
            {
                IsActive = false;
                _sirenAudio?.Stop(); // Выключаем при провале
                OnTimeExpired?.Invoke();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsActive) return;

            if (other.GetComponent<PlayerFacade>() != null)
            {
                IsActive = false;
                _sirenAudio?.Stop(); // Выключаем при успехе
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