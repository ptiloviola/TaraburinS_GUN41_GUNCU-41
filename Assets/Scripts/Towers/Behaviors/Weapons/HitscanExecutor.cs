using UnityEngine;
using Gameplay.Projectiles.Contracts;

namespace Gameplay.Towers.Behaviors.Weapons
{
    // Никакого MonoBehaviour! Чистая стратегия.
    public class HitscanExecutor : IAttackExecutor
    {
        public void ExecuteAttack(Transform target, IProjectilePayload payload, Transform firePoint)
        {
            Debug.DrawRay(firePoint.position, firePoint.forward * 5f, Color.red, 0.2f);
            
            if (payload != null)
            {
                payload.Apply(target, target.position);
            }
        }
    }
}