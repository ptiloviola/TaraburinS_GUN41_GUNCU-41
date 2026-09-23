using UnityEngine;
using UnityEngine.AI;

namespace Gameplay.Enemies.Visuals
{
    [RequireComponent(typeof(BoxCollider), typeof(NavMeshAgent))]
    public class AutoFitBounds : MonoBehaviour
    {
        [Tooltip("Корень, внутри которого лежат графические модели (MeshRenderers)")]
        [SerializeField] private Transform _visualRoot;

        [ContextMenu("Auto-Fit Physics")]
        public void FitBounds()
        {
            if (_visualRoot == null)
            {
                Debug.LogWarning("[AutoFit] Не назначен Visual Root!");
                return;
            }


            Renderer[] renderers = _visualRoot.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;


            Bounds combinedBounds = renderers[0].bounds;


            for (int i = 1; i < renderers.Length; i++)
            {
                combinedBounds.Encapsulate(renderers[i].bounds);
            }


            BoxCollider box = GetComponent<BoxCollider>();

            box.center = transform.InverseTransformPoint(combinedBounds.center);
            box.size = combinedBounds.size;


            NavMeshAgent agent = GetComponent<NavMeshAgent>();

            agent.radius = Mathf.Max(combinedBounds.extents.x, combinedBounds.extents.z);
            agent.height = combinedBounds.size.y;
            
            Debug.Log($"<color=green>[AutoFit] Габариты успешно подогнаны под меш!</color>");
        }
    }
}