using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using Gameplay.Campaign.Data;
using Gameplay.Campaign.Services;
using Gameplay.MapScene.Data;
using Gameplay.MapScene.Views;

namespace Gameplay.MapScene.Services
{
    public class MapSceneBuilder : IInitializable
    {
        private readonly RunProgressModel _progressModel;
        private readonly IRunDirectorService _runDirector;
        private readonly MapSceneConfig _config;
        private readonly Transform _mapRoot;
        private readonly IInstantiator _instantiator;

        private readonly Dictionary<string, MapNodeView> _spawnedNodes = new Dictionary<string, MapNodeView>();

        public event Action<MapNode> OnNodeSelected;

        public MapSceneBuilder(
            RunProgressModel progressModel,
            IRunDirectorService runDirector,
            MapSceneConfig config,
            [Inject(Id = "MapRoot")] Transform mapRoot,
            IInstantiator instantiator)
        {
            _progressModel = progressModel;
            _runDirector = runDirector;
            _config = config;
            _mapRoot = mapRoot;
            _instantiator = instantiator;
        }

        public void Initialize()
        {
            if (_progressModel.CurrentMap == null || _progressModel.CurrentMap.Nodes.Count == 0)
            {
                Debug.LogWarning("[MapSceneBuilder] Карта пуста. Генерация...");
                _runDirector.GenerateRunMap(_progressModel);
            }

            BuildVisualMap();
        }

        private void BuildVisualMap()
        {
            var nodes = _progressModel.CurrentMap.Nodes;
            
            CalculatePositions(nodes.Values);
            DrawConnections(nodes.Values);
            UpdateNodeStates();
        }

        private void CalculatePositions(IEnumerable<MapNode> nodes)
        {
            var nodesByLayer = new Dictionary<int, List<MapNode>>();
            foreach (var node in nodes)
            {
                if (!nodesByLayer.ContainsKey(node.Depth))
                    nodesByLayer[node.Depth] = new List<MapNode>();
                nodesByLayer[node.Depth].Add(node);
            }

            foreach (var layer in nodesByLayer)
            {
                int depth = layer.Key;
                List<MapNode> layerNodes = layer.Value;
                int nodeCount = layerNodes.Count;

                float startX = -(nodeCount - 1) * _config.NodeXSpacing / 2f;
                float currentY = _config.StartYOffset + (depth * _config.LayerYSpacing);

                for (int i = 0; i < nodeCount; i++)
                {
                    MapNode nodeData = layerNodes[i];
                    Vector3 worldPos = new Vector3(startX + (i * _config.NodeXSpacing), currentY, 0f);

                    MapNodeView view = _instantiator.InstantiatePrefabForComponent<MapNodeView>(
                        _config.NodePrefab, worldPos, Quaternion.identity, _mapRoot);
                    
                    view.Setup(nodeData.Id, nodeData.NodeDisplayName, worldPos, nodeData.NodeIcon);
                    view.OnNodeClicked += HandleNodeClicked;

                    _spawnedNodes.Add(nodeData.Id, view);
                }
            }
        }

        private void DrawConnections(IEnumerable<MapNode> nodes)
        {
            foreach (var node in nodes)
            {
                if (!_spawnedNodes.TryGetValue(node.Id, out MapNodeView startView)) continue;

                foreach (string nextId in node.NextNodeIds)
                {
                    if (_spawnedNodes.TryGetValue(nextId, out MapNodeView endView))
                    {
                        MapLineView line = _instantiator.InstantiatePrefabForComponent<MapLineView>(
                            _config.LinePrefab, Vector3.zero, Quaternion.identity, _mapRoot);
                        
                        line.Setup(startView.transform.position, endView.transform.position);
                    }
                }
            }
        }

        private void UpdateNodeStates()
        {
            foreach (var view in _spawnedNodes.Values)
            {
                view.SetState(NodeVisualState.Locked);
            }

            var availableChoices = _runDirector.GetAvailableChoices(_progressModel);
            foreach (var choice in availableChoices)
            {
                if (_spawnedNodes.TryGetValue(choice.Id, out MapNodeView view))
                {
                    view.SetState(NodeVisualState.Available);
                }
            }

            if (_progressModel.CurrentNode != null)
            {
                if (_spawnedNodes.TryGetValue(_progressModel.CurrentNode.Id, out MapNodeView currentView))
                {
                    currentView.SetState(NodeVisualState.Completed);
                }
            }
        }

        private void HandleNodeClicked(string nodeId)
        {
            var availableChoices = _runDirector.GetAvailableChoices(_progressModel);
            var choice = availableChoices.FirstOrDefault(c => c.Id == nodeId);

            if (choice != null)
            {
                OnNodeSelected?.Invoke(choice);
            }
            else
            {
                Debug.Log($"<color=yellow>[MapSceneBuilder] Узел {nodeId} недоступен для перехода!</color>");
            }
        }
    }
}