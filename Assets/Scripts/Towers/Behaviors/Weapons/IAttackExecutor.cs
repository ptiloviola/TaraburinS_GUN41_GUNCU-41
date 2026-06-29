using UnityEngine;
using Gameplay.Projectiles.Contracts;

namespace Gameplay.Towers.Behaviors.Weapons
{
    public interface IAttackExecutor
    {
        // Вместо float damage передаем готовую посылку!
        void ExecuteAttack(Transform target, IProjectilePayload payload, Transform firePoint);
    }
}