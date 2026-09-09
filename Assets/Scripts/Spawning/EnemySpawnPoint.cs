using UnityEngine;
using Zenject;
using Gameplay.Spawning.Visuals;

namespace Gameplay.Spawning
{
    /// <summary>
    /// Humble Object. Физическая точка спавна на сцене.
    /// Отвечает только за саморегистрацию в реестре и проброс визуальных эффектов.
    /// </summary>
    public class EnemySpawnPoint : MonoBehaviour
    {
        // Избавляемся от магических чисел
        private static readonly Color GizmoColor = new Color(1f, 0f, 1f, 0.5f);
        private const float GizmoRadius = 0.5f;
        private const float GizmoHeightOffset = 0.5f;

        [Header("Настройки")]
        // Строгая инкапсуляция: поле видно в инспекторе, но изменить извне его нельзя (только чтение)
        [SerializeField] private string _pointId = "DefaultSpawn";
        
        private SpawnRegistry _registry;
        private ISpawnVisuals _visuals;

        public string PointId => _pointId;

        // Позволяем генератору уровня задать ID при спавне
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

// Оборачиваем Gizmos, чтобы они полностью вырезались при сборке релизного билда
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