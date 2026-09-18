using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Gameplay.Spawning.Data;
using Gameplay.Spawning.Factories;

namespace Gameplay.Spawning.Services
{
    public class WaveSpawnerService
    {
        private readonly SpawnRegistry _spawnRegistry;
        private readonly EnemyFactory _enemyFactory;


        private const float WarningDurationSeconds = 2f;

        public WaveSpawnerService(SpawnRegistry spawnRegistry, EnemyFactory enemyFactory)
        {
            _spawnRegistry = spawnRegistry;
            _enemyFactory = enemyFactory;
        }

        public async UniTask SpawnWaveAsync(WaveData wave, CancellationToken ct)
        {
            foreach (SquadData squad in wave.Squads)
            {
                _spawnRegistry.TriggerWarning(squad.SpawnPointId, WarningDurationSeconds);
                
                await UniTask.Delay(TimeSpan.FromSeconds(WarningDurationSeconds), cancellationToken: ct);

#if UNITY_EDITOR
                Debug.Log($"<color=cyan>[SpawnerService] Выходит отряд: {squad.Count}x {squad.EnemyId} (Точка: {squad.SpawnPointId})</color>");
#endif

                for (int i = 0; i < squad.Count; i++)
                {
                    SpawnPhysicalEnemy(squad);
                    if (squad.SpawnInterval > 0)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(squad.SpawnInterval), cancellationToken: ct);
                    }
                }
            }
        }

        private void SpawnPhysicalEnemy(SquadData squad)
        {
            if (!_spawnRegistry.TryGetSpawnPosition(squad.SpawnPointId, out Vector3 spawnPos))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[SpawnerService] Спавн '{squad.SpawnPointId}' не найден! Спавним в (0,0,0).");
#endif
                spawnPos = Vector3.zero; 
            }
            
            _enemyFactory.SpawnEnemy(squad.EnemyId, spawnPos, squad.TargetBaseId);
        }
    }
}