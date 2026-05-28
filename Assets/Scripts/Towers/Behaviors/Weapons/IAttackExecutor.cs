using UnityEngine;

namespace Gameplay.Towers.Behaviors.Weapons
{
    public interface IAttackExecutor
    {
        // Передаем цель, урон и точку, откуда вылетает снаряд/луч
        void ExecuteAttack(Transform target, float damage, Transform firePoint);
    }
}