using UnityEngine;
using Gameplay.Core; 
using Gameplay.Projectiles.Contracts;

namespace Gameplay.Projectiles.Payloads
{
    public class SingleTargetPayload : IProjectilePayload
    {
        private readonly DamagePayload _damagePayload;

        public SingleTargetPayload(DamagePayload damagePayload)
        {
            _damagePayload = damagePayload;
        }

        public void Apply(Transform target, Vector3 hitPoint)
        {
            if (target == null) return;

            // Используем ?. (null-conditional) для краткости
            var damageable = target.GetComponent<IDamageable>();
            damageable?.TakeDamage(_damagePayload);
        }
    }
}