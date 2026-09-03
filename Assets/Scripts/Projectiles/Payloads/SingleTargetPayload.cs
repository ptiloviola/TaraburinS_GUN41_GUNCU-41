using UnityEngine;
using Gameplay.Core; // Здесь лежит наш IDamageable
using Gameplay.Projectiles.Contracts;

namespace Gameplay.Projectiles.Payloads
{
    // Обычный эффект урона по одной цели (Single Target)
    public class SingleTargetPayload : IProjectilePayload
    {
        private readonly float _damage;

        public SingleTargetPayload(float damage)
        {
            _damage = damage;
        }

        public void Apply(Transform target, Vector3 hitPoint)
        {
            if (target == null) return;

            var damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(new DamagePayload(_damage, DamageType.Physical));
            }
        }
    }
}