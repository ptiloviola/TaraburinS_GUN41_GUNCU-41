using System.Collections.Generic;
using UnityEngine;
using Gameplay.Campaign.Data;

namespace Gameplay.Campaign.Rules
{
    [CreateAssetMenu(fileName = "MaxShopsPerPath", menuName = "TD/Campaign/Rules/Max Shops Per Path")]
    public class MaxShopsPerPathRule : NodePlacementRule
    {
        [SerializeField] private int _maxShopsAllowed = 2;

        public override bool Validate(MapNode node, INodeEncounter candidate, Dictionary<string, List<string>> parentsMap, CampaignGraphAsset graph)
        {
            if (!(candidate is ShopEncounter)) return true;

            return CheckPathRecursively(node.Id, 1, parentsMap, graph);
        }

        private bool CheckPathRecursively(string currentNodeId, int currentShopCount, Dictionary<string, List<string>> parentsMap, CampaignGraphAsset graph)
        {
            if (currentShopCount > _maxShopsAllowed) return false;

            if (!parentsMap.TryGetValue(currentNodeId, out List<string> parentIds)) 
                return true;

            foreach (string parentId in parentIds)
            {
                MapNode parentNode = graph.Nodes.Find(n => n.Id == parentId);
                int shopsOnThisBranch = currentShopCount;

                if (parentNode != null && parentNode.Encounter is ShopEncounter)
                {
                    shopsOnThisBranch++;
                }

                if (!CheckPathRecursively(parentId, shopsOnThisBranch, parentsMap, graph))
                {
                    return false;
                }
            }

            return true;
        }
    }
}