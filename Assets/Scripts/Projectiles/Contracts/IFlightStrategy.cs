using UnityEngine;

namespace Gameplay.Projectiles.Contracts
{
    public interface IFlightStrategy
    {
        void Initialize(Transform projectile, Transform target);
        bool ExecuteFlight(Transform projectile, Transform target, float speed, float hitDistance);
    }
}