using UnityEngine;
using Gameplay.Projectiles.Contracts;

namespace Gameplay.Projectiles.Flight
{
    public class BallisticFlightStrategy : MonoBehaviour, IFlightStrategy
    {
        // Избавляемся от магических чисел
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

            // Вычисляем процент завершения пути
            float totalDistance = Vector3.Distance(_startPosition, target.position);
            
            // Защита от деления на ноль через константу
            if (totalDistance < MinDistanceToTarget) return true; 

            // Увеличиваем прогресс пропорционально скорости
            _progress += (speed * Time.deltaTime) / totalDistance;

            // 1. Двигаемся по прямой линии (Lerp)
            Vector3 currentPos = Vector3.Lerp(_startPosition, target.position, _progress);
            
            // 2. Добавляем высоту по синусоиде
            currentPos.y += Mathf.Sin(_progress * Mathf.PI) * _arcHeight;

            // 3. Поворачиваем ядро носом по вектору движения
            Vector3 moveDirection = currentPos - projectile.position;
            if (moveDirection != Vector3.zero)
            {
                projectile.forward = moveDirection;
            }

            // 4. Применяем позицию
            projectile.position = currentPos;

            // Если прогресс достиг максимума - долетели
            return _progress >= MaxProgress;
        }
    }
}