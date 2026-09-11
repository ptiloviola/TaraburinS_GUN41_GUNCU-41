using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Gameplay.Spawning.Data;
using Gameplay.Grid;

namespace Gameplay.Editor
{
    [CustomPropertyDrawer(typeof(GridPointIdAttribute))]
    public class GridPointIdDrawer : PropertyDrawer
    {
        // Кэшируем ссылку на компонент на сцене
        private GridGizmosDrawer _cachedDrawer;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GridPointIdAttribute pointAttr = (GridPointIdAttribute)attribute;

            // Ищем объект на сцене только один раз
            if (_cachedDrawer == null)
            {
                _cachedDrawer = Object.FindObjectOfType<GridGizmosDrawer>();
            }

            if (_cachedDrawer != null && _cachedDrawer.EditorConfig != null)
            {
                List<string> availableIds = new List<string>();
                
                if (pointAttr.FilterType == NodeType.Base)
                {
                    availableIds.Add("[Ближайшая по пути]"); 
                    
                    if (string.IsNullOrEmpty(property.stringValue))
                        property.stringValue = "[Ближайшая по пути]";
                }

                GridConfig config = _cachedDrawer.EditorConfig;
                
                // Проход по массиву в памяти - это быстро, в отличие от FindObjectOfType
                for (int x = 0; x < config.width; x++)
                {
                    for (int z = 0; z < config.height; z++)
                    {
                        if (config.GetCellData(x, z).type == pointAttr.FilterType)
                        {
                            string prefix = pointAttr.FilterType == NodeType.Spawn ? "Spawn" : "Base";
                            availableIds.Add($"{prefix}_{x}_{z}");
                        }
                    }
                }

                if (availableIds.Count > 0)
                {
                    int currentIndex = Mathf.Max(0, availableIds.IndexOf(property.stringValue));
                    currentIndex = EditorGUI.Popup(position, label.text, currentIndex, availableIds.ToArray());
                    property.stringValue = availableIds[currentIndex];
                    return;
                }
            }

            EditorGUI.PropertyField(position, property, label);
        }
    }
}