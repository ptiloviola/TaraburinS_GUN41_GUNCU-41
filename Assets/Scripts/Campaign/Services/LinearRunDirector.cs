using Gameplay.Campaign.Data;

namespace Gameplay.Campaign.Services
{
    public class LinearRunDirector : IRunDirectorService
    {
        private readonly CampaignConfig _campaignConfig;

        public LinearRunDirector(CampaignConfig campaignConfig)
        {
            _campaignConfig = campaignConfig;
        }

        public bool HasNextNode(RunProgressModel progress)
        {
            return progress.CurrentRunDepth < _campaignConfig.Nodes.Count;
        }

        public MapNode GetNextNode(RunProgressModel progress)
        {
            if (HasNextNode(progress))
            {
                return _campaignConfig.Nodes[progress.CurrentRunDepth];
            }
            return null; 
        }
    }
}