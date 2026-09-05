using UnityEngine;
using Gameplay.Towers.Behaviors.Aiming;
using Gameplay.Towers.Data.Modules;

namespace Gameplay.Towers.Behaviors.Targeting
{
    public class ClosestTargetStrategy : ITargetingStrategy
    {
        private readonly Collider[] _targetColliders = new Collider[20];

        public Transform FindTarget(Transform center, AttackStats stats, LayerMask enemyMask, IAimStrategy aimStrategy)
        {
            int hitsCount = Physics.OverlapSphereNonAlloc(center.position, stats.Range, _targetColliders, enemyMask);
            
            Transform bestTarget = null;
            float closestSqrDistance = Mathf.Infinity;

            float minRangeSqr = stats.MinRange * stats.MinRange;

            for (int i = 0; i < hitsCount; i++)
            {
                Collider hit = _targetColliders[i];
                float sqrDistance = (hit.transform.position - center.position).sqrMagnitude;
                
                // Пропускаем врага, если он зашел в мертвую зону мортиры
                if (sqrDistance < minRangeSqr) continue;

                if (sqrDistance < closestSqrDistance)
                {
                    if (aimStrategy == null || aimStrategy.CanAimAt(center, hit.transform))
                    {
                        closestSqrDistance = sqrDistance;
                        bestTarget = hit.transform;
                    }
                }
            }
            
            return bestTarget;
        }

        public bool IsTargetValid(Transform target, Transform center, AttackStats stats, IAimStrategy aimStrategy)
        {
            if (target == null || !target.gameObject.activeInHierarchy) return false;
            
            float sqrDistance = (target.position - center.position).sqrMagnitude;
            
            // Цель невалидна, если вышла из радиуса ИЛИ подошла слишком близко
            if (sqrDistance > (stats.Range * stats.Range) || sqrDistance < (stats.MinRange * stats.MinRange)) 
                return false;

            if (aimStrategy != null && !aimStrategy.CanAimAt(center, target)) return false;

            return true;
        }
    }
}