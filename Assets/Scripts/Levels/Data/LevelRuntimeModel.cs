using Gameplay.Core.Data;

namespace Gameplay.Levels.Data
{
    public class LevelRuntimeModel
    {
        public int StartingLives { get; private set; }
        public int StartingMoney { get; private set; }
        public int FoundationQuota { get; private set; }

        public LevelRuntimeModel(RunProgressModel progressModel)
        {
            var blueprint = progressModel.CurrentNode?.CombatLevel;
            
            if (blueprint != null)
            {
                StartingLives = blueprint.StartingLives;
                StartingMoney = blueprint.StartingMoney;
                FoundationQuota = blueprint.FoundationQuota;
            }
        }
    }
}