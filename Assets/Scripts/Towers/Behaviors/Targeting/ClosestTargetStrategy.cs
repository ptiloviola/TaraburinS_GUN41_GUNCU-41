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
                Vector3 directionToTarget = hit.transform.position - center.position;
                float sqrDistance = directionToTarget.sqrMagnitude;
                
                // Проверка мертвой зоны
                if (sqrDistance < minRangeSqr) continue;

                // НОВОЕ: Честная баллистическая проверка углов (Pitch)
                // Считаем горизонтальную дистанцию (X и Z)
                float distance2D = new Vector2(directionToTarget.x, directionToTarget.z).magnitude;
                // Вычисляем угол возвышения в градусах (отрицательный - цель ниже, положительный - цель выше)
                float pitchAngle = Mathf.Atan2(directionToTarget.y, distance2D) * Mathf.Rad2Deg;

                // Отсеиваем всё, что слишком высоко или слишком низко для этой башни
                if (pitchAngle < stats.MinPitch || pitchAngle > stats.MaxPitch) continue;

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
            
            Vector3 directionToTarget = target.position - center.position;
            float sqrDistance = directionToTarget.sqrMagnitude;
            
            if (sqrDistance > (stats.Range * stats.Range) || sqrDistance < (stats.MinRange * stats.MinRange)) 
                return false;

            // НОВОЕ: Проверка угла при удержании цели
            float distance2D = new Vector2(directionToTarget.x, directionToTarget.z).magnitude;
            float pitchAngle = Mathf.Atan2(directionToTarget.y, distance2D) * Mathf.Rad2Deg;

            if (pitchAngle < stats.MinPitch || pitchAngle > stats.MaxPitch) 
                return false; // Сбрасываем цель, если она улетела слишком высоко

            if (aimStrategy != null && !aimStrategy.CanAimAt(center, target)) return false;

            return true;
        }
    }
}