using System.Collections.Generic;
using UnityEngine;
using Gameplay.Campaign.Data;

namespace Gameplay.Campaign.Rules
{
    [CreateAssetMenu(fileName = "NoConsecutiveShops", menuName = "TD/Campaign/Rules/No Consecutive Shops")]
    public class NoConsecutiveShopsRule : NodePlacementRule
    {
        public override bool Validate(MapNode node, INodeEncounter candidate, Dictionary<string, List<string>> parentsMap, CampaignGraphAsset graph)
        {
            if (!(candidate is ShopEncounter)) 
                return true;

            if (!parentsMap.TryGetValue(node.Id, out List<string> parentIds)) 
                return true;

            foreach (string parentId in parentIds)
            {
                MapNode parentNode = graph.Nodes.Find(n => n.Id == parentId);
                
                if (parentNode != null && parentNode.Encounter is ShopEncounter)
                {
                    return false;
                }
            }

            return true;
        }
    }
}