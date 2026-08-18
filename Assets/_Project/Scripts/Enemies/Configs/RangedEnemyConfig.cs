using UnityEngine;
using TpsShooter.Weapons.Configs;
using TpsShooter.Enemies.States; // Будущие стейты
using TpsShooter.Enemies.Core;

namespace TpsShooter.Enemies.Configs
{
    [CreateAssetMenu(fileName = "RangedEnemyConfig", menuName = "TpsShooter/Enemies/Ranged Config")]
    public class RangedEnemyConfig : EnemyConfig
    {
        [Header("Ranged Combat")]
        public GameObject WeaponPrefab;
        public WeaponConfig WeaponStats;
        public float ProjectileSpeed = 50f; 
        public float AimInaccuracy = 0.5f; 
        public float MaxInaccuracyDistance = 20f;

        // Стрелок использует ходьбу по точкам
        public override IEnemyState CreatePatrolState(EnemyBrain brain) => new EnemyWaypointPatrolState(brain);
        
        // Временно возвращаем базовый комбат, пока не разделим их
        public override IEnemyState CreateCombatState(EnemyBrain brain) => new EnemyCombatState(brain);
    }
}