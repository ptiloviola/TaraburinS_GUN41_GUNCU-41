using UnityEngine;

namespace Gameplay.Projectiles.Contracts
{
    public interface IFlightStrategy
    {
        // НОВОЕ: Подготовка перед вылетом
        void Initialize(Transform projectile, Transform target);
        // Метод возвращает true, если снаряд достиг цели
        bool ExecuteFlight(Transform projectile, Transform target, float speed, float hitDistance);
    }
}