using UnityEngine;
using Gameplay.Towers.Data.Payloads;

namespace Gameplay.Towers.Data.Modules
{
    [System.Serializable]
    public class AttackStats : IModuleDescriptor
    {
        public float Damage = 10f;
        public float Range = 3f;
        public float Cooldown = 1f;

        // НОВОЕ: Теперь геймдизайнер в Инспекторе перетаскивает сюда 
        // нужный SO (Яд, Огонь, Сплеш, Обычный урон)
        [Header("Тип атаки")]
        public PayloadConfig PayloadStrategy;

        public string GetStatsDescription()
        {
            // Небольшая защита: если урона нет, не выводим ничего
            if (Damage <= 0 && Range <= 0) return string.Empty;
            
            return $"Урон: {Damage}\nРадиус: {Range}\nКулдаун: {Cooldown} сек\n";
        }
    }
}