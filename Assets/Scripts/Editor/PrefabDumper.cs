using UnityEngine;
using UnityEngine.AI;
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
            
            string pos = $"Pos({node.localPosition.x:F2}, {node.localPosition.y:F2}, {node.localPosition.z:F2})";
            string rot = $"Rot({node.localEulerAngles.x:F1}, {node.localEulerAngles.y:F1}, {node.localEulerAngles.z:F1})";
            string scale = $"Scale({node.localScale.x:F2}, {node.localScale.y:F2}, {node.localScale.z:F2})";
            
            Component[] components = node.GetComponents<Component>();
            StringBuilder compBuilder = new StringBuilder();
            
            foreach (var c in components)
            {
                if (c == null || c is Transform) continue;
                
                string compName = c.GetType().Name;
                string details = GetComponentDetails(c); // Достаем важные параметры
                
                compBuilder.Append($"[{compName}{details}] ");
            }

            string compString = compBuilder.Length > 0 ? compBuilder.ToString() : "-";

            sb.AppendLine($"{indent}📦 {node.name} | {pos} | {rot} | {scale} | Компоненты: {compString}");

            foreach (Transform child in node)
            {
                DumpNode(child, sb, depth + 1);
            }
        }

        // --- НОВЫЙ МЕТОД: Извлекаем суть из компонентов ---
        private static string GetComponentDetails(Component c)
        {
            switch (c)
            {
                // 1. Физика: Коллайдеры
                case Collider col:
                    string triggerStr = col.isTrigger ? "(TRIGGER)" : "";
                    string sizeStr = "";
                    
                    if (col is SphereCollider sc) sizeStr = $"r:{sc.radius:F2}";
                    else if (col is BoxCollider bc) sizeStr = $"size:{bc.size.x:F1}x{bc.size.y:F1}x{bc.size.z:F1}";
                    else if (col is CapsuleCollider cc) sizeStr = $"r:{cc.radius:F2},h:{cc.height:F2}";

                    // Если параметров нет, скобки не выводим
                    if (string.IsNullOrEmpty(triggerStr) && string.IsNullOrEmpty(sizeStr)) return "";
                    return $" {triggerStr} {sizeStr}".TrimEnd();

                // 2. Физика: Тела
                case Rigidbody rb:
                    string kinStr = rb.isKinematic ? "Kinematic" : "Dynamic";
                    string gravStr = rb.useGravity ? "+Grav" : "-Grav";
                    return $" ({kinStr}, {gravStr})";

                // 3. Навигация
                case NavMeshAgent agent:
                    return $" (Offset:{agent.baseOffset:F2}, Speed:{agent.speed:F1})";

                // 4. Интерфейс (чтобы сразу видеть, если полоска ХП отвалилась в Screen Space)
                case Canvas canvas:
                    return $" ({canvas.renderMode})";

                // Сюда в будущем можно легко дописывать новые типы компонентов!
                default:
                    return "";
            }
        }
    }
}