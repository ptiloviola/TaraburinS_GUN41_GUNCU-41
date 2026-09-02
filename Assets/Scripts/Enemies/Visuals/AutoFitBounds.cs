using UnityEngine;
using UnityEngine.AI;

namespace Gameplay.Enemies.Visuals
{
    [RequireComponent(typeof(BoxCollider), typeof(NavMeshAgent))]
    public class AutoFitBounds : MonoBehaviour
    {
        [Tooltip("Корень, внутри которого лежат графические модели (MeshRenderers)")]
        [SerializeField] private Transform _visualRoot;

        [ContextMenu("Auto-Fit Physics")] // Позволяет нажать ПКМ по скрипту в редакторе и подогнать размеры
        public void FitBounds()
        {
            if (_visualRoot == null)
            {
                Debug.LogWarning("[AutoFit] Не назначен Visual Root!");
                return;
            }

            // 1. Собираем все рендереры (кубики, цилиндры, сложные меши из Blender)
            Renderer[] renderers = _visualRoot.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;

            // 2. Создаем стартовую коробку по первой детали
            Bounds combinedBounds = renderers[0].bounds;

            // 3. Расширяем коробку, чтобы она поглотила все остальные детали
            for (int i = 1; i < renderers.Length; i++)
            {
                combinedBounds.Encapsulate(renderers[i].bounds);
            }

            // 4. Подгоняем BoxCollider
            BoxCollider box = GetComponent<BoxCollider>();
            // Переводим мировой центр коробки в локальные координаты корня
            box.center = transform.InverseTransformPoint(combinedBounds.center);
            box.size = combinedBounds.size;

            // 5. Подгоняем NavMeshAgent
            NavMeshAgent agent = GetComponent<NavMeshAgent>();
            // Радиус агента берем по самой широкой части модели (X или Z)
            agent.radius = Mathf.Max(combinedBounds.extents.x, combinedBounds.extents.z);
            agent.height = combinedBounds.size.y;
            
            Debug.Log($"<color=green>[AutoFit] Габариты успешно подогнаны под меш!</color>");
        }
    }
}