using System;
using System.Collections.Generic;
using UnityEngine;
using Gameplay.Enemies.Data.Movement;
using Gameplay.Enemies.Data.Death;
using Gameplay.Combat.Statuses;
using Gameplay.Combat;

namespace Gameplay.Enemies.Data
{
    [Serializable]
    public struct StatusResistEntry
    {
        public StatusType Type;
        [Tooltip("1.0 = 100% времени/урона, 0.5 = 50%, 0 = полный иммунитет")]
        public float Multiplier;
    }

    [Serializable]
    public struct EnemyStats
    {
        public float MaxHealth;
        public float MoveSpeed;
        public int DamageToBase;
        public int RewardMoney;
    }

    [Serializable]
    public struct ArmorStats
    {
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
        public TargetType Type = TargetType.Ground; 

        [Header("Визуал и UI")]
        public Sprite UIIcon;         
        public EnemyFacade Prefab;    

        [Header("Модуль: Характеристики")]
        public EnemyStats Stats = new EnemyStats { MaxHealth = 100f, MoveSpeed = 3.5f, DamageToBase = 1, RewardMoney = 15 };
        public ArmorStats Armor = new ArmorStats { PhysicalMultiplier = 1f, EnergyMultiplier = 1f, ExplosiveMultiplier = 1f };
        
        [Header("Модуль: Сопротивления Статусам")]
        public List<StatusResistEntry> Resistances = new List<StatusResistEntry>();

        [Header("Модуль: Движение")]
        public MovementConfig Movement; 

        [Header("Модуль: Поведение при смерти (Опционально)")]
        public DeathBehaviorConfig DeathBehavior;

        public float GetResistMultiplier(StatusType type)
        {
            foreach (var entry in Resistances)
            {
                if (entry.Type == type) return entry.Multiplier;
            }
            return 1f; 
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Movement == null) Debug.LogWarning($"[EnemyConfig] Врагу {EnemyId} не назначен модуль Movement!", this);
            if (Prefab == null) Debug.LogWarning($"[EnemyConfig] Врагу {EnemyId} не назначен Prefab!", this);
        }
#endif
    }
}