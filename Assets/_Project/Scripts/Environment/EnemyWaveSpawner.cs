using System;
using System.Collections;
using UnityEngine;
using Zenject;
using TpsShooter.Enemies.Configs;
using TpsShooter.Enemies.Core;
using TpsShooter.Services.Progress; // Для связи с глобальным прогрессом

namespace TpsShooter.Environment
{
    [Serializable]
    public struct PatrolRoute
    {
        public string RouteName; 
        public Transform[] Waypoints;
    }

    public class EnemyWaveSpawner : MonoBehaviour
    {
        [Header("Wave Settings")]
        [SerializeField] private LevelWavesConfig _wavesConfig;
        
        [Tooltip("Точки на уровне, где будут появляться враги")]
        [SerializeField] private Transform[] _spawnPoints;

        [Header("Routes Library")]
        [SerializeField] private PatrolRoute[] _patrolRoutes;

        [Header("Difficulty Scaling")]
        [Tooltip("На сколько дополнительных врагов увеличивается спавн за каждый пройденный уровень")]
        [SerializeField] private float _extraEnemiesPerLevel = 1.5f;

        private IEnemyFactory _enemyFactory;
        private GameProgressService _progressService; 

        // Отслеживание живых врагов для победы
        private int _totalEnemiesToSpawn = 0;
        private int _enemiesSpawned = 0;
        private int _enemiesDead = 0;
        
        // ТО САМОЕ СОБЫТИЕ, КОТОРОЕ ИЩЕТ LevelFlowManager
        public event Action OnAllEnemiesDefeated; 

        [Inject]
        public void Construct(IEnemyFactory enemyFactory, GameProgressService progressService)
        {
            _enemyFactory = enemyFactory;
            _progressService = progressService;
        }

        private void Start()
        {
            if (_wavesConfig == null || _spawnPoints == null || _spawnPoints.Length == 0)
            {
                Debug.LogError("[EnemyWaveSpawner] Не назначен конфиг волн или точки спавна!");
                return;
            }

            CalculateTotalEnemies();
            StartCoroutine(SpawnWavesRoutine());
        }

        private void CalculateTotalEnemies()
        {
            _totalEnemiesToSpawn = 0;
            int levelScale = _progressService.CurrentLevel - 1;

            foreach (var wave in _wavesConfig.Waves)
            {
                foreach (var group in wave.Enemies)
                {
                    int scaledCount = group.Count + Mathf.FloorToInt(levelScale * _extraEnemiesPerLevel);
                    _totalEnemiesToSpawn += scaledCount;
                }
            }
            Debug.Log($"<color=green>[Spawner]</color> Уровень {_progressService.CurrentLevel}. Всего врагов к спавну: {_totalEnemiesToSpawn}");
        }

        private IEnumerator SpawnWavesRoutine()
        {
            int levelScale = _progressService.CurrentLevel - 1;

            for (int waveIndex = 0; waveIndex < _wavesConfig.Waves.Count; waveIndex++)
            {
                WaveData currentWave = _wavesConfig.Waves[waveIndex];
                yield return new WaitForSeconds(currentWave.StartDelay);

                foreach (EnemySpawnData enemyGroup in currentWave.Enemies)
                {
                    int scaledCount = enemyGroup.Count + Mathf.FloorToInt(levelScale * _extraEnemiesPerLevel);
                    int bonusEnemies = scaledCount - enemyGroup.Count;

                    // <--- ДОБАВЛЕН ПОДРОБНЫЙ ЛОГ --->
                    Debug.Log($"<color=cyan>[Spawner]</color> Спавн волны {waveIndex + 1}. Врагов: {scaledCount} (База: {enemyGroup.Count} | Бонус за уровень: +{bonusEnemies})");

                    for (int i = 0; i < scaledCount; i++)
                    {
                        SpawnEnemy(enemyGroup);
                        yield return new WaitForSeconds(currentWave.SpawnInterval);
                    }
                }
            }
        }

        private void SpawnEnemy(EnemySpawnData spawnData)
        {
            if (spawnData.BasePrefab == null || spawnData.Config == null) return;

            Transform randomPoint = _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Length)];
            Transform[] selectedRoute = null;
            
            if (_patrolRoutes != null && spawnData.RouteIndex >= 0 && spawnData.RouteIndex < _patrolRoutes.Length)
            {
                selectedRoute = _patrolRoutes[spawnData.RouteIndex].Waypoints;
            }

            EnemyBrain spawnedEnemy = _enemyFactory.Create(spawnData.BasePrefab, spawnData.Config, randomPoint.position, randomPoint.rotation, selectedRoute);
            _enemiesSpawned++;

            // Подписываемся на смерть каждого заспавненного врага
            if (spawnedEnemy.Health != null)
            {
                spawnedEnemy.Health.OnDeath += HandleEnemyDeath;
            }
        }

        private void HandleEnemyDeath()
        {
            _enemiesDead++;
            Debug.Log($"<color=yellow>[Spawner]</color> Враг убит. {_enemiesDead} / {_totalEnemiesToSpawn}");

            if (_enemiesDead >= _totalEnemiesToSpawn && _enemiesSpawned == _totalEnemiesToSpawn)
            {
                Debug.Log($"<color=yellow>[Spawner]</color> ВСЕ ВРАГИ УНИЧТОЖЕНЫ!");
                OnAllEnemiesDefeated?.Invoke();
            }
        }
    }
}