using UnityEngine;

namespace Gameplay.Towers.Data.Modules
{
    public enum AuraType
    {
        Slowdown,
        PoisonCloud
    }

    [System.Serializable]
    public class AuraStats : IModuleDescriptor
    {
        [Header("Настройки триггера (Башня)")]
        public float TriggerRadius = 5f; // Как далеко башня "видит" врага
        public float Cooldown = 4f; // Перезарядка распылителя
        
        [Header("Настройки облака (Зона)")]
        public AuraType EffectType = AuraType.Slowdown;
        public float ZoneDuration = 3f; // Сколько секунд висит облако
        public float ZoneRadius = 2f; // Размер самого облака
        public float TickRate = 0.5f; // Как часто облако тикает (применяет эффект)

        [Header("Сила эффекта")]
        public float SlowdownMultiplier = 0.5f; // Для заморозки
        public int PoisonDamagePerTick = 5; // Для яда
        
        [Header("Визуал")]
        public GameObject ZonePrefab; // Ссылка на префаб облака

        public string GetStatsDescription()
        {
            if (EffectType == AuraType.Slowdown)
                return $"Замедление: {SlowdownMultiplier}x\nВремя лужи: {ZoneDuration}с\nПерезарядка: {Cooldown}с\n";
            else
                return $"Урон ядом: {PoisonDamagePerTick}/тик\nВремя облака: {ZoneDuration}с\nПерезарядка: {Cooldown}с\n";
        }
    }
}