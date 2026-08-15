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
        
        [Header("Visuals")]
        [SerializeField] private Transform _visualModel; 
        [SerializeField] private Light _glowLight;

        // События для Presenter'а и FlowManager'а
        public event Action OnActivated;
        public event Action OnPlayerExtracted;
        public event Action OnTimeExpired;

        // Публичные свойства состояния для чтения
        public float TimeRemaining { get; private set; }
        public bool IsActive { get; private set; }

        public void Activate()
        {
            IsActive = true;
            TimeRemaining = _timeLimit;
            gameObject.SetActive(true);

            OnActivated?.Invoke();

            // Исправленная анимация DOTween
            if (_visualModel != null)
            {
                // Очищаем старые анимации на случай перезапуска
                _visualModel.DOKill(); 
                
                _visualModel.localScale = Vector3.zero;
                
                // 1. Сначала вырастаем до (1, 1, 1)
                _visualModel.DOScale(Vector3.one, 1f).SetEase(Ease.OutBounce).OnComplete(() => 
                {
                    // 2. По завершении роста (OnComplete) запускаем бесконечную пульсацию
                    _visualModel.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 1f, 2, 0.5f).SetLoops(-1, LoopType.Yoyo);
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
            if (TimeRemaining <= 0)
            {
                IsActive = false;
                OnTimeExpired?.Invoke();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsActive) return;

            if (other.GetComponent<PlayerFacade>() != null)
            {
                IsActive = false;
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