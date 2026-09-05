using UnityEngine;
using Gameplay.Core; 
using Gameplay.Projectiles.Data;
using Gameplay.Projectiles; // Для ModularProjectile

namespace Gameplay.Towers.Data.Modules
{
    [System.Serializable]
    public class AttackStats : IModuleDescriptor
    {
        [Header("Базовые характеристики")]
        public float Damage = 10f;
        public DamageType Type = DamageType.Physical;
        public float MinRange = 0f; // НОВОЕ: Мертвая зона
        public float Range = 3f;
        public float Cooldown = 1f;

        // НОВОЕ: Настройки геометрии и обзора
        [Header("Ограничения прицеливания")]
        [Range(0f, 360f)] 
        [Tooltip("Сектор обстрела по горизонтали (360 - круговой)")]
        public float FieldOfView = 360f; 
        public float MinPitch = -10f; // Насколько низко может опустить ствол
        public float MaxPitch = 80f;  // Насколько высоко может задрать ствол
        public bool CheckLineOfSight = true; // Нужна ли прямая видимость (Raycast)

        [Header("Логика поведения (Стратегии)")]
        public TargetingType Targeting = TargetingType.Closest;
        public AimingType Aiming = AimingType.Omni;
        
        [Header("Логика выстрела (Экзекутор)")]
        public ExecutorType Executor = ExecutorType.Hitscan;
        [Tooltip("Нужен только если выбран тип Projectile")]
        public ModularProjectile ProjectilePrefab;
        public PayloadConfig PayloadStrategy;

        public string GetStatsDescription()
        {
            if (Damage <= 0 && Range <= 0) return string.Empty;
            return $"Урон: {Damage} ({Type})\nРадиус: {Range}\nКулдаун: {Cooldown} сек\n";
        }
    }
}