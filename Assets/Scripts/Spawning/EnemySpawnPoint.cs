using UnityEngine;
using Zenject;

namespace Gameplay.Spawning
{
    public class EnemySpawnPoint : MonoBehaviour
    {
        public string PointId = "DefaultSpawn";
        private SpawnRegistry _registry;

        [Inject]
        public void Construct(SpawnRegistry registry)
        {
            _registry = registry;
        }

        // ИСПРАВЛЕНИЕ: Переносим регистрацию в Start()
        private void Start() => _registry?.Register(this);
        // ИСПРАВЛЕНИЕ: Раз регистрируемся в Start, выписываемся в OnDestroy
        private void OnDestroy() => _registry?.Unregister(this);

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0f, 1f, 0.5f); // Пурпурный
            Gizmos.DrawSphere(transform.position + Vector3.up * 0.5f, 0.5f);
        }

        // Фабрика для генерации из GridGenerator
        public class Factory : PlaceholderFactory<EnemySpawnPoint> { }

    }
}