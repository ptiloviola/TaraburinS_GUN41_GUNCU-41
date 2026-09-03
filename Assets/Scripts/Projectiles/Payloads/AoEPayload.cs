using UnityEngine;
using Gameplay.Core;
using Gameplay.Projectiles.Contracts;

namespace Gameplay.Projectiles.Payloads
{
    public class AoEPayload : IProjectilePayload
    {
        private readonly DamagePayload _damagePayload;
        private readonly float _radius;
        private readonly LayerMask _enemyMask;

        public AoEPayload(DamagePayload damagePayload, float radius, LayerMask enemyMask)
        {
            _damagePayload = damagePayload;
            _radius = radius;
            _enemyMask = enemyMask;
        }

        public void Apply(Transform target, Vector3 hitPoint)
        {
            Collider[] hits = Physics.OverlapSphere(hitPoint, _radius, _enemyMask);
            
            foreach (var hit in hits)
            {
                // Ищем IDamageable на самом объекте или родителе (наш DamageReceiver)
                var damageable = hit.GetComponentInParent<IDamageable>();
                damageable?.TakeDamage(_damagePayload);
            }
            
            Debug.Log($"<color=orange>[AoE] Взрыв на {hitPoint}! Задето объектов: {hits.Length}</color>");
        }
    }
}