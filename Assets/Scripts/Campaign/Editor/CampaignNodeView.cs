using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor;
using Gameplay.Campaign.Data;
using Gameplay.Levels.Data;
using System.Reflection;

namespace Gameplay.Campaign.Editor
{
    public class CampaignNodeView : Node
    {
        public MapNode NodeData { get; private set; }
        public Port InputPort { get; private set; }
        public Port OutputPort { get; private set; }

        public CampaignNodeView(MapNode nodeData)
        {
            NodeData = nodeData;
            
            UpdateTitle();

            float editorX = nodeData.RenderPosition.x * CampaignGraphView.EditorScaleX;
            float editorY = nodeData.RenderPosition.y * CampaignGraphView.EditorScaleY; 
            SetPosition(new Rect(editorX, editorY, 150, 150));

            CreatePorts();
            DrawEncounterConfig(); 

            RefreshExpandedState();
            RefreshPorts();
        }

        private void UpdateTitle()
        {
            title = NodeData.Encounter != null ? NodeData.Encounter.DisplayName : "Пустой узел";
            
            if (NodeData.Encounter == null) return;

            var attribute = NodeData.Encounter.GetType().GetCustomAttribute<EncounterNodeAttribute>();
            if (attribute != null && ColorUtility.TryParseHtmlString(attribute.HexColor, out Color color))
            {
                titleContainer.style.backgroundColor = new StyleColor(color);
            }
        }

        private void CreatePorts()
        {
            InputPort = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(bool));
            InputPort.portName = "In";
            inputContainer.Add(InputPort);

            OutputPort = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
            OutputPort.portName = "Out";
            outputContainer.Add(OutputPort);
        }

        private void DrawEncounterConfig()
        {
            if (NodeData.Encounter == null) return;

            var attribute = NodeData.Encounter.GetType().GetCustomAttribute<EncounterNodeAttribute>();
            if (attribute == null) return;

            extensionContainer.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f));

            if (attribute.ConfigType == typeof(Sprite))
            {
                var field = new ObjectField("Start Icon")
                {
                    objectType = typeof(Sprite),
                    value = GetPrivateField<Sprite>(NodeData.Encounter, attribute.FieldName)
                };
                field.RegisterValueChangedCallback(evt => SetPrivateField(NodeData.Encounter, attribute.FieldName, evt.newValue));
                extensionContainer.Add(field);
            }
            else
            {
                CreateConfigEditor(NodeData.Encounter, attribute.FieldName, attribute.ConfigType.Name, attribute.ConfigType);
            }

            expanded = true; 
        }

        private void CreateConfigEditor(object encounter, string fieldName, string label, System.Type configType)
        {
            var currentConfig = GetPrivateField<Object>(encounter, fieldName);

            var objectField = new ObjectField(label)
            {
                objectType = configType,
                value = currentConfig
            };

            var inspectorContainer = new VisualElement();

            void RefreshInspector(Object config)
            {
                inspectorContainer.Clear();

                if (config != null)
                {
                    var serializedObject = new SerializedObject(config);
                    
                    var inspector = new InspectorElement(serializedObject);
                    
                    var foldout = new Foldout 
                    { 
                        text = $"Edit {config.name}", 
                        value = false 
                    };
                    
                    foldout.style.marginLeft = 10;
                    foldout.style.marginBottom = 5;

                    foldout.Add(inspector);
                    inspectorContainer.Add(foldout);
                }
            }

            objectField.RegisterValueChangedCallback(evt =>
            {
                SetPrivateField(encounter, fieldName, evt.newValue);
                UpdateTitle();
                RefreshInspector(evt.newValue);
            });

            RefreshInspector(currentConfig);

            extensionContainer.Add(objectField);
            extensionContainer.Add(inspectorContainer);
        }

        private void SetPrivateField(object target, string fieldName, object value)
        {
            var fieldInfo = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fieldInfo != null) fieldInfo.SetValue(target, value);
        }

        private T GetPrivateField<T>(object target, string fieldName)
        {
            var fieldInfo = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return fieldInfo != null ? (T)fieldInfo.GetValue(target) : default;
        }
    }
}