using UnityEngine;
using Zenject;
using Gameplay.Enemies;
using System.Collections;
using UnityEngine.AI;

namespace Gameplay.Spawner
{
    public class WaveSpawner : MonoBehaviour
    {
        [Header("Настройки спавна")]
        [SerializeField] private Vector3 _spawnPosition = new Vector3(0, 1, 0); // Точка старта (например, край сетки)
        [SerializeField] private Vector3 _targetPosition = new Vector3(4, 1, 4); // Точка базы (противоположный край)
        [SerializeField] private float _spawnInterval = 2.0f;                  // Интервал между спавном сфер
        [SerializeField] private int _enemiesInWave = 5;                       // Сколько врагов выпустить

        private EnemyFacade.Pool _enemyPool;


        // Внедряем пул врагов через Zenject
        [Inject]
        public void Construct(EnemyFacade.Pool enemyPool)
        {
            _enemyPool = enemyPool;
        }

        private void Start()
        {
            // Запускаем корутину спавна волны
            StartCoroutine(SpawnWaveRoutine());

        }

        private IEnumerator SpawnWaveRoutine()
        {

            // КРИТИЧЕСКОЕ ИСПРАВЛЕНИЕ: Ждем до конца текущего кадра (или 0.1 секунды),
            // чтобы GridGenerator успел полностью запечь NavMesh и движок обновил навигацию.
            yield return new WaitForEndOfFrame(); 
            // Альтернатива, если кадр слишком короткий: yield return new WaitForSeconds(0.1f);

            Debug.Log("<color=yellow>[WaveSpawner] Волна начинается!</color>");

            for (int i = 0; i < _enemiesInWave; i++)
            {
                // 1. Достаем "голую" сущность из пула (передаем пока пустые параметры, если пул требует)
                EnemyFacade enemy = _enemyPool.Spawn(null, _enemyPool);

                // 2. СНАЧАЛА перемещаем физический объект в точку старта
                // В NavMeshAgent для телепортации лучше использовать встроенный метод Warp
                var agent = enemy.GetComponent<NavMeshAgent>();
                if (agent != null)
                {
                    agent.Warp(_spawnPosition); // Это корректно перенесет агента на сетку
                }
                else
                {
                    enemy.transform.position = _spawnPosition;
                }

                // 3. И ТОЛЬКО ТЕПЕРЬ создаем стратегию и запускаем ее!
                IMovementStrategy movement = new NavMeshMovement(_targetPosition);
                enemy.InitializeMovement(movement); // Этот метод мы сейчас добавим в Фасад

                yield return new WaitForSeconds(_spawnInterval);

            }
            Debug.Log("<color=yellow>[WaveSpawner] Все враги волны выпущены.</color>");
        }


    }
}

