using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Gameplay.Campaign.Data;
using Gameplay.Levels.Data;

namespace Gameplay.Campaign.Services
{
    public class ProceduralRunDirector : IRunDirectorService
    {
        private readonly ProceduralGenerationConfig _config;

        public ProceduralRunDirector(ProceduralGenerationConfig config)
        {
            _config = config;
        }

        public void GenerateRunMap(RunProgressModel progress)
        {
            progress.CurrentMap = new RunMapModel();
            HashSet<LevelBlueprintConfig> usedCombatLevels = new HashSet<LevelBlueprintConfig>();
            List<List<MapNode>> layers = new List<List<MapNode>>();
            

            for (int depth = 0; depth <= _config.MaxDepth; depth++)
            {
                List<MapNode> layerNodes = new List<MapNode>();

                int nodeCount = (depth == 0 || depth == _config.MaxDepth) ? 1 : Random.Range(2, 4);

                for (int i = 0; i < nodeCount; i++)
                {
                    MapNode node = CreateRandomNode(depth, i, usedCombatLevels);
                    layerNodes.Add(node);
                    progress.CurrentMap.Nodes[node.Id] = node;
                }
                layers.Add(layerNodes);
            }


            for (int depth = 0; depth < _config.MaxDepth; depth++)
            {
                var currentLayer = layers[depth];
                var nextLayer = layers[depth + 1];


                if (currentLayer.Count == 1)
                {
                    foreach (var n in nextLayer) currentLayer[0].NextNodeIds.Add(n.Id);
                    continue;
                }

                if (nextLayer.Count == 1)
                {
                    foreach (var c in currentLayer) c.NextNodeIds.Add(nextLayer[0].Id);
                    continue;
                }


                for (int i = 0; i < currentLayer.Count; i++)
                {
                    int targetIndex = Mathf.Min(i, nextLayer.Count - 1);
                    currentLayer[i].NextNodeIds.Add(nextLayer[targetIndex].Id);


                    if (i + 1 < nextLayer.Count && Random.value > 0.5f)
                    {
                        currentLayer[i].NextNodeIds.Add(nextLayer[i + 1].Id);
                    }
                }
            }

            string startNodeId = layers[0][0].Id;
            progress.CurrentMap.StartingNodeIds.Add(startNodeId);
            
            if (progress.CurrentNode == null)
            {
                AdvanceToNode(progress, startNodeId);
            }
        }

        private MapNode CreateRandomNode(int depth, int indexInLayer, HashSet<LevelBlueprintConfig> usedLevels)
        {
            MapNode node = new MapNode { Id = $"proc_node_{depth}_{indexInLayer}", Depth = depth };


            if (depth == 0)
            {
                node.NodeType = MapNodeType.Event;
                node.NodeDisplayName = "Start";
                node.NodeIcon = _config.StartNodeIcon;
                return node;
            }


            if (depth == _config.MaxDepth)
            {
                node.NodeType = MapNodeType.Combat;
                node.NodeDisplayName = "BOSS";
                node.CombatLevel = _config.BossLevel;
                node.NodeIcon = _config.BossLevel != null ? _config.BossLevel.MapIcon : null;
                return node;
            }

            TierConfig tier = _config.Tiers.FirstOrDefault(t => depth >= t.MinDepth && depth <= t.MaxDepth);
            if (tier == null) return node;

            float totalWeight = tier.CombatWeight + tier.ShopWeight + tier.EventWeight;
            float roll = Random.Range(0, totalWeight);

            if (roll < tier.CombatWeight)
            {
                node.NodeType = MapNodeType.Combat;
                node.NodeDisplayName = "Battle";
                var availableLevels = tier.CombatPool.Where(lvl => lvl != null && !usedLevels.Contains(lvl)).ToList();

                if (availableLevels.Count > 0)
                {
                    LevelBlueprintConfig chosenLevel = availableLevels[Random.Range(0, availableLevels.Count)];
                    node.CombatLevel = chosenLevel;
                    node.NodeIcon = chosenLevel.MapIcon;
                    usedLevels.Add(chosenLevel);
                }
                else
                {
                    var anyValid = tier.CombatPool.Where(lvl => lvl != null).ToList();
                    node.CombatLevel = anyValid.Count > 0 ? anyValid[Random.Range(0, anyValid.Count)] : _config.BossLevel;
                    node.NodeIcon = node.CombatLevel?.MapIcon;
                }
            }
            else if (roll < tier.CombatWeight + tier.ShopWeight)
            {
                node.NodeType = MapNodeType.Shop;
                node.NodeDisplayName = "Shop";
                var validShops = tier.ShopPool.Where(s => s != null).ToList();
                if (validShops.Count > 0)
                {
                    node.ShopData = validShops[Random.Range(0, validShops.Count)];
                    node.NodeIcon = node.ShopData.MapIcon;
                }
            }
            else
            {
                node.NodeType = MapNodeType.Event;
                node.NodeDisplayName = "Event";
                var validEvents = tier.EventPool.Where(e => e != null).ToList();
                if (validEvents.Count > 0)
                {
                    node.EventData = validEvents[Random.Range(0, validEvents.Count)];
                    node.NodeIcon = node.EventData.MapIcon;
                }
            }

            return node;
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