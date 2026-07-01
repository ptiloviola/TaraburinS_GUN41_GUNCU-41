using UnityEngine;
using Gameplay.Core;
using Gameplay.Projectiles.Contracts;

namespace Gameplay.Projectiles.Payloads
{
    public class AoEPayload : IProjectilePayload
    {
        private readonly float _damage;
        private readonly float _radius;
        private readonly LayerMask _enemyMask;

        public AoEPayload(float damage, float radius, LayerMask enemyMask)
        {
            _damage = damage;
            _radius = radius;
            _enemyMask = enemyMask;
        }

        public void Apply(Transform target, Vector3 hitPoint)
        {
            // Находим всех врагов в радиусе взрыва
            Collider[] hits = Physics.OverlapSphere(hitPoint, _radius, _enemyMask);
            
            foreach (var hit in hits)
            {
                var damageable = hit.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(_damage);
                }
            }
            
            // Здесь же в будущем можно вызывать спавн префаба взрыва (VFX)
            Debug.Log($"<color=orange>[AoE] Взрыв на {hitPoint}! Задето врагов: {hits.Length}</color>");
        }
    }
}