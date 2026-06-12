using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Gameplay.Spawning.Data;
using Gameplay.Grid;

namespace Gameplay.Editor
{
    // Этот скрипт говорит Unity: "Когда увидишь [GridPointId], рисуй его по-моему!"
    [CustomPropertyDrawer(typeof(GridPointIdAttribute))]
    public class GridPointIdDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Получаем наш атрибут
            GridPointIdAttribute pointAttr = (GridPointIdAttribute)attribute;

            // Ищем GridGenerator прямо на открытой сцене!
            GridGenerator generator = Object.FindObjectOfType<GridGenerator>();

            if (generator != null && generator.EditorConfig != null)
            {
                // Собираем список доступных ID
                List<string> availableIds = new List<string>();
                
                // Для базы логично иметь вариант Найти ближайшую
                if (pointAttr.FilterType == NodeType.Base)
                {
                    // Вставляем красивый тег нулевым элементом
                    availableIds.Add("[Ближайшая по пути]"); 
                    
                    // Если строка только что создана и пуста, автоматически присваиваем ей этот тег
                    if (string.IsNullOrEmpty(property.stringValue))
                        property.stringValue = "[Ближайшая по пути]";
                }

                // Прочесываем флешку сетки
                GridConfig config = generator.EditorConfig;
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
                    // Ищем индекс текущего выбранного значения
                    int currentIndex = Mathf.Max(0, availableIds.IndexOf(property.stringValue));
                    
                    // Рисуем красивый Dropdown!
                    currentIndex = EditorGUI.Popup(position, label.text, currentIndex, availableIds.ToArray());
                    
                    // Сохраняем выбранное значение
                    property.stringValue = availableIds[currentIndex];
                    return;
                }
            }

            // Фолбэк: Если генератор не найден, рисуем обычное текстовое поле
            EditorGUI.PropertyField(position, property, label);
        }
    }
}
