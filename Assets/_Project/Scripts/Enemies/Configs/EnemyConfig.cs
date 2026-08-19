using UnityEngine;
using TpsShooter.Items.Configs;
using TpsShooter.Enemies.Core;
using TpsShooter.Audio;

namespace TpsShooter.Enemies.Configs
{
    public abstract class EnemyConfig : ScriptableObject
    {
        [Header("General Base")]
        public string EnemyName = "Enemy";
        public float MaxHealth = 100f;

        [Header("Combat Base")]
        public float AttackRange = 15f; 
        public float AttackCooldown = 1.5f;

        [Header("Navigation Base")]
        public float PatrolSpeed = 2f;
        public float ChaseSpeed = 4.5f;
        public float SearchDuration = 3f; 

        [Header("Vision & Hearing")]
        public float VisionRadius = 20f;
        [Range(0, 360)] public float ViewAngle = 120f; 
        public float HearingRadius = 15f;
        public float SensorTickRate = 0.2f; 
        public LayerMask TargetMask; 
        public LayerMask ObstacleMask; 

        [Header("Loot Drop")]
        public ItemConfig DropLoot; 
        [Range(0f, 1f)] public float DropChance = 0.5f;

        [Header("Audio")]
        public FootstepConfig FootstepAudioConfig;

        [Header("Debug & Gizmos")]
        public float EyeHeight = 1.5f;
        public float StateIndicatorHeight = 2.5f;

        public abstract IEnemyState CreatePatrolState(EnemyBrain brain);
        public abstract IEnemyState CreateCombatState(EnemyBrain brain);
    }
}