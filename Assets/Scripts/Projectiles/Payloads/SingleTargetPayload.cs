using UnityEngine;
using Gameplay.Combat; 
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

            if (target.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(_damagePayload);
            }
        }
    }
}