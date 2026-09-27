using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Gameplay.Campaign.Data;
using UnityEditor;
using System.Reflection;
using System;

namespace Gameplay.Campaign.Editor
{
    public class CampaignGraphView : GraphView
    {
        public const float EditorScaleX = 250f;
        public const float EditorScaleY = -200f;

        private Dictionary<string, CampaignNodeView> _nodeViews = new Dictionary<string, CampaignNodeView>();
        
        public CampaignGraphAsset CurrentAsset { get; private set; }

        public CampaignGraphView()
        {
            style.flexGrow = 1;
            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }

        public void PopulateView(CampaignGraphAsset asset)
        {
            ClearGraph();
            CurrentAsset = asset;

            if (asset == null || asset.Nodes.Count == 0) return;

            foreach (var mapNode in asset.Nodes)
            {
                var nodeView = new CampaignNodeView(mapNode);
                _nodeViews.Add(mapNode.Id, nodeView);
                AddElement(nodeView);
            }

            foreach (var mapNode in asset.Nodes)
            {
                if (!_nodeViews.TryGetValue(mapNode.Id, out var parentView)) continue;

                foreach (var childId in mapNode.NextNodeIds)
                {
                    if (_nodeViews.TryGetValue(childId, out var childView))
                    {
                        Edge edge = parentView.OutputPort.ConnectTo(childView.InputPort);
                        AddElement(edge);
                    }
                }
            }
        }

        public void ClearGraph()
        {
            CurrentAsset = null;
            DeleteElements(graphElements.ToList());
            _nodeViews.Clear();
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            ports.ForEach(port =>
            {
                if (startPort == port) return;
                if (startPort.direction == port.direction) return;
                if (startPort.node == port.node) return;
                compatiblePorts.Add(port);
            });
            return compatiblePorts;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            if (CurrentAsset == null) return;

            var types = TypeCache.GetTypesDerivedFrom<INodeEncounter>();

            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute<EncounterNodeAttribute>();
                if (attribute != null)
                {
                    evt.menu.AppendAction(attribute.MenuPath, a => 
                    {
                        var instance = (INodeEncounter)Activator.CreateInstance(type, new object[] { null });
                        CreateNewNode(instance, a.eventInfo.localMousePosition);
                    });
                }
            }
        }

        private void CreateNewNode(INodeEncounter encounter, Vector2 mousePosition)
        {
            Vector2 localPos = contentViewContainer.WorldToLocal(mousePosition);

            MapNode newNodeData = new MapNode
            {
                Id = System.Guid.NewGuid().ToString(),
                Encounter = encounter,
                RenderPosition = new Vector2(localPos.x / EditorScaleX, localPos.y / EditorScaleY) 
            };

            CurrentAsset.Nodes.Add(newNodeData);

            var nodeView = new CampaignNodeView(newNodeData);
            _nodeViews.Add(newNodeData.Id, nodeView);
            AddElement(nodeView);
        }
    }
}