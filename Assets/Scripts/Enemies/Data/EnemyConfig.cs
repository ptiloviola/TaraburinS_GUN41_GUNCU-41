using UnityEngine;
using Gameplay.Enemies.Data.Movement;
using Gameplay.Enemies.Data.Death;

namespace Gameplay.Enemies.Data
{
    // НОВОЕ: Типы статусов
    public enum StatusType
    {
        Control,       // Заморозка, оглушение (В будущем: Заземление)
        DamageOverTime,// Яд, горение (В будущем: Рефлексия)
        Debuff         // Снижение брони (В будущем: Когнитивная открытость)
    }

    [System.Serializable]
    public struct EnemyStats
    {
        public float MaxHealth;
        public int DamageToBase;
        public int RewardMoney;
    }

    [System.Serializable]
    public struct ArmorStats
    {
        [Tooltip("1.0 = 100% урона, 0.5 = 50% урона, 2.0 = 200% урона")]
        public float PhysicalMultiplier;
        public float EnergyMultiplier;
        public float ExplosiveMultiplier;
    }

    // НОВОЕ: Сопротивление статусам
    [System.Serializable]
    public struct StatusResistances
    {
        [Tooltip("1.0 = 100% времени действия, 0.5 = статус висит в 2 раза меньше, 0 = полный иммунитет")]
        public float ControlMultiplier;
        public float DoTMultiplier;
        public float DebuffMultiplier;
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
        public ArmorStats Armor = new ArmorStats { PhysicalMultiplier = 1f, EnergyMultiplier = 1f, ExplosiveMultiplier = 1f };
        
        // НОВОЕ: По умолчанию статусы работают на 100% времени
        public StatusResistances StatusResist = new StatusResistances { ControlMultiplier = 1f, DoTMultiplier = 1f, DebuffMultiplier = 1f };

        [Header("Модуль: Движение")]
        public MovementConfig Movement; 

        [Header("Модуль: Поведение при смерти (Опционально)")]
        public DeathBehaviorConfig DeathBehavior;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Movement == null) Debug.LogWarning($"[EnemyConfig] Врагу {EnemyId} не назначен модуль Movement!", this);
            if (Prefab == null) Debug.LogWarning($"[EnemyConfig] Врагу {EnemyId} не назначен Prefab!", this);
        }
#endif
    }
}