using System.Collections.Generic;
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

        public void GenerateRunMap(RunProgressModel progress)
        {
            progress.CurrentMap = new RunMapModel();

            for (int i = 0; i < _campaignConfig.Nodes.Count; i++)
            {
                MapNode node = _campaignConfig.Nodes[i];
                node.Id = $"linear_node_{i}";
                node.Depth = i;


                node.NextNodeIds.Clear();
                if (i < _campaignConfig.Nodes.Count - 1)
                {
                    node.NextNodeIds.Add($"linear_node_{i + 1}");
                }

                progress.CurrentMap.Nodes[node.Id] = node;
            }

            if (_campaignConfig.Nodes.Count > 0)
            {
                progress.CurrentMap.StartingNodeIds.Add($"linear_node_0");
            }
        }

        public IReadOnlyList<MapNode> GetAvailableChoices(RunProgressModel progress)
        {
            List<MapNode> choices = new List<MapNode>();
            if (progress.CurrentNode == null)
            {
                foreach (string id in progress.CurrentMap.StartingNodeIds)
                    choices.Add(progress.CurrentMap.Nodes[id]);
                return choices;
            }

            foreach (string nextId in progress.CurrentNode.NextNodeIds)
                choices.Add(progress.CurrentMap.Nodes[nextId]);
            return choices;
        }

        public void AdvanceToNode(RunProgressModel progress, string nodeId)
        {
            if (progress.CurrentMap.Nodes.TryGetValue(nodeId, out MapNode nextNode))
            {
                progress.CurrentNode = nextNode;

            }
        }

        public bool IsCampaignCompleted(RunProgressModel progress)
        {
            if (progress.CurrentNode == null) return false;
            return progress.CurrentNode.NextNodeIds.Count == 0;
        }
    }
}