using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Text;
using TMPro;

namespace Gameplay.Tools
{
    public class CanvasDumper : UnityEditor.Editor
    {
        [MenuItem("Tools/TD/Скопировать структуру Canvas для ИИ")]
        public static void CopyCanvasStructureToClipboard()
        {
            GameObject selected = Selection.activeGameObject;
            
            if (selected == null)
            {
                Debug.LogWarning("[TD Tools] Сначала выдели объект UI в иерархии!");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"--- UI Структура: {selected.name} ---");
            
            DumpNode(selected.transform, sb, 0);

            GUIUtility.systemCopyBuffer = sb.ToString();
            Debug.Log($"<color=cyan>[TD Tools] UI структура '{selected.name}' скопирована! Можно вставлять в чат (Ctrl+V).</color>");
        }

        private static void DumpNode(Transform node, StringBuilder sb, int depth)
        {
            string indent = new string(' ', depth * 4) + (depth > 0 ? "┗ " : "");
            

            string pos = $"Pos({node.localPosition.x:F1}, {node.localPosition.y:F1}, {node.localPosition.z:F1})";
            string scale = $"Scale({node.localScale.x:F2}, {node.localScale.y:F2}, {node.localScale.z:F2})";
            
            Component[] components = node.GetComponents<Component>();
            StringBuilder compBuilder = new StringBuilder();
            
            foreach (var c in components)
            {

                if (c == null || c.GetType() == typeof(Transform)) continue;
                
                string compName = c.GetType().Name;
                string details = GetComponentDetails(c);
                
                compBuilder.Append($"[{compName}{details}] ");
            }

            string compString = compBuilder.Length > 0 ? compBuilder.ToString() : "-";

            sb.AppendLine($"{indent}📦 {node.name} | {pos} | {scale} | Компоненты: {compString}");

            foreach (Transform child in node)
            {
                DumpNode(child, sb, depth + 1);
            }
        }

        private static string GetComponentDetails(Component c)
        {
            switch (c)
            {
                case RectTransform rt:
                    return $" (Anchors: Min({rt.anchorMin.x:F2}, {rt.anchorMin.y:F2}) Max({rt.anchorMax.x:F2}, {rt.anchorMax.y:F2}), Pivot: {rt.pivot}, SizeDelta: {rt.sizeDelta})";

                case Canvas canvas:
                    return $" (RenderMode: {canvas.renderMode}, SortOrder: {canvas.sortingOrder})";

                case CanvasScaler scaler:
                    string resStr = scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize ? $", RefRes: {scaler.referenceResolution}" : "";
                    return $" (Mode: {scaler.uiScaleMode}{resStr})";

                case CanvasGroup cg:
                    return $" (Alpha: {cg.alpha:F2}, BlocksRaycasts: {cg.blocksRaycasts}, Interactable: {cg.interactable})";

                case Image img:
                    return $" (RaycastTarget: {img.raycastTarget}, Color.a: {img.color.a:F2})";

                case TMP_Text tmp:
                    return $" (RaycastTarget: {tmp.raycastTarget}, FontSize: {tmp.fontSize}, Align: {tmp.alignment})";

                case Button btn:
                    return $" (Interactable: {btn.interactable})";

                case HorizontalOrVerticalLayoutGroup layout:
                    return $" (Spacing: {layout.spacing}, Align: {layout.childAlignment}, ControlSize: {layout.childControlWidth}/{layout.childControlHeight})";

                case GridLayoutGroup grid:
                    return $" (CellSize: {grid.cellSize}, Spacing: {grid.spacing}, Align: {grid.childAlignment})";

                case ContentSizeFitter csf:
                    return $" (H: {csf.horizontalFit}, V: {csf.verticalFit})";

                default:
                    return ""; 
            }
        }
    }
}