using UnityEngine;
using Gameplay.Enemies.Data.Movement;
using Gameplay.Enemies.Data.Death;

namespace Gameplay.Enemies.Data
{
    [System.Serializable]
    public struct EnemyStats
    {
        public float MaxHealth;
        public int DamageToBase;
        public int RewardMoney;
    }

    [CreateAssetMenu(fileName = "NewEnemyConfig", menuName = "TD/Enemies/Enemy Config", order = 51)]
    public class EnemyConfig : ScriptableObject
    {
        [Header("Идентификация")]
        public string EnemyId;        
        public string DisplayName;    

        [Header("Визуал и UI")]
        public Sprite UIIcon;         
        public EnemyFacade Prefab;    

        [Header("Модуль: Характеристики")]
        public EnemyStats Stats = new EnemyStats { MaxHealth = 100f, DamageToBase = 1, RewardMoney = 15 };

        [Header("Модуль: Движение")]
        // Сюда мы будем перетаскивать наши ContinuousMovementConfig или DiscreteMovementConfig
        public MovementConfig Movement; 

        [Header("Модуль: Поведение при смерти (Опционально)")]
        public DeathBehaviorConfig DeathBehavior;
    }
}