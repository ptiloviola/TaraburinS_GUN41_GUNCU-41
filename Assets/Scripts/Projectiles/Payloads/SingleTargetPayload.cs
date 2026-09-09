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

            // TryGetComponent работает быстрее и не создает мусор в памяти
            if (target.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(_damagePayload);
            }
        }
    }
}