using System.Collections.Generic;
using Gameplay.Campaign.Data;

namespace Gameplay.Campaign.Rules
{
    public interface IGraphRule
    {
        bool Validate(
            MapNode node, 
            INodeEncounter candidate, 
            Dictionary<string, List<string>> parentsMap, 
            CampaignGraphAsset graph);
    }
}