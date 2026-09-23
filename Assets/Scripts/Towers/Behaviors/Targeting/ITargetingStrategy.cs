using UnityEngine;
using Gameplay.Towers.Behaviors.Aiming;
using Gameplay.Towers.Data.Modules;

namespace Gameplay.Towers.Behaviors.Targeting
{
    public interface ITargetingStrategy
    {
        Transform FindTarget(Transform center, AttackStats stats, LayerMask enemyMask, IAimStrategy aimStrategy);
        bool IsTargetValid(Transform target, Transform center, AttackStats stats, IAimStrategy aimStrategy);
    }
}