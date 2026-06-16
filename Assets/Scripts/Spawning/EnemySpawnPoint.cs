using UnityEngine;
using Zenject;
using Gameplay.Spawning.Visuals;

namespace Gameplay.Spawning
{
    public class EnemySpawnPoint : MonoBehaviour
    {
        public string PointId = "DefaultSpawn";
        private SpawnRegistry _registry;

        // Ссылка на компонент визуала (через абстракцию!)
        private ISpawnVisuals _visuals;

        [Inject]
        public void Construct(SpawnRegistry registry)
        {
            _registry = registry;
        }

        private void Awake()
        {
            // Ищем любой скрипт, реализующий ISpawnVisuals на этом объекте или детях
            _visuals = GetComponentInChildren<ISpawnVisuals>();
        }

        // ИСПРАВЛЕНИЕ: Переносим регистрацию в Start()
        private void Start() => _registry?.Register(this);
        // ИСПРАВЛЕНИЕ: Раз регистрируемся в Start, выписываемся в OnDestroy
        private void OnDestroy() => _registry?.Unregister(this);

        // Публичный метод для Реестра
        public void TriggerWarning(float duration)
        {
            _visuals?.PlayWarningEffect(duration);
        }

        

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0f, 1f, 0.5f); // Пурпурный
            Gizmos.DrawSphere(transform.position + Vector3.up * 0.5f, 0.5f);
        }

        // Фабрика для генерации из GridGenerator
        public class Factory : PlaceholderFactory<EnemySpawnPoint> { }

    }
}