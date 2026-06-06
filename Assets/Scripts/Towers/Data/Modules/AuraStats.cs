namespace Gameplay.Towers.Data.Modules
{
    [System.Serializable]
    public class AuraStats : IModuleDescriptor
    {
        public float Radius = 3f;
        public float TickRate = 0.5f;
        public float SlowdownMultiplier = 0.5f;

        public string GetStatsDescription()
        {
            if (Radius <= 0) return string.Empty;
            
            return $"Радиус Ауры: {Radius}\nЗамедление: {SlowdownMultiplier}x\n";
        }
    }
}