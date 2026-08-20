using System;
using UnityEngine;
using DG.Tweening;

namespace TpsShooter.Effects
{
    public class BulletTracer : MonoBehaviour
    {
        [SerializeField] private TrailRenderer _trail;

        public void Simulate(Vector3 startPoint, Vector3 endPoint, float duration, Action onComplete)
        {
            transform.position = startPoint;
            if (_trail != null) _trail.Clear(); 
            
            gameObject.SetActive(true);

            transform.DOMove(endPoint, duration).SetEase(Ease.Linear).OnComplete(() =>
            {
                gameObject.SetActive(false);
                onComplete?.Invoke();
            });
        }

        private void OnDestroy()
        {
            transform.DOKill();
        }
    }
}