using UnityEngine;

// Защита: скрипт автоматически добавит BoxCollider, если его нет
[RequireComponent(typeof(BoxCollider))] 
public class AutoFitCollider : MonoBehaviour
{
    // Эта магия добавляет кнопку в меню скрипта!
    [ContextMenu("Подогнать BoxCollider под Визуал")]
    public void FitCollider()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        
        // Ищем все MeshRenderer в дочерних объектах (в твоем Visual)
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Debug.LogWarning("Дочерние меши не найдены!");
            return;
        }

        // Создаем "коробку" на основе первой детали
        Bounds bounds = renderers[0].bounds;

        // Если деталей несколько, расширяем коробку, чтобы она вместила их все
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        // Границы (bounds) рассчитываются в мировых координатах.
        // Нам нужно перевести центр в локальные координаты корня.
        boxCollider.center = transform.InverseTransformPoint(bounds.center);

        // Переводим мировой размер в локальный (с учетом твоего масштаба 1.0 на корне)
        boxCollider.size = new Vector3(
            bounds.size.x / transform.lossyScale.x,
            bounds.size.y / transform.lossyScale.y,
            bounds.size.z / transform.lossyScale.z
        );

        Debug.Log("Коллайдер успешно подогнан под модель!");
    }
}