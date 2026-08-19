using UnityEngine;
using TpsShooter.Weapons.Configs;
using TpsShooter.Enemies.States;
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

        [Header("Animations")]
        [Tooltip("Название State в Аниматоре для стрельбы")]
        public string ShootAnimState = "Firing Rifle"; 
        [Tooltip("Сколько секунд длится блокировка аниматора при выстреле")]
        public float ShootAnimDuration = 0.4f;


        public override IEnemyState CreatePatrolState(EnemyBrain brain) => new EnemyWaypointPatrolState(brain);
        public override IEnemyState CreateCombatState(EnemyBrain brain) => new EnemyCombatState(brain);
    }
}