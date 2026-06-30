using UnityEngine;
using Gameplay.Towers.Behaviors.Aiming;

namespace Gameplay.Towers.Behaviors.Targeting
{
    public interface ITargetingStrategy
    {
        // Найти лучшую цель в радиусе
        Transform FindTarget(Transform center, float range, LayerMask enemyMask, IAimStrategy aimStrategy);
        
        // Проверить, не убежала ли текущая цель или не вышла ли за углы обстрела
        bool IsTargetValid(Transform target, Transform center, float range, IAimStrategy aimStrategy);
    }
}
