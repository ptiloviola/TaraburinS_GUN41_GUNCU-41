using UnityEngine;
using Zenject;
using Gameplay.Enemies;
using System.Collections;
using Gameplay.Base;
using Infrastructure.Signals; // Подключаем сигналы

namespace Gameplay.Spawner
{
    public class WaveSpawner : MonoBehaviour
    {
        [Header("Настройки спавна")]
        [SerializeField] private Vector3 _spawnPosition = new Vector3(0, 1, 0); // Точка старта (например, край сетки)
        [SerializeField] private WaveConfig _waveConfig; // Ссылка на наш новый конфиг




        private EnemyFacade.Pool _enemyPool;
        private BaseCore _baseCore; // Ссылка на компонент базы
        private SignalBus _signalBus;


        // Внедряем пул врагов через Zenject
        [Inject]
        public void Construct(EnemyFacade.Pool enemyPool, BaseCore baseCore, SignalBus signalBus)
        {
            _enemyPool = enemyPool;
            _baseCore = baseCore;
            _signalBus = signalBus;
        }

        private void Start()
        {
            if (_waveConfig == null || _waveConfig.Waves.Count == 0)
            {
                Debug.LogError("[WaveSpawner] Конфиг волн не назначен или пуст!");
                return;
            }
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
            // Главный цикл: идем по списку волн из конфига
            for (int i = 0; i < _waveConfig.Waves.Count; i++)
            {
                WaveData currentWave = _waveConfig.Waves[i];
                Debug.Log($"<color=cyan>[WaveSpawner] Ожидание {currentWave.DelayBeforeWave} сек. до волны {i + 1}.</color>");

                // Передышка для игрока (время на постройку башен)
                yield return new WaitForSeconds(currentWave.DelayBeforeWave);

                _signalBus.Fire(new SignalWaveStarted
                {
                    CurrentWaveIndex = i + 1,
                    TotalWaves = _waveConfig.Waves.Count
                });

                Debug.Log($"<color=yellow>[WaveSpawner] Волна {i + 1} началась! Врагов: {currentWave.EnemyCount}</color>");
                
                // Внутренний цикл: спавним врагов текущей волны
                for (int j = 0; j < currentWave.EnemyCount; j++)
                {
                    SpawnSingleEnemy();
                    yield return new WaitForSeconds(currentWave.SpawnInterval);
                }
            }
            Debug.Log("<color=yellow>[WaveSpawner] Все враги волны выпущены.</color>");
        }

        private void SpawnSingleEnemy()
        {
            EnemyFacade enemy = _enemyPool.Spawn();

            var agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.Warp(_spawnPosition);
            }

            IMovementStrategy movement = new NavMeshMovement(_baseCore.transform.position);
            enemy.InitializeMovement(movement);
        }


    }
}

