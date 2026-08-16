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
            // Сбрасываем позицию и очищаем старый хвост
            transform.position = startPoint;
            if (_trail != null) _trail.Clear(); 
            
            gameObject.SetActive(true);

            // Летим в точку попадания и по завершении выключаемся, возвращаясь в пул
            transform.DOMove(endPoint, duration).SetEase(Ease.Linear).OnComplete(() =>
            {
                gameObject.SetActive(false);
                onComplete?.Invoke();
            });
        }

        private void OnDestroy()
        {
            // Защита от утечек при уничтожении сцены
            transform.DOKill();
        }
    }
}