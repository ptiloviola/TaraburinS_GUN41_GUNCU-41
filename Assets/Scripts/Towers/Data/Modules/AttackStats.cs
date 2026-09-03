using UnityEngine;
using Gameplay.Core; // Для DamageType
using Gameplay.Projectiles.Data;

namespace Gameplay.Towers.Data.Modules
{
    [System.Serializable]
    public class AttackStats : IModuleDescriptor
    {
        [Header("Базовые характеристики")]
        public float Damage = 10f;
        public DamageType Type = DamageType.Physical; // НОВОЕ: Тип урона
        public float Range = 3f;
        public float Cooldown = 1f;

        [Header("Тип атаки (Логика)")]
        public PayloadConfig PayloadStrategy;

        public string GetStatsDescription()
        {
            if (Damage <= 0 && Range <= 0) return string.Empty;
            return $"Урон: {Damage} ({Type})\nРадиус: {Range}\nКулдаун: {Cooldown} сек\n";
        }
    }
}