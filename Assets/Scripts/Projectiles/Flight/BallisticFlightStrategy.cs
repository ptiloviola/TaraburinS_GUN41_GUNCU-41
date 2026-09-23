using UnityEngine;
using Gameplay.Projectiles.Contracts;

namespace Gameplay.Projectiles.Flight
{
    public class BallisticFlightStrategy : MonoBehaviour, IFlightStrategy
    {
        private const float MinDistanceToTarget = 0.1f;
        private const float MaxProgress = 1f;

        [Header("Настройки дуги")]
        [SerializeField] private float _arcHeight = 4f; 

        private Vector3 _startPosition;
        private float _progress;

        public void Initialize(Transform projectile, Transform target)
        {
            _startPosition = projectile.position;
            _progress = 0f;
        }

        public bool ExecuteFlight(Transform projectile, Transform target, float speed, float hitDistance)
        {
            if (target == null) return false;

            float totalDistance = Vector3.Distance(_startPosition, target.position);
            
            if (totalDistance < MinDistanceToTarget) return true; 

            _progress += (speed * Time.deltaTime) / totalDistance;

            Vector3 currentPos = Vector3.Lerp(_startPosition, target.position, _progress);
            
            currentPos.y += Mathf.Sin(_progress * Mathf.PI) * _arcHeight;

            Vector3 moveDirection = currentPos - projectile.position;
            if (moveDirection != Vector3.zero)
            {
                projectile.forward = moveDirection;
            }

            projectile.position = currentPos;

            return _progress >= MaxProgress;
        }
    }
}