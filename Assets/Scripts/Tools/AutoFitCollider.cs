using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(BoxCollider))]
public class AutoFitCollider : MonoBehaviour
{
    [ContextMenu("Подогнать BoxCollider под Визуал")]
    public void FitCollider()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        
        Renderer[] renderers = GetComponentsInChildren<Renderer>()
            .Where(r => !r.gameObject.name.Contains("RadiusIndicator"))
            .ToArray();
            
        if (renderers.Length == 0)
        {
            Gameplay.Tools.GameLogger.LogWarning("Дочерние меши не найдены!");
            return;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

#if UNITY_EDITOR
        Undo.RecordObject(boxCollider, "Fit BoxCollider");
#endif

        boxCollider.center = transform.InverseTransformPoint(bounds.center);

        boxCollider.size = new Vector3(
            bounds.size.x / transform.lossyScale.x,
            bounds.size.y / transform.lossyScale.y,
            bounds.size.z / transform.lossyScale.z
        );

#if UNITY_EDITOR
        EditorUtility.SetDirty(boxCollider);
        PrefabUtility.RecordPrefabInstancePropertyModifications(boxCollider);
#endif

        Gameplay.Tools.GameLogger.Log($"Коллайдер подогнан и СОХРАНЕН! Размер: {boxCollider.size}");
    }
}