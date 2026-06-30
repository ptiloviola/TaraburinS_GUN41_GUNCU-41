using UnityEngine;
using Gameplay.Towers.Behaviors.Aiming;

namespace Gameplay.Towers.Behaviors.Targeting
{
    // Компонент, который ищет ближайшего врага
    public class ClosestTargetStrategy : MonoBehaviour, ITargetingStrategy
    {
        // Массив живет здесь, не засоряя боевой модуль
        private Collider[] _targetColliders = new Collider[20];

        public Transform FindTarget(Transform center, float range, LayerMask enemyMask, IAimStrategy aimStrategy)
        {
            int hitsCount = Physics.OverlapSphereNonAlloc(center.position, range, _targetColliders, enemyMask);
            
            Transform bestTarget = null;
            float closestSqrDistance = Mathf.Infinity;

            for (int i = 0; i < hitsCount; i++)
            {
                Collider hit = _targetColliders[i];
                float sqrDistance = (hit.transform.position - center.position).sqrMagnitude;
                
                if (sqrDistance < closestSqrDistance)
                {
                    // Проверяем, может ли башня физически туда повернуться
                    if (aimStrategy == null || aimStrategy.CanAimAt(center, hit.transform))
                    {
                        closestSqrDistance = sqrDistance;
                        bestTarget = hit.transform;
                    }
                }
            }
            
            return bestTarget;
        }

        public bool IsTargetValid(Transform target, Transform center, float range, IAimStrategy aimStrategy)
        {
            if (target == null || !target.gameObject.activeInHierarchy) return false;
            
            float sqrDistance = (target.position - center.position).sqrMagnitude;
            if (sqrDistance > (range * range)) return false;

            if (aimStrategy != null && !aimStrategy.CanAimAt(center, target)) return false;

            return true;
        }
    }
}

