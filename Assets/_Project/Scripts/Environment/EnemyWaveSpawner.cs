using System;
using UnityEngine;
using Zenject;
using TpsShooter.Enemies.Configs;
using TpsShooter.Enemies.Core;
using TpsShooter.Services.Progress; 
using Cysharp.Threading.Tasks;
using System.Threading;

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
        [SerializeField] private Transform[] _spawnPoints;

        [Header("Routes Library")]
        [SerializeField] private PatrolRoute[] _patrolRoutes;

        [Header("Difficulty Scaling")]
        [SerializeField] private float _extraEnemiesPerLevel = 1.5f;

        private IEnemyFactory _enemyFactory;
        private GameProgressService _progressService; 

        private int _totalEnemiesToSpawn = 0;
        private int _enemiesSpawned = 0;
        private int _enemiesDead = 0;
        
        public event Action OnAllEnemiesDefeated; 

        [Inject]
        public void Construct(IEnemyFactory enemyFactory, GameProgressService progressService)
        {
            _enemyFactory = enemyFactory;
            _progressService = progressService;
        }

        private void Start()
        {
            if (_wavesConfig == null || _spawnPoints == null || _spawnPoints.Length == 0) return;

            CalculateTotalEnemies();
            
            SpawnWavesAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        private void CalculateTotalEnemies()
        {
            _totalEnemiesToSpawn = 0;
            int levelScale = _progressService.CurrentLevel - 1;

            foreach (var wave in _wavesConfig.Waves)
            {
                foreach (var group in wave.Enemies)
                {
                    _totalEnemiesToSpawn += group.Count + Mathf.FloorToInt(levelScale * _extraEnemiesPerLevel);
                }
            }
        }

        private async UniTaskVoid SpawnWavesAsync(CancellationToken token)
        {
            int levelScale = _progressService.CurrentLevel - 1;

            for (int waveIndex = 0; waveIndex < _wavesConfig.Waves.Count; waveIndex++)
            {
                WaveData currentWave = _wavesConfig.Waves[waveIndex];
                
                await UniTask.Delay(TimeSpan.FromSeconds(currentWave.StartDelay), cancellationToken: token);

                foreach (EnemySpawnData enemyGroup in currentWave.Enemies)
                {
                    int scaledCount = enemyGroup.Count + Mathf.FloorToInt(levelScale * _extraEnemiesPerLevel);

                    for (int i = 0; i < scaledCount; i++)
                    {
                        SpawnEnemy(enemyGroup);
                        await UniTask.Delay(TimeSpan.FromSeconds(currentWave.SpawnInterval), cancellationToken: token);
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

            if (spawnedEnemy.Health != null)
            {
                Action deathHandler = null;
                deathHandler = () => 
                {
                    spawnedEnemy.Health.OnDeath -= deathHandler;
                    HandleEnemyDeath();
                };
                
                spawnedEnemy.Health.OnDeath += deathHandler;
            }
        }

        private void HandleEnemyDeath()
        {
            _enemiesDead++;
            if (_enemiesDead >= _totalEnemiesToSpawn && _enemiesSpawned == _totalEnemiesToSpawn)
            {
                OnAllEnemiesDefeated?.Invoke();
            }
        }
    }
}