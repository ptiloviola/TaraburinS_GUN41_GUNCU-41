using System.Collections.Generic;
using Gameplay.Campaign.Data;

namespace Gameplay.Campaign.Services
{
    public interface IRunDirectorService
    {
        void GenerateRunMap(RunProgressModel progress);

        IReadOnlyList<MapNode> GetAvailableChoices(RunProgressModel progress);

        void AdvanceToNode(RunProgressModel progress, string nodeId);
        
        bool IsCampaignCompleted(RunProgressModel progress);
    }
}