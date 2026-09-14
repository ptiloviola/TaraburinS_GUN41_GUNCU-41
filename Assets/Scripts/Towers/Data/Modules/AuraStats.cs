using System;
using System.Collections.Generic;
using UnityEngine;
using Gameplay.Core.Statuses.Data;
using Gameplay.Core.Attributes;

namespace Gameplay.Towers.Data.Modules
{
    [Serializable]
    public class AuraStats : IModuleDescriptor
    {
        [Header("Настройки триггера (Башня)")]
        public float TriggerRadius = 5f; 
        public float Cooldown = 4f; 
        
        [Header("Настройки облака (Зона)")]
        public float ZoneDuration = 3f; 
        public float ZoneRadius = 2f; 
        public float TickRate = 0.5f; 
        
        [Header("Визуал")]
        public GameObject ZonePrefab; 

        [Header("Накладываемые эффекты")]
        [SerializeReference, SubclassSelector] 
        public List<IStatusConfig> StatusEffects = new List<IStatusConfig>();

        public string GetStatsDescription()
        {
            return $"Радиус зоны: {ZoneRadius}\nДлительность: {ZoneDuration}с\nЭффектов: {StatusEffects.Count}\n";
        }
    }
}