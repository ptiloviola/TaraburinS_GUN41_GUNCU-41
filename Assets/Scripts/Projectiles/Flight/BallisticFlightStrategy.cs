using UnityEngine;
using Gameplay.Projectiles.Contracts;

namespace Gameplay.Projectiles.Flight
{
    public class BallisticFlightStrategy : MonoBehaviour, IFlightStrategy
    {
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

            // Вычисляем процент завершения пути (от 0 до 1)
            float totalDistance = Vector3.Distance(_startPosition, target.position);
            
            // Защита от деления на ноль, если цель в упор
            if (totalDistance < 0.1f) return true; 

            // Увеличиваем прогресс пропорционально скорости
            _progress += (speed * Time.deltaTime) / totalDistance;

            // 1. Двигаемся по прямой линии (Lerp)
            Vector3 currentPos = Vector3.Lerp(_startPosition, target.position, _progress);
            
            // 2. Добавляем высоту по синусоиде! (Sin от 0 до PI дает идеальную дугу)
            currentPos.y += Mathf.Sin(_progress * Mathf.PI) * _arcHeight;

            // 3. Поворачиваем ядро носом по вектору движения
            Vector3 moveDirection = currentPos - projectile.position;
            if (moveDirection != Vector3.zero)
            {
                projectile.forward = moveDirection;
            }

            // 4. Применяем позицию
            projectile.position = currentPos;

            // Если прогресс достиг 100% - долетели
            return _progress >= 1f;
        }
    }
}