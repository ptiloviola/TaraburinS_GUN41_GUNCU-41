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
        private struct NodeConnection
        {
            public string StartId;
            public string EndId;
            public MapLineView LineView;

        }

        private readonly RunProgressModel _progressModel;
        private readonly IRunDirectorService _runDirector;
        private readonly MapSceneConfig _config;
        private readonly Transform _mapRoot;
        private readonly IInstantiator _instantiator;

        private readonly Dictionary<string, MapNodeView> _spawnedNodes = new Dictionary<string, MapNodeView>();
        private readonly List<NodeConnection> _spawnedLines = new List<NodeConnection>();

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
            _progressModel.LoadOrCreateData();

            bool isNewGame = false;
            
            if (_progressModel.CurrentMap == null || _progressModel.CurrentMap.Nodes.Count == 0)
            {
                _runDirector.GenerateRunMap(_progressModel);
                isNewGame = true;
            }

            if (!string.IsNullOrEmpty(_progressModel.CurrentNodeId))
            {
                if (_progressModel.CurrentMap.Nodes.TryGetValue(_progressModel.CurrentNodeId, out var savedNode))
                {
                    _progressModel.CurrentNode = savedNode;
                }
            }

            if (isNewGame && _progressModel.CurrentNode == null && _progressModel.CurrentMap.StartingNodeIds.Count > 0)
            {
                string startId = _progressModel.CurrentMap.StartingNodeIds[0];
                if (_progressModel.CurrentMap.Nodes.TryGetValue(startId, out var startNode))
                {
                    _progressModel.MoveToNode(startNode);
                    _progressModel.CompleteCurrentNode(); 
                }
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


            UnityEngine.Random.InitState(_progressModel.CurrentMap.Nodes.Count);

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

                    if (nodeData.Encounter == null)
                    {
                        Gameplay.Tools.GameLogger.LogError($"<color=red>[MapSceneBuilder] Узел {nodeData.Id} поврежден (Encounter == null)! Пропускаем отрисовку.</color>");
                        continue;
                    }

                    if (nodeData.RenderPosition == Vector2.zero)
                    {
                        float jitterX = UnityEngine.Random.Range(-_config.PositionJitter.x, _config.PositionJitter.x);
                        float jitterY = UnityEngine.Random.Range(-_config.PositionJitter.y, _config.PositionJitter.y);
                        

                        if (depth == 0 || depth == nodesByLayer.Count - 1) jitterX = 0;

                        nodeData.RenderPosition = new Vector2(startX + (i * _config.NodeXSpacing) + jitterX, currentY + jitterY);
                    }

                    MapNodeView view = _instantiator.InstantiatePrefabForComponent<MapNodeView>(
                        _config.NodePrefab, nodeData.RenderPosition, Quaternion.identity, _mapRoot);
                    

                    view.Setup(
                        nodeData.Id, 
                        nodeData.Encounter.DisplayName, 
                        nodeData.RenderPosition, 
                        nodeData.Encounter.Icon, 
                        nodeData.Encounter.GlowColor, 
                        nodeData.Depth == 0
                    );
                    
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
                        
                        _spawnedLines.Add(new NodeConnection { StartId = node.Id, EndId = nextId, LineView = line });
                    }
                }
            }
        }

        private void UpdateNodeStates()
        {
            foreach (var view in _spawnedNodes.Values) view.SetState(NodeVisualState.Locked);

            foreach (string historyId in _progressModel.PathHistory)
            {
                if (_spawnedNodes.TryGetValue(historyId, out MapNodeView historyView))
                    historyView.SetState(NodeVisualState.Completed);
            }

            var availableChoices = _runDirector.GetAvailableChoices(_progressModel);
            foreach (var choice in availableChoices)
            {
                if (_spawnedNodes.TryGetValue(choice.Id, out MapNodeView view))
                    view.SetState(NodeVisualState.Available);
            }


            foreach (var connection in _spawnedLines)
            {
                bool startCompleted = _progressModel.PathHistory.Contains(connection.StartId);
                bool endCompleted = _progressModel.PathHistory.Contains(connection.EndId);
                bool endAvailable = availableChoices.Any(c => c.Id == connection.EndId);

                if (startCompleted && endCompleted)
                    connection.LineView.SetState(NodeVisualState.Completed);
                else if (startCompleted && endAvailable)
                    connection.LineView.SetState(NodeVisualState.Available);
                else
                    connection.LineView.SetState(NodeVisualState.Locked);
            }
        }

        private void HandleNodeClicked(string nodeId)
        {
            var availableChoices = _runDirector.GetAvailableChoices(_progressModel);
            var choice = availableChoices.FirstOrDefault(c => c.Id == nodeId);

            if (choice != null) OnNodeSelected?.Invoke(choice);
        }
    }
}