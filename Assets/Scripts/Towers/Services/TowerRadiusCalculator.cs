using Gameplay.Towers.Data;

namespace Gameplay.Towers.Services
{
    /// <summary>
    /// Чистый калькулятор без состояния.
    /// Единственная ответственность: извлечение радиусов из конфигурации башни.
    /// </summary>
    public static class TowerRadiusCalculator
    {
        public static float GetMaxRadius(TowerLevelData levelData)
        {
            if (levelData == null) return 0f;
            
            if (levelData.Attack != null && levelData.Attack.Range > 0) return levelData.Attack.Range;
            
            // ИСПРАВЛЕНО: Теперь используем TriggerRadius
            if (levelData.Aura != null && levelData.Aura.TriggerRadius > 0) return levelData.Aura.TriggerRadius;
            
            return 0f;
        }

        public static float GetMinRadius(TowerLevelData levelData)
        {
            if (levelData == null) return 0f;
            
            if (levelData.Attack != null && levelData.Attack.MinRange > 0) return levelData.Attack.MinRange;
            
            return 0f;
        }
    }
}