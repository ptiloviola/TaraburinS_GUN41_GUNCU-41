using UnityEngine;
using UnityEditor;
using Gameplay.Campaign.Data;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Campaign.Rules;

namespace Gameplay.Campaign.Editor
{
    [CustomEditor(typeof(CampaignGraphAsset))]
    public class CampaignGraphAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            CampaignGraphAsset asset = (CampaignGraphAsset)target;

            GUILayout.Space(10);
            if (GUILayout.Button("Сгенерировать Тестовую Карту (Старт -> Босс)", GUILayout.Height(30)))
            {
                GenerateTestMap(asset);
            }

            GUILayout.Space(10);
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("СГЕНЕРИРОВАТЬ ИЗ КОЛОДЫ", GUILayout.Height(40)))
            {
                if (asset.GeneratorConfig == null)
                {
                    Debug.LogError("[Editor] Назначьте GeneratorConfig!");
                    return;
                }
                GenerateDeckMapWithRetries(asset);
            }
            GUI.backgroundColor = Color.white;
        }

        private void GenerateTestMap(CampaignGraphAsset asset)
        {
            asset.Clear();
            MapNode startNode = new MapNode { Id = "start", Depth = 0, RenderPosition = new Vector2(0, 0), NextNodeIds = new List<string> { "boss" }, Encounter = new StartEncounter(null) };
            MapNode bossNode = new MapNode { Id = "boss", Depth = 1, RenderPosition = new Vector2(0, 3), NextNodeIds = new List<string>(), Encounter = new CombatEncounter(null) };
            asset.Nodes.Add(startNode);
            asset.Nodes.Add(bossNode);
            asset.StartingNodeIds.Add("start");
            SaveAsset(asset);
        }

        private void GenerateDeckMapWithRetries(CampaignGraphAsset asset)
        {
            int maxAttempts = 100;
            for (int i = 0; i < maxAttempts; i++)
            {
                if (TryGenerateDeckMap(asset))
                {
                    SaveAsset(asset);
                    Debug.Log($"<color=green>[Editor] Карта успешно сгенерирована за {i + 1} попыток! Узлов: {asset.Nodes.Count}</color>");
                    return;
                }
            }

            Debug.LogError($"<color=red>[Editor] Не удалось сгенерировать карту за {maxAttempts} попыток. " +
                           "Правила конфликтуют с размером колоды или топологией!</color>");
        }

        private bool TryGenerateDeckMap(CampaignGraphAsset asset)
        {
            var config = asset.GeneratorConfig;

            var backupNodes = new List<MapNode>(asset.Nodes);
            var backupStarts = new List<string>(asset.StartingNodeIds);
            
            asset.Clear();
            
            List<List<MapNode>> layers = new List<List<MapNode>>();
            int totalIntermediateNodes = 0;
            float nodeXSpacing = 2.5f;
            float layerYSpacing = 3f;
            float startYOffset = -4f;

            for (int depth = 0; depth <= config.MaxDepth; depth++)
            {
                List<MapNode> layerNodes = new List<MapNode>();
                int nodeCount = (depth == 0 || depth == config.MaxDepth) ? 1 : Random.Range(config.MinNodesPerLayer, config.MaxNodesPerLayer + 1);

                float startX = -(nodeCount - 1) * nodeXSpacing / 2f;
                float currentY = startYOffset + (depth * layerYSpacing);

                for (int i = 0; i < nodeCount; i++)
                {
                    MapNode node = new MapNode 
                    { 
                        Id = $"node_{depth}_{i}", 
                        Depth = depth,
                        RenderPosition = new Vector2(startX + (i * nodeXSpacing), currentY)
                    };
                    layerNodes.Add(node);
                    asset.Nodes.Add(node);
                    
                    if (depth > 0 && depth < config.MaxDepth) totalIntermediateNodes++;
                }
                layers.Add(layerNodes);
            }

            for (int depth = 0; depth < config.MaxDepth; depth++)
            {
                var currentLayer = layers[depth];
                var nextLayer = layers[depth + 1];
                List<Vector2Int> edges = new List<Vector2Int>();
                int currentCount = currentLayer.Count;
                int nextCount = nextLayer.Count;

                int maxNodes = Mathf.Max(currentCount, nextCount);
                for (int k = 0; k < maxNodes; k++)
                {
                    int i = Mathf.Min(k, currentCount - 1);
                    int j = Mathf.Min(k, nextCount - 1);
                    edges.Add(new Vector2Int(i, j));
                }

                for (int i = 0; i < currentCount; i++)
                {
                    for (int j = 0; j < nextCount; j++)
                    {
                        Vector2Int candidate = new Vector2Int(i, j);
                        if (edges.Contains(candidate)) continue;

                        bool crosses = false;
                        foreach (var edge in edges)
                        {
                            if (edge.x < candidate.x && edge.y > candidate.y) crosses = true;
                            if (edge.x > candidate.x && edge.y < candidate.y) crosses = true;
                        }

                        if (!crosses && Random.value > 0.7f) edges.Add(candidate);
                    }
                }

                foreach (var edge in edges)
                    currentLayer[edge.x].NextNodeIds.Add(nextLayer[edge.y].Id);
            }

            List<INodeEncounter> deck = new List<INodeEncounter>();
            for (int i = 0; i < config.ExactCombats; i++) deck.Add(new CombatEncounter(GetRandomFromPool(config.CombatPool)));
            for (int i = 0; i < config.ExactShops; i++) deck.Add(new ShopEncounter(GetRandomFromPool(config.ShopPool)));
            for (int i = 0; i < config.ExactEvents; i++) deck.Add(new EventEncounter(GetRandomFromPool(config.EventPool)));

            while (deck.Count < totalIntermediateNodes) deck.Add(new CombatEncounter(GetRandomFromPool(config.CombatPool)));

            var rng = new System.Random();
            deck = deck.OrderBy(x => rng.Next()).ToList();

            Dictionary<string, List<string>> parentsMap = new Dictionary<string, List<string>>();
            foreach (var n in asset.Nodes)
            {
                foreach (var childId in n.NextNodeIds)
                {
                    if (!parentsMap.ContainsKey(childId)) parentsMap[childId] = new List<string>();
                    parentsMap[childId].Add(n.Id);
                }
            }

            foreach (var node in asset.Nodes)
            {
                if (node.Depth == 0)
                {
                    node.Encounter = new StartEncounter(config.StartIcon);
                    asset.StartingNodeIds.Add(node.Id);
                    continue;
                }
                if (node.Depth == config.MaxDepth)
                {
                    node.Encounter = new CombatEncounter(config.BossLevel);
                    continue;
                }

                INodeEncounter chosenCard = null;
                int chosenIndex = -1;

                for (int i = 0; i < deck.Count; i++)
                {
                    INodeEncounter candidate = deck[i];
                    bool isValid = true;

                    foreach (IGraphRule rule in config.Rules)
                    {
                        if (!rule.Validate(node, candidate, parentsMap, asset))
                        {
                            isValid = false;
                            break;
                        }
                    }

                    if (isValid)
                    {
                        chosenCard = candidate;
                        chosenIndex = i;
                        break;
                    }
                }

                if (chosenCard == null)
                {
                    asset.Nodes = backupNodes;
                    asset.StartingNodeIds = backupStarts;
                    return false; 
                }

                node.Encounter = chosenCard;
                deck.RemoveAt(chosenIndex);
            }

            return true;
        }

        private T GetRandomFromPool<T>(List<T> pool) where T : ScriptableObject
        {
            if (pool == null || pool.Count == 0) return null;
            return pool[Random.Range(0, pool.Count)];
        }

        private void SaveAsset(CampaignGraphAsset asset)
        {
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        }
    }
}