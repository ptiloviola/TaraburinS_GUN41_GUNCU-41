using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

namespace Gameplay.Auras.Visuals
{
    public class AuraZoneVisualizer : MonoBehaviour
    {
        [Header("Ссылки")]
        [SerializeField] private Transform _visualRoot; 
        [SerializeField] private ParticleSystem _particles;

        [Header("Настройки анимации")]
        [SerializeField] private float _animationTime = 0.4f;
        [SerializeField] private Ease _appearEase = Ease.OutBack;
        [SerializeField] private Ease _disappearEase = Ease.InBack;

        public async UniTask PlayAppearAsync(float targetRadius, CancellationToken token)
        {
            _visualRoot.DOKill(); 
            
            _visualRoot.localScale = Vector3.zero;
            _visualRoot.gameObject.SetActive(true);
            
            if (_particles != null) _particles.Play();

            float targetScale = targetRadius * 2f;

            // 1. Запускаем твин и жестко связываем его с жизнью этого GameObject
            _visualRoot.DOScale(targetScale, _animationTime)
                .SetEase(_appearEase)
                .SetLink(gameObject);

            // 2. Ждем ровно столько, сколько длится анимация, с поддержкой токена отмены
            await UniTask.Delay(TimeSpan.FromSeconds(_animationTime), cancellationToken: token);
        }

        public async UniTask PlayDisappearAsync(CancellationToken token)
        {
            _visualRoot.DOKill();

            if (_particles != null) _particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            _visualRoot.DOScale(0f, _animationTime)
                .SetEase(_disappearEase)
                .SetLink(gameObject);

            await UniTask.Delay(TimeSpan.FromSeconds(_animationTime), cancellationToken: token);
                
            _visualRoot.gameObject.SetActive(false);
        }
    }
}