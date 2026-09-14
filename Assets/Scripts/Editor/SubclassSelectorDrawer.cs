using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Gameplay.Core.Attributes;

[CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
public class SubclassSelectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Рисуем имя поля (например, "Element 0")
        Rect popupPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        
        // Получаем базовый тип из списка (например, IStatusConfig)
        Type fieldType = GetFieldType(property);
        if (fieldType == null) return;

        // Ищем все классы, которые реализуют этот интерфейс
        var inheritedTypes = TypeCache.GetTypesDerivedFrom(fieldType)
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .ToArray();

        // Формируем список имен для выпадающего меню
        string[] typeNames = new string[inheritedTypes.Length + 1];
        typeNames[0] = "<Null>";
        for (int i = 0; i < inheritedTypes.Length; i++)
        {
            typeNames[i + 1] = inheritedTypes[i].Name;
        }

        // Определяем текущий выбранный тип
        int currentIndex = 0;
        string currentTypeName = property.managedReferenceFullTypename.Split(' ').Last();
        for (int i = 0; i < inheritedTypes.Length; i++)
        {
            if (inheritedTypes[i].FullName == currentTypeName)
            {
                currentIndex = i + 1;
                break;
            }
        }

        // Рисуем выпадающий список
        int newIndex = EditorGUI.Popup(popupPosition, label.text, currentIndex, typeNames);
        if (newIndex != currentIndex)
        {
            if (newIndex == 0)
            {
                property.managedReferenceValue = null;
            }
            else
            {
                // Создаем инстанс выбранного класса
                property.managedReferenceValue = Activator.CreateInstance(inheritedTypes[newIndex - 1]);
            }
        }

        // Отрисовываем поля самого выбранного класса
        if (property.managedReferenceValue != null)
        {
            Rect propertyRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, position.height - EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(propertyRect, property, GUIContent.none, true);
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return property.isExpanded || property.managedReferenceValue != null 
            ? EditorGUI.GetPropertyHeight(property, true) + EditorGUIUtility.singleLineHeight 
            : EditorGUIUtility.singleLineHeight;
    }

    private Type GetFieldType(SerializedProperty property)
    {
        string[] typeNames = property.managedReferenceFieldTypename.Split(' ');
        if (typeNames.Length < 2) return null;
        return Type.GetType($"{typeNames[1]}, {typeNames[0]}");
    }
}