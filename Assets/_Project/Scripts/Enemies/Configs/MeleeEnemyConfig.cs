using UnityEngine;
using TpsShooter.Enemies.States;
using TpsShooter.Enemies.Core;

namespace TpsShooter.Enemies.Configs
{
    [CreateAssetMenu(fileName = "MeleeEnemyConfig", menuName = "TpsShooter/Enemies/Melee Config")]
    public class MeleeEnemyConfig : EnemyConfig
    {
        [Header("Melee Combat")]
        public float MeleeDamage = 25f;
        
        [Tooltip("Список названий триггеров в Аниматоре для рандома (напр. 'Kick', 'Spin')")]
        public string[] AttackAnimTriggers = { "MeleeAttack" }; 

        [Header("Wander Settings")]
        public float WanderRadius = 15f; // Насколько далеко шатается
        public float IdlePauseDuration = 3f; // Сколько "тупит" между точками

        // Каратист использует случайное шатание!
        public override IEnemyState CreatePatrolState(EnemyBrain brain) => new EnemyWanderPatrolState(brain);
        
        public override IEnemyState CreateCombatState(EnemyBrain brain) => new EnemyCombatState(brain);
    }
}