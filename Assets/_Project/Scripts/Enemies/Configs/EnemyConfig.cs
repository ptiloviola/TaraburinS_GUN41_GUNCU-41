using UnityEngine;
using TpsShooter.Items.Configs; 
using TpsShooter.Weapons.Configs;

namespace TpsShooter.Enemies.Configs
{
    public enum EnemyType { Melee, Ranged }

    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "TpsShooter/Enemies/EnemyConfig")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("General")]
        public string EnemyName = "Enemy";
        public EnemyType Type = EnemyType.Ranged; // Переключатель поведения
        public float MaxHealth = 100f;

        [Header("Navigation (States)")]
        public float PatrolSpeed = 2f;
        public float ChaseSpeed = 4.5f;
        public float SearchDuration = 3f; // Сколько секунд стоит и ищет игрока при потере

        [Header("Vision (FOV)")]
        public float VisionRadius = 20f;
        [Range(0, 360)] public float ViewAngle = 120f; // Широкий угол обзора
        public float SensorTickRate = 0.2f; // Опрос 5 раз в секунду
        public LayerMask TargetMask; // Слой игрока
        public LayerMask ObstacleMask; // Слой стен

        [Header("Combat (Common)")]
        public float AttackRange = 15f; // Для мили ставим 2, для дальника 15-20
        public float AttackCooldown = 1.5f;

        

        [Header("Melee Settings (If Type = Melee)")]
        public float MeleeDamage = 25f;

        [Header("Ranged Settings (If Type = Ranged)")]
        public GameObject WeaponPrefab;
        public WeaponConfig WeaponStats;
        [Tooltip("Скорость полета пули для расчета упреждения")]
        public float ProjectileSpeed = 50f; 
        [Tooltip("Радиус разброса стрельбы (чтобы ИИ мазал)")]
        public float AimInaccuracy = 0.5f; 

        [Tooltip("Дистанция, на которой разброс достигает максимума")]
        public float MaxInaccuracyDistance = 20f;

        [Header("Loot Drop")]
        public ItemConfig DropLoot; 
        [Range(0f, 1f)] public float DropChance = 0.5f;
    }
}