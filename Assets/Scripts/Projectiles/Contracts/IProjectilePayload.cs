using UnityEngine;

namespace Gameplay.Projectiles.Contracts
{
    public interface IProjectilePayload
    {
        void Apply(Transform target, Vector3 hitPoint);
    }
}