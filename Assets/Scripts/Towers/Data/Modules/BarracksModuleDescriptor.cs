using System;
using Gameplay.Units.Data; // Путь к нашему новому DefenderConfig
using UnityEngine;

namespace Gameplay.Towers.Data.Modules
{
    [Serializable]
    public class BarracksModuleDescriptor : IModuleDescriptor
    {
        [Header("Настройки спавна")]
        public DefenderConfig DefenderData;
        public int MaxDefenders = 3;
        public float RespawnCooldown = 5f;

        [Header("Точка сбора")]
        public float RallyPointRadius = 5f;

        public string GetStatsDescription()
        {
            if (DefenderData == null) return string.Empty;
            
            return $"Гарнизон: {MaxDefenders} чел.\nВоскрешение: {RespawnCooldown} сек\n";
        }
    }
}