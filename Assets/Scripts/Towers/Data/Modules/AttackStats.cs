namespace Gameplay.Towers.Data.Modules
{
    [System.Serializable]
    public class AttackStats : IModuleDescriptor
    {
        public float Damage = 10f;
        public float Range = 3f;
        public float Cooldown = 1f;

        public string GetStatsDescription()
        {
            // Небольшая защита: если урона нет, не выводим ничего
            if (Damage <= 0 && Range <= 0) return string.Empty;
            
            return $"Урон: {Damage}\nРадиус: {Range}\nКулдаун: {Cooldown} сек\n";
        }
    }
}