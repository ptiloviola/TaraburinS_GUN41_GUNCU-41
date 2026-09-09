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
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            List<string> enemyIds = new List<string>();

            // 1. Ищем НАШ РЕЕСТР (EnemyRegistry), а не отдельные конфиги!
            string[] guids = AssetDatabase.FindAssets("t:EnemyRegistry");
            
            if (guids.Length > 0)
            {
                // Берем первый найденный реестр (обычно он один на проект)
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                EnemyRegistry registry = AssetDatabase.LoadAssetAtPath<EnemyRegistry>(path);

                // 2. Достаем ID только из официально зарегистрированных врагов
                if (registry != null && registry.Enemies != null)
                {
                    foreach (var enemy in registry.Enemies)
                    {
                        if (enemy != null && !string.IsNullOrEmpty(enemy.EnemyId))
                        {
                            enemyIds.Add(enemy.EnemyId);
                        }
                    }
                }
            }

            // Если реестр пуст или не найден — рисуем обычное текстовое поле
            if (enemyIds.Count == 0)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            // Находим индекс текущего сохраненного значения (если его нет в списке, ставим 0)
            int selectedIndex = Mathf.Max(0, enemyIds.IndexOf(property.stringValue));

            // Отрисовываем выпадающий список
            selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, enemyIds.ToArray());

            // Сохраняем выбранное значение обратно в property
            property.stringValue = enemyIds[selectedIndex];
        }
    }
}