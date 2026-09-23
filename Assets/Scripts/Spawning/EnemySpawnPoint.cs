using UnityEngine;
using Zenject;
using Gameplay.Spawning.Visuals;

namespace Gameplay.Spawning
{
    public class EnemySpawnPoint : MonoBehaviour
    {

        private static readonly Color GizmoColor = new Color(1f, 0f, 1f, 0.5f);
        private const float GizmoRadius = 0.5f;
        private const float GizmoHeightOffset = 0.5f;

        [Header("Настройки")]
        [SerializeField] private string _pointId = "DefaultSpawn";
        
        private SpawnRegistry _registry;
        private ISpawnVisuals _visuals;

        public string PointId => _pointId;

        public void SetId(string newId)
        {
            _pointId = newId;
        }

        [Inject]
        public void Construct(SpawnRegistry registry)
        {
            _registry = registry;
        }

        private void Awake()
        {
            _visuals = GetComponentInChildren<ISpawnVisuals>();
        }

        private void Start()
        {
            _registry?.Register(this);
        }

        private void OnDestroy()
        {
            _registry?.Unregister(this);
        }

        public void TriggerWarning(float duration)
        {
            _visuals?.PlayWarningEffect(duration);
        }


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = GizmoColor;
            Gizmos.DrawSphere(transform.position + Vector3.up * GizmoHeightOffset, GizmoRadius);
        }
#endif

        public class Factory : PlaceholderFactory<EnemySpawnPoint> { }
    }
}