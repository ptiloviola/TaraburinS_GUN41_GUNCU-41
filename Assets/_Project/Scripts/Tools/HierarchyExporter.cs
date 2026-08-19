using UnityEditor;
using UnityEngine;
using System.Text;
using UnityEngine.Animations.Rigging;

public class HierarchyExporter : Editor
{
    // Добавляем пункт в верхнее меню Unity
    [MenuItem("Tools/Export Hierarchy to Clipboard")]
    public static void ExportHierarchy()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogWarning("Сначала выдели GameObject в иерархии!");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"=== HIERARCHY DUMP FOR: {selected.name} ===");
        
        DumpObject(selected, sb, "");

        // Копируем результат в буфер обмена
        GUIUtility.systemCopyBuffer = sb.ToString();
        DevLogger.Log("Иерархия скопирована в буфер обмена! Можешь вставлять текст (Ctrl+V).");
    }

    private static void DumpObject(GameObject obj, StringBuilder sb, string indent)
    {
        sb.AppendLine($"{indent}■ {obj.name}");
        
        // Данные Transform
        Transform t = obj.transform;
        sb.AppendLine($"{indent}  └ Transform: Pos({t.localPosition.x:F3}, {t.localPosition.y:F3}, {t.localPosition.z:F3}) | " +
                      $"Rot({t.localEulerAngles.x:F2}, {t.localEulerAngles.y:F2}, {t.localEulerAngles.z:F2}) | " +
                      $"Scale({t.localScale.x:F2}, {t.localScale.y:F2}, {t.localScale.z:F2})");

        // Анализ компонентов
        Component[] components = obj.GetComponents<Component>();
        foreach (var comp in components)
        {
            if (comp == null || comp is Transform) continue;

            string compDetails = GetComponentDetails(comp);
            sb.AppendLine($"{indent}  ├ [Comp] {comp.GetType().Name} {compDetails}");
        }

        // Рекурсивный проход по детям
        for (int i = 0; i < t.childCount; i++)
        {
            DumpObject(t.GetChild(i).gameObject, sb, indent + "    ");
        }
    }

    private static string GetComponentDetails(Component comp)
    {
        // Парсим специфичные компоненты для вывода их важных настроек
        if (comp is TwoBoneIKConstraint ik)
        {
            return $"(Target: {GetName(ik.data.target)}, Hint: {GetName(ik.data.hint)}, Weight: {ik.weight})";
        }
        if (comp is Rig rig)
        {
            return $"(Weight: {rig.weight})";
        }
        if (comp is Animator anim)
        {
            return $"(Avatar: {GetName(anim.avatar)}, Controller: {GetName(anim.runtimeAnimatorController)})";
        }
        if (comp is CharacterController cc)
        {
            return $"(Center: {cc.center}, Radius: {cc.radius}, Height: {cc.height})";
        }
        
        // Для всех остальных кастомных скриптов просто выводим их наличие
        return "";
    }

    private static string GetName(Object obj) => obj != null ? obj.name : "NULL";
}