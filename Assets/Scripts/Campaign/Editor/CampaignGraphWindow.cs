using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Linq;
using Gameplay.Campaign.Data;

namespace Gameplay.Campaign.Editor
{
    public class CampaignGraphWindow : EditorWindow
    {
        private CampaignGraphView _graphView;
        
        private CampaignGraphAsset _currentAsset; 

        [MenuItem("TD/Campaign Graph Editor")]
        public static void OpenWindow()
        {
            var window = GetWindow<CampaignGraphWindow>();
            window.titleContent = new GUIContent("Campaign Graph");
            window.Show();
        }

        private void OnEnable()
        {
            ConstructGraphView();
            GenerateToolbar();
        }

        private void OnDisable()
        {
            if (_graphView != null) rootVisualElement.Remove(_graphView); 
        }

        private void ConstructGraphView()
        {
            _graphView = new CampaignGraphView();
            _graphView.StretchToParentSize(); 
            rootVisualElement.Add(_graphView); 
        }

        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();

            var objectField = new ObjectField("Graph Asset")
            {
                objectType = typeof(CampaignGraphAsset),
                allowSceneObjects = false
            };

            objectField.RegisterValueChangedCallback(evt =>
            {
                _currentAsset = evt.newValue as CampaignGraphAsset;
                if (_currentAsset != null)
                {
                    _graphView.PopulateView(_currentAsset);
                }
                else
                {
                    _graphView.ClearGraph();
                }
            });

            toolbar.Add(objectField);

            var saveButton = new Button(() => SaveGraph()) 
            { 
                text = "Save Graph" 
            };
            toolbar.Add(saveButton);

            rootVisualElement.Add(toolbar);
        }

        private void SaveGraph()
        {
            if (_currentAsset == null) return;

            var nodeViews = _graphView.nodes.ToList().Cast<CampaignNodeView>().ToList();
            var edges = _graphView.edges.ToList();

            var validNodeIds = nodeViews.Select(n => n.NodeData.Id).ToList();

            _currentAsset.Nodes.RemoveAll(n => !validNodeIds.Contains(n.Id));
            _currentAsset.StartingNodeIds.RemoveAll(id => !validNodeIds.Contains(id));

            foreach (var nodeView in nodeViews)
            {
                Rect pos = nodeView.GetPosition();
                nodeView.NodeData.RenderPosition = new Vector2(
                    pos.xMin / CampaignGraphView.EditorScaleX, 
                    pos.yMin / CampaignGraphView.EditorScaleY
                );
                nodeView.NodeData.NextNodeIds.Clear();
            }

            foreach (var edge in edges)
            {
                var parentView = edge.output.node as CampaignNodeView;
                var childView = edge.input.node as CampaignNodeView;

                if (parentView != null && childView != null)
                {
                    parentView.NodeData.NextNodeIds.Add(childView.NodeData.Id);
                }
            }

            EditorUtility.SetDirty(_currentAsset);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"<color=cyan>[Graph Editor] Граф успешно сохранен в {_currentAsset.name}!</color>");
        }
    }
}