using UnityEngine;
using UnityEditor;
using System.Text;

namespace Gameplay.Tools
{
    public class PrefabDumper : UnityEditor.Editor
    {
        [MenuItem("Tools/TD/Скопировать структуру префаба для ИИ")]
        public static void CopyStructureToClipboard()
        {
            GameObject selected = Selection.activeGameObject;
            
            if (selected == null)
            {
                Debug.LogWarning("[TD Tools] Сначала выдели объект в иерархии (Scene или Project)!");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"--- Структура: {selected.name} ---");
            
            DumpNode(selected.transform, sb, 0);

            GUIUtility.systemCopyBuffer = sb.ToString();
            Debug.Log($"<color=cyan>[TD Tools] Структура '{selected.name}' скопирована! Можно вставлять в чат (Ctrl+V).</color>");
        }

        private static void DumpNode(Transform node, StringBuilder sb, int depth)
        {
            string indent = new string(' ', depth * 4) + (depth > 0 ? "┗ " : "");
            
            // Собираем координаты, поворот и масштаб
            string pos = $"Pos({node.localPosition.x:F2}, {node.localPosition.y:F2}, {node.localPosition.z:F2})";
            string rot = $"Rot({node.localEulerAngles.x:F1}, {node.localEulerAngles.y:F1}, {node.localEulerAngles.z:F1})";
            string scale = $"Scale({node.localScale.x:F2}, {node.localScale.y:F2}, {node.localScale.z:F2})";
            
            Component[] components = node.GetComponents<Component>();
            string compString = "";
            
            foreach (var c in components)
            {
                if (c == null) continue;
                if (c is Transform) continue;
                
                compString += $"[{c.GetType().Name}] ";
            }

            if (string.IsNullOrEmpty(compString)) compString = "-";

            // Добавляем Scale в финальный вывод
            sb.AppendLine($"{indent}📦 {node.name} | {pos} | {rot} | {scale} | Компоненты: {compString}");

            foreach (Transform child in node)
            {
                DumpNode(child, sb, depth + 1);
            }
        }
    }
}