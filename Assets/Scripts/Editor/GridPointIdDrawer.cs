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
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            try
            {
                if (property == null || property.serializedObject == null) return;

                GridPointIdAttribute pointAttr = (GridPointIdAttribute)attribute;
                
                SerializedProperty gridProp = property.serializedObject.FindProperty("_targetGrid");
                GridConfig config = gridProp != null ? gridProp.objectReferenceValue as GridConfig : null;

                List<string> availableIds = new List<string>();
                    
                if (pointAttr.FilterType == NodeType.Base)
                {
                    availableIds.Add("[Ближайшая по пути]"); 
                        
                    if (string.IsNullOrEmpty(property.stringValue))
                        property.stringValue = "[Ближайшая по пути]";
                }

                if (config != null)
                {
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
                }

                // 3. Рисуем интерфейс
                if (availableIds.Count > 0)
                {

                    int currentIndex = Mathf.Max(0, availableIds.IndexOf(property.stringValue));
                    currentIndex = EditorGUI.Popup(position, label.text, currentIndex, availableIds.ToArray());
                    property.stringValue = availableIds[currentIndex];
                }
                else
                {

                    position.height = EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(position, property, label);
                    
                    if (config == null)
                    {

                        Rect warningRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y + position.height, position.width - EditorGUIUtility.labelWidth, position.height);
                        GUI.Label(warningRect, "Укажите TargetGrid выше!", EditorStyles.helpBox);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GridPointIdDrawer] Ошибка отрисовки: {ex.Message}");
                EditorGUI.PropertyField(position, property, label);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {

            SerializedProperty gridProp = property.serializedObject.FindProperty("_targetGrid");
            if (gridProp != null && gridProp.objectReferenceValue == null)
            {
                return EditorGUIUtility.singleLineHeight * 2.2f; 
            }
            
            return EditorGUIUtility.singleLineHeight;
        }
    }
}