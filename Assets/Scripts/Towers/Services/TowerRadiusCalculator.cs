using Gameplay.Towers.Data;
using Gameplay.Towers.Data.Modules;

namespace Gameplay.Towers.Services
{
    public static class TowerRadiusCalculator
    {
        public static float GetMaxRadius(TowerLevelData levelData)
        {
            if (levelData == null) return 0f;
            
            var attack = levelData.GetModule<AttackStats>();
            if (attack != null && attack.Range > 0) return attack.Range;
            

            var barracks = levelData.GetModule<BarracksModuleDescriptor>();
            if (barracks != null && barracks.RallyPointRadius > 0) return barracks.RallyPointRadius;
            
            return 0f;
        }

        public static float GetMinRadius(TowerLevelData levelData)
        {
            if (levelData == null) return 0f;
            
            var attack = levelData.GetModule<AttackStats>();
            if (attack != null && attack.MinRange > 0) return attack.MinRange;
            
            return 0f;
        }
    }
}