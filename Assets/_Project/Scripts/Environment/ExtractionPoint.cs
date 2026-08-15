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
        
        // <--- НОВЫЕ НАСТРОЙКИ РАЗМЕРА --->
        [Header("Animation Settings")]
        [Tooltip("Итоговый размер модели после появления")]
        [SerializeField] private Vector3 _targetScale = new Vector3(2f, 2f, 2f);
        [Tooltip("Сила эффекта пульсации")]
        [SerializeField] private Vector3 _punchScale = new Vector3(0.3f, 0.3f, 0.3f);
        
        [Header("Visuals")]
        [SerializeField] private Transform _visualModel; 
        [SerializeField] private Light _glowLight;

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

            if (_visualModel != null)
            {
                _visualModel.DOKill(); 
                _visualModel.localScale = Vector3.zero;
                
                // Используем _targetScale вместо Vector3.one
                _visualModel.DOScale(_targetScale, 1f).SetEase(Ease.OutBounce).OnComplete(() => 
                {
                    // Используем _punchScale
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