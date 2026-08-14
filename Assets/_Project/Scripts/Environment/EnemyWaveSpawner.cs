using System.Collections;
using UnityEngine;
using Zenject;
using TpsShooter.Enemies.Configs;
using TpsShooter.Enemies.Core;

namespace TpsShooter.Environment
{
    public class EnemyWaveSpawner : MonoBehaviour
    {
        [Header("Wave Settings")]
        [SerializeField] private LevelWavesConfig _wavesConfig;
        
        [Tooltip("Точки на уровне, где будут появляться враги")]
        [SerializeField] private Transform[] _spawnPoints;

        // <--- ДОБАВЛЕНО: Точки патрулирования комнаты --->
        [Tooltip("Точки патрулирования комнаты. Будут переданы всем заспавненным врагам")]
        [SerializeField] private Transform[] _roomPatrolPoints;

        private IEnemyFactory _enemyFactory;

        [Inject]
        public void Construct(IEnemyFactory enemyFactory)
        {
            _enemyFactory = enemyFactory;
        }

        private void Start()
        {
            if (_wavesConfig == null || _spawnPoints == null || _spawnPoints.Length == 0)
            {
                Debug.LogError("[EnemyWaveSpawner] Не назначен конфиг волн или точки спавна!");
                return;
            }

            StartCoroutine(SpawnWavesRoutine());
        }

        private IEnumerator SpawnWavesRoutine()
        {
            for (int waveIndex = 0; waveIndex < _wavesConfig.Waves.Count; waveIndex++)
            {
                WaveData currentWave = _wavesConfig.Waves[waveIndex];
                
                Debug.Log($"<color=green>[Spawner]</color> Подготовка к волне {waveIndex + 1}. Ожидание {currentWave.StartDelay} сек.");
                yield return new WaitForSeconds(currentWave.StartDelay);

                Debug.Log($"<color=green>[Spawner]</color> Волна {waveIndex + 1} началась!");

                foreach (EnemySpawnData enemyGroup in currentWave.Enemies)
                {
                    for (int i = 0; i < enemyGroup.Count; i++)
                    {
                        SpawnEnemy(enemyGroup);
                        yield return new WaitForSeconds(currentWave.SpawnInterval);
                    }
                }
            }
            Debug.Log("<color=green>[Spawner]</color> Все волны успешно завершены!");
        }

        private void SpawnEnemy(EnemySpawnData spawnData)
        {
            if (spawnData.BasePrefab == null || spawnData.Config == null)
            {
                Debug.LogWarning("[EnemyWaveSpawner] Пустой префаб или конфиг в настройках волны!");
                return;
            }

            Transform randomPoint = _spawnPoints[Random.Range(0, _spawnPoints.Length)];

            // <--- ИЗМЕНЕНО: Передаем _roomPatrolPoints в фабрику --->
            _enemyFactory.Create(spawnData.BasePrefab, spawnData.Config, randomPoint.position, randomPoint.rotation, _roomPatrolPoints);
        }
    }
}