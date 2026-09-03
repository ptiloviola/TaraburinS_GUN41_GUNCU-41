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

    // НОВОЕ: Настройки восприимчивости
    [System.Serializable]
    public struct ArmorStats
    {
        [Tooltip("1.0 = 100% урона, 0.5 = 50% урона (сопротивление), 2.0 = 200% урона (уязвимость)")]
        public float PhysicalMultiplier;
        public float EnergyMultiplier;
        public float ExplosiveMultiplier;
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
        // НОВОЕ: Инициализируем единицами, чтобы не прописывать руками для каждого врага
        public ArmorStats Armor = new ArmorStats { PhysicalMultiplier = 1f, EnergyMultiplier = 1f, ExplosiveMultiplier = 1f };
        
        [Header("Модуль: Движение")]
        // Сюда мы будем перетаскивать наши ContinuousMovementConfig или DiscreteMovementConfig
        public MovementConfig Movement; 

        [Header("Модуль: Поведение при смерти (Опционально)")]
        public DeathBehaviorConfig DeathBehavior;

        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (Movement == null)
            {
                Debug.LogWarning($"[EnemyConfig] Врагу {EnemyId} не назначен модуль Movement!", this);
            }
            if (Prefab == null)
            {
                Debug.LogWarning($"[EnemyConfig] Врагу {EnemyId} не назначен Prefab!", this);
            }
        }
        #endif


    }
}