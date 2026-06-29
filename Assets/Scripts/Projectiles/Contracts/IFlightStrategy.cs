using UnityEngine;

namespace Gameplay.Projectiles.Contracts
{
    public interface IFlightStrategy
    {
        // Метод возвращает true, если снаряд достиг цели
        bool ExecuteFlight(Transform projectile, Transform target, float speed, float hitDistance);
    }
}