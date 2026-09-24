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

                foreach (var node in currentLayer)
                {
                    var target = nextLayer[Random.Range(0, nextLayer.Count)];
                    node.NextNodeIds.Add(target.Id);
                }

                foreach (var nextNode in nextLayer)
                {
                    bool hasIncoming = currentLayer.Any(n => n.NextNodeIds.Contains(nextNode.Id));
                    if (!hasIncoming)
                    {
                        var source = currentLayer[Random.Range(0, currentLayer.Count)];
                        if (!source.NextNodeIds.Contains(nextNode.Id))
                        {
                            source.NextNodeIds.Add(nextNode.Id);
                        }
                    }
                }
            }

            foreach (var node in layers[0])
            {
                progress.CurrentMap.StartingNodeIds.Add(node.Id);
            }
        }

        private MapNode CreateRandomNode(int depth, int indexInLayer, HashSet<LevelBlueprintConfig> usedLevels)
        {
            MapNode node = new MapNode
            {
                Id = $"proc_node_{depth}_{indexInLayer}",
                Depth = depth,
                RenderPosition = new Vector2(depth * 200, indexInLayer * 150)
            };

            if (depth == _config.MaxDepth)
            {
                node.NodeType = MapNodeType.Combat;
                node.NodeDisplayName = "BOSS";
                node.CombatLevel = _config.BossLevel;
                return node;
            }

            TierConfig tier = _config.Tiers.FirstOrDefault(t => depth >= t.MinDepth && depth <= t.MaxDepth);
            

            if (tier == null)
            {
                Debug.LogError($"[ProceduralRunDirector] ДЫРА В ТИРАХ! Для глубины {depth} не настроен Tier. Настройте Min/Max Depth!");
                node.NodeType = MapNodeType.Combat;
                node.NodeDisplayName = "Tier Error";
                node.CombatLevel = _config.BossLevel;
                return node;
            }

            float totalWeight = tier.CombatWeight + tier.ShopWeight + tier.EventWeight;
            float roll = Random.Range(0, totalWeight);

            if (roll < tier.CombatWeight)
            {
                node.NodeType = MapNodeType.Combat;
                node.NodeDisplayName = "Battle";
                
                var availableLevels = tier.CombatPool
                    .Where(lvl => lvl != null && !usedLevels.Contains(lvl))
                    .ToList();

                if (availableLevels.Count > 0)
                {
                    LevelBlueprintConfig chosenLevel = availableLevels[Random.Range(0, availableLevels.Count)];
                    node.CombatLevel = chosenLevel;
                    usedLevels.Add(chosenLevel);
                }
                else
                {
                    Debug.LogWarning($"[ProceduralRunDirector] На глубине {depth} закончились уникальные уровни в пуле!");
                    var anyValid = tier.CombatPool.Where(lvl => lvl != null).ToList();
                    node.CombatLevel = anyValid.Count > 0 ? anyValid[Random.Range(0, anyValid.Count)] : _config.BossLevel;
                }
            }
            else if (roll < tier.CombatWeight + tier.ShopWeight)
            {
                node.NodeType = MapNodeType.Shop;
                node.NodeDisplayName = "Shop";
                var validShops = tier.ShopPool.Where(s => s != null).ToList();
                node.ShopData = validShops.Count > 0 ? validShops[Random.Range(0, validShops.Count)] : null;
            }
            else
            {
                node.NodeType = MapNodeType.Event;
                node.NodeDisplayName = "Event";
                var validEvents = tier.EventPool.Where(e => e != null).ToList();
                node.EventData = validEvents.Count > 0 ? validEvents[Random.Range(0, validEvents.Count)] : null;
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
            }
        }

        public bool IsCampaignCompleted(RunProgressModel progress)
        {
            if (progress.CurrentNode == null) return false;
            return progress.CurrentNode.NextNodeIds.Count == 0;
        }
    }
}