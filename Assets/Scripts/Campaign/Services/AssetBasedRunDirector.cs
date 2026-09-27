using System.Collections.Generic;
using UnityEngine;
using Gameplay.Campaign.Data;

namespace Gameplay.Campaign.Services
{
    public class AssetBasedRunDirector : IRunDirectorService
    {
        private readonly CampaignGraphAsset _graphAsset;

        public AssetBasedRunDirector(CampaignGraphAsset graphAsset)
        {
            _graphAsset = graphAsset;
        }

        public void GenerateRunMap(RunProgressModel progress)
        {
            progress.CurrentMap = new RunMapModel();
            progress.PathHistory.Clear();
            progress.CurrentRunDepth = 0;
            progress.CurrentNode = null;

            if (_graphAsset == null || _graphAsset.Nodes.Count == 0)
            {
                Debug.LogError("[AssetBasedRunDirector] CampaignGraphAsset пуст или не назначен!");
                return;
            }

            foreach (var node in _graphAsset.Nodes)
            {
                var clonedNode = new MapNode
                {
                    Id = node.Id,
                    Depth = node.Depth,
                    RenderPosition = node.RenderPosition,
                    NextNodeIds = new List<string>(node.NextNodeIds),
                    Encounter = node.Encounter
                };
                
                progress.CurrentMap.Nodes[clonedNode.Id] = clonedNode;
            }

            progress.CurrentMap.StartingNodeIds = new List<string>(_graphAsset.StartingNodeIds);

            if (progress.CurrentNode == null && progress.CurrentMap.StartingNodeIds.Count > 0)
            {
                AdvanceToNode(progress, progress.CurrentMap.StartingNodeIds[0]);
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
                progress.CurrentRunDepth = nextNode.Depth;
                
                if (!progress.PathHistory.Contains(nodeId))
                {
                    progress.PathHistory.Add(nodeId);
                }
            }
        }

        public bool IsCampaignCompleted(RunProgressModel progress)
        {
            if (progress.CurrentNode == null) return false;
            return progress.CurrentNode.NextNodeIds.Count == 0;
        }
    }
}