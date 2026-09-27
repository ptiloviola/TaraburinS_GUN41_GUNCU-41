using System.Collections.Generic;
using UnityEngine;
using Gameplay.Campaign.Data;

namespace Gameplay.Campaign.Rules
{
    public abstract class NodePlacementRule : ScriptableObject, IGraphRule
    {
        public abstract bool Validate(
            MapNode node, 
            INodeEncounter candidate, 
            Dictionary<string, List<string>> parentsMap, 
            CampaignGraphAsset graph);
    }
}