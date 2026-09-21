using Gameplay.Core.Data;
using Gameplay.Levels.Data;

namespace Gameplay.Core.Services
{
    public class LinearRunDirector : IRunDirectorService
    {
        private readonly CampaignConfig _campaignConfig;

        public LinearRunDirector(CampaignConfig campaignConfig)
        {
            _campaignConfig = campaignConfig;
        }

        public bool HasNextLevel(RunProgressModel progress)
        {
            return progress.CurrentRunDepth < _campaignConfig.Levels.Count;
        }

        public LevelBlueprintConfig GetNextLevel(RunProgressModel progress)
        {
            if (HasNextLevel(progress))
            {
                return _campaignConfig.Levels[progress.CurrentRunDepth];
            }
            return null; 
        }
    }
}