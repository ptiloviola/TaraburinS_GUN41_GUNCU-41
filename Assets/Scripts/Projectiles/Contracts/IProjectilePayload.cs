using UnityEngine;

namespace Gameplay.Projectiles.Contracts
{
    public interface IProjectilePayload
    {
        // Передаем цель и точную мировую координату попадания
        void Apply(Transform target, Vector3 hitPoint);
    }
}