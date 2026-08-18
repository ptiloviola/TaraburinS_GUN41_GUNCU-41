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
        [Tooltip("Список названий анимаций для простоя")]
        public string[] IdleAnimStates = { "Idle", "Idle_Stretch", "Idle_Crazy" };

        [Tooltip("Анимация, когда заметил игрока (Alert)")]
        public string AlertAnimState = "Alert"; 
        public float AlertDuration = 1.2f; // Сколько секунд стоит и рычит

        // Каратист использует случайное шатание!
        public override IEnemyState CreatePatrolState(EnemyBrain brain) => new EnemyWanderPatrolState(brain);
        
        public override IEnemyState CreateCombatState(EnemyBrain brain) => new EnemyMeleeCombatState(brain);
    }
}