using UnityEngine;
using Gameplay.Core;
using Gameplay.Projectiles.Contracts;

namespace Gameplay.Projectiles.Payloads
{
    public class AoEPayload : IProjectilePayload
    {
        // Единый статический буфер для всех взрывов в игре (Zero Allocation)
        private static readonly Collider[] HitBuffer = new Collider[32];
        
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
            // Используем NonAlloc версию, чтобы не создавать новые массивы
            int hitCount = Physics.OverlapSphereNonAlloc(hitPoint, _radius, HitBuffer, _enemyMask);
            
            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = HitBuffer[i];
                var damageable = hit.GetComponentInParent<IDamageable>();
                damageable?.TakeDamage(_damagePayload);
            }
            
#if UNITY_EDITOR
            Debug.Log($"<color=orange>[AoE] Взрыв на {hitPoint}! Задето объектов: {hitCount}</color>");
#endif
        }
    }
}