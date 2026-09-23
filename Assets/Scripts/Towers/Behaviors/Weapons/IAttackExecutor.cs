using UnityEngine;
using Gameplay.Projectiles.Contracts;

namespace Gameplay.Towers.Behaviors.Weapons
{
    public interface IAttackExecutor
    {
        void ExecuteAttack(Transform target, IProjectilePayload payload, Transform firePoint);
    }
}