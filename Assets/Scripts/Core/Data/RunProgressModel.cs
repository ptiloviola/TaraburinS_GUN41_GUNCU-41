using Gameplay.Levels.Data;

namespace Gameplay.Core.Data
{
    public class RunProgressModel
    {

        public LevelBlueprintConfig CurrentLevelBlueprint { get; set; }
        
        public int CurrentRunDepth { get; set; } = 0;

        public void ResetRun()
        {
            CurrentRunDepth = 0;
            CurrentLevelBlueprint = null;
        }
    }
}