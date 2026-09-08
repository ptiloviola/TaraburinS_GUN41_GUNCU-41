using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Gameplay.Spawning.Data;
using Gameplay.Spawning.Factories;

namespace Gameplay.Spawning.Services
{
    /// <summary>
    /// Чистый класс, отвечающий ТОЛЬКО за физическое создание врагов с нужными интервалами.
    /// </summary>
    public class WaveSpawnerService
    {
        private readonly SpawnRegistry _spawnRegistry;
        private readonly EnemyFactory _enemyFactory;

        // Избавляемся от магических чисел: выносим время предупреждения в константу
        private const float WarningDurationSeconds = 2f;

        public WaveSpawnerService(SpawnRegistry spawnRegistry, EnemyFactory enemyFactory)
        {
            _spawnRegistry = spawnRegistry;
            _enemyFactory = enemyFactory;
        }

        /// <summary>
        /// Асинхронно спавнит всю волну. Прерывается мгновенно, если токен (ct) отменен.
        /// </summary>
        public async UniTask SpawnWaveAsync(WaveData wave, CancellationToken ct)
        {
            foreach (SquadData squad in wave.Squads)
            {
                // 1. Показываем визуальное предупреждение на точке спавна
                _spawnRegistry.TriggerWarning(squad.SpawnPointId, WarningDurationSeconds);
                
                // Ждем окончания предупреждения. 
                // В отличие от WaitForSeconds, UniTask.Delay не создает мусора (Zero Allocation)
                // и мгновенно прерывается, если ct отменен!
                await UniTask.Delay(TimeSpan.FromSeconds(WarningDurationSeconds), cancellationToken: ct);

#if UNITY_EDITOR
                Debug.Log($"<color=cyan>[SpawnerService] Выходит отряд: {squad.Count}x {squad.EnemyId} (Точка: {squad.SpawnPointId})</color>");
#endif

                // 2. Спавним врагов по очереди
                for (int i = 0; i < squad.Count; i++)
                {
                    SpawnPhysicalEnemy(squad);

                    // Убрали условие `if (i < squad.Count - 1)`. 
                    // Ждем интервал после каждого моба, как в старой корутине!
                    if (squad.SpawnInterval > 0)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(squad.SpawnInterval), cancellationToken: ct);
                    }
                }
            }
        }

        private void SpawnPhysicalEnemy(SquadData squad)
        {
            // Пытаемся получить координаты из реестра спавн-поинтов
            if (!_spawnRegistry.TryGetSpawnPosition(squad.SpawnPointId, out Vector3 spawnPos))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[SpawnerService] Спавн '{squad.SpawnPointId}' не найден! Спавним в (0,0,0).");
#endif
                spawnPos = Vector3.zero; 
            }
            
            // Фабрика сама создаст врага, выдаст ему нужную модельку и отправит к нужной базе
            _enemyFactory.SpawnEnemy(squad.EnemyId, spawnPos, squad.TargetBaseId);
        }
    }
}