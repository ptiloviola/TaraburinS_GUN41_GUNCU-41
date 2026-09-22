using System.Collections.Generic;
using Gameplay.Levels.Data;

namespace Gameplay.Core.Data
{
    public class RunProgressModel
    {
        public LevelBlueprintConfig CurrentLevelBlueprint { get; set; }
        public int CurrentRunDepth { get; set; } = 0;
        
        public int CurrentRunGold { get; set; } = 0; 
        public List<string> ActiveRunItems { get; set; } = new List<string> { "StrongHeart", "CashMachine" };

        public void ResetRun()
        {
            CurrentRunDepth = 0;
            CurrentLevelBlueprint = null;
            CurrentRunGold = 0;
            ActiveRunItems.Clear();
        }
    }
}