using UnityEngine;
using Zenject;
using Gameplay.Enemies;
using System.Collections;
using UnityEngine.AI;
using Gameplay.Base;

namespace Gameplay.Spawner
{
    public class WaveSpawner : MonoBehaviour
    {
        [Header("Настройки спавна")]
        [SerializeField] private Vector3 _spawnPosition = new Vector3(0, 1, 0); // Точка старта (например, край сетки)
        
        [SerializeField] private float _spawnInterval = 2.0f;                  // Интервал между спавном сфер
        [SerializeField] private int _enemiesInWave = 5;                       // Сколько врагов выпустить

        private EnemyFacade.Pool _enemyPool;
        private BaseCore _baseCore; // Ссылка на компонент базы


        // Внедряем пул врагов через Zenject
        [Inject]
        public void Construct(EnemyFacade.Pool enemyPool, BaseCore baseCore)
        {
            _enemyPool = enemyPool;
            _baseCore = baseCore;
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

            // Защита: если базы почему-то нет на сцене, останавливаем спавн
            if (_baseCore == null)
            {
                Debug.LogError("[WaveSpawner] Невозможно начать волну: База не найдена в контейнере Zenject!");
                yield break;
            }

            for (int i = 0; i < _enemiesInWave; i++)
            {
                // Было: EnemyFacade enemy = _enemyPool.Spawn(null, _enemyPool);
                // СТАЛО: Просто берем врага из пула!
                EnemyFacade enemy = _enemyPool.Spawn();

                var agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null)
                {
                    agent.Warp(_spawnPosition);
                }

                IMovementStrategy movement = new NavMeshMovement(_baseCore.transform.position);
                enemy.InitializeMovement(movement);

                yield return new WaitForSeconds(_spawnInterval);

            }
            Debug.Log("<color=yellow>[WaveSpawner] Все враги волны выпущены.</color>");
        }


    }
}

