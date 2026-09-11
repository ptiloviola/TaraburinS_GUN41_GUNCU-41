using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Gameplay.Spawning.Data;
using Gameplay.Enemies.Data; 

namespace Gameplay.Spawning.Editor
{
    [CustomPropertyDrawer(typeof(EnemyIdAttribute))]
    public class EnemyIdDrawer : PropertyDrawer
    {
        // Кэшируем ссылку на реестр, чтобы не "насиловать" AssetDatabase каждый кадр
        private EnemyRegistry _cachedRegistry;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            // 1. Ищем реестр только если еще не нашли
            if (_cachedRegistry == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:EnemyRegistry");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    _cachedRegistry = AssetDatabase.LoadAssetAtPath<EnemyRegistry>(path);
                }
            }

            List<string> enemyIds = new List<string>();

            // 2. Достаем ID (проход по списку в памяти работает мгновенно)
            if (_cachedRegistry != null && _cachedRegistry.Enemies != null)
            {
                foreach (var enemy in _cachedRegistry.Enemies)
                {
                    if (enemy != null && !string.IsNullOrEmpty(enemy.EnemyId))
                    {
                        enemyIds.Add(enemy.EnemyId);
                    }
                }
            }

            if (enemyIds.Count == 0)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            int selectedIndex = Mathf.Max(0, enemyIds.IndexOf(property.stringValue));
            selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, enemyIds.ToArray());
            property.stringValue = enemyIds[selectedIndex];
        }
    }
}