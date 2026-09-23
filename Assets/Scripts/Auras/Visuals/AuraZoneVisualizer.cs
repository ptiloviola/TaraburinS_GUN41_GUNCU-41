using UnityEngine;
using System.Collections;
using System;

namespace Gameplay.Auras.Visuals
{
    public class AuraZoneVisualizer : MonoBehaviour
    {
        [Header("Настройки анимации")]
        [SerializeField] private float _animationTime = 0.4f;
        [SerializeField] private ParticleSystem _particles; 
        
        private Transform _visualRoot; 
        private Coroutine _animCoroutine;

        private void EnsureVisualRoot()
        {
            if (_visualRoot == null)
            {
                _visualRoot = transform.Find("Visuals");
                if (_visualRoot == null)
                {
                    if (transform.childCount > 0) _visualRoot = transform.GetChild(0);
                    else _visualRoot = transform;
                }
            }
        }

        public void PlayAppear(float targetRadius)
        {
            EnsureVisualRoot();
            if (_animCoroutine != null) StopCoroutine(_animCoroutine);
            
            _visualRoot.gameObject.SetActive(true);
            
            if (_particles != null) 
            {
                var shape = _particles.shape;
                shape.radius = targetRadius;
                _particles.Play();
            }

            float targetScale = targetRadius * 2f;
            
            if (_animationTime <= 0f)
            {
                _visualRoot.localScale = new Vector3(targetScale, targetScale, targetScale);
                return;
            }
            
            _animCoroutine = StartCoroutine(AnimateScaleRoutine(_visualRoot.localScale.x, targetScale, null));
        }

        public void PlayDisappear(Action onComplete)
        {
            EnsureVisualRoot();
            if (_animCoroutine != null) StopCoroutine(_animCoroutine);
            
            if (_particles != null) _particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            
            if (_animationTime <= 0f)
            {
                _visualRoot.gameObject.SetActive(false);
                onComplete?.Invoke();
                return;
            }

            _animCoroutine = StartCoroutine(AnimateScaleRoutine(_visualRoot.localScale.x, 0f, () => 
            {
                _visualRoot.gameObject.SetActive(false);
                onComplete?.Invoke();
            }));
        }

        private IEnumerator AnimateScaleRoutine(float fromScale, float toScale, Action onComplete)
        {
            float elapsed = 0f;

            while (elapsed < _animationTime)
            {
                elapsed += Time.unscaledDeltaTime; 
                float t = Mathf.Clamp01(elapsed / _animationTime);
                
                float easeT = 1f - Mathf.Pow(1 - t, 3);
                float current = Mathf.Lerp(fromScale, toScale, easeT);
                
                _visualRoot.localScale = new Vector3(current, current, current);
                yield return null;
            }

            _visualRoot.localScale = new Vector3(toScale, toScale, toScale);
            onComplete?.Invoke();
        }
    }
}