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
        

        [Header("Wander Settings")]
        public float WanderRadius = 15f;
        public float IdlePauseDuration = 3f;
        
        [Tooltip("Список названий анимаций для простоя")]
        public string[] IdleAnimStates = { "Idle", "Idle_Stretch", "Idle_Crazy" };

        [Header("Melee Animations")]
        [Tooltip("Список названий стейтов в Аниматоре для рандома (напр. 'Kick', 'Spin')")]
        public string[] AttackAnimStates = { "MeleeAttack" }; 
        [Tooltip("Длительность блокировки при ударе")]
        public float AttackAnimDuration = 0.8f;

        [Header("Backstep Settings")]
        [Tooltip("Название анимации отхода назад")]
        public string BackstepAnimState = "Walking Backwards";
        [Tooltip("Сколько секунд враг пятится назад")]
        public float BackstepDuration = 1.5f;
        [Tooltip("Дистанция отхода в метрах")]
        public float BackstepDistance = 4f;
        [Tooltip("Скорость ходьбы спиной")]
        public float BackstepSpeed = 2.5f;
        
        [Tooltip("Анимация, когда заметил игрока (Alert)")]
        public string AlertAnimState = "Alert"; 
        public float AlertDuration = 1.2f;

        public override IEnemyState CreatePatrolState(EnemyBrain brain) => new EnemyWanderPatrolState(brain);
        
        public override IEnemyState CreateCombatState(EnemyBrain brain) => new EnemyMeleeCombatState(brain);
    }
}