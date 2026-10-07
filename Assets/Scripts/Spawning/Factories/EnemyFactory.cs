using System;
using UnityEngine;
using Zenject;
using Gameplay.Enemies;
using Gameplay.Base;
using Gameplay.Infrastructure.Signals;
using Gameplay.Enemies.Data;
using Gameplay.Enemies.Movement;

namespace Gameplay.Spawning.Factories
{
    public class EnemyFactory
    {
        private readonly DiContainer _container;
        private readonly EnemyRegistry _enemyRegistry;
        private readonly BaseLocatorService _baseLocatorService;
        private readonly SignalBus _signalBus;

        public EnemyFactory(
            DiContainer container, 
            EnemyRegistry enemyRegistry, 
            BaseLocatorService baseLocatorService, 
            SignalBus signalBus)
        {
            _container = container;
            _enemyRegistry = enemyRegistry;
            _baseLocatorService = baseLocatorService;
            _signalBus = signalBus;
        }

        public void SpawnEnemy(string enemyId, Vector3 spawnPos, string targetBaseId)
        {
            EnemyConfig config = _enemyRegistry.GetEnemyById(enemyId);
            if (config == null)
            {
                Gameplay.Tools.GameLogger.LogError($"[EnemyFactory] Враг '{enemyId}' не найден в EnemyRegistry!");
                return;
            }

            EnemyFacade.Pool specificPool;
            try
            {
                specificPool = _container.ResolveId<EnemyFacade.Pool>(enemyId);
            }
            catch (ZenjectException)
            {
                Gameplay.Tools.GameLogger.LogError($"[EnemyFactory] Ошибка спавна! Пул для врага '{enemyId}' не найден.");
                return;
            }

            if (string.IsNullOrEmpty(targetBaseId))
            {
                targetBaseId = BaseLocatorService.NearestByPathTag; 
            }
            
            BaseCore targetBase = _baseLocatorService.LocateTargetBase(targetBaseId, spawnPos);
            
            if (targetBase == null)
            {
                Gameplay.Tools.GameLogger.LogError("[EnemyFactory] Ошибка! База не найдена. Спавн отменен.");
                return;
            }

            EnemyFacade enemy = specificPool.Spawn();
            
            IMovementStrategy movement = config.Movement.CreateStrategy(targetBase.transform.position);
            
            enemy.Initialize(config, movement, spawnPos);

            Action<EnemyFacade> despawnHandler = null;
            despawnHandler = (facade) =>
            {
                facade.OnDespawnRequested -= despawnHandler;
                specificPool.Despawn(facade);
            };
            enemy.OnDespawnRequested += despawnHandler;
            _signalBus.Fire<SignalEnemySpawned>();
        }

        public void OnSpawnEnemyRequested(SignalSpawnEnemyRequest request)
        {
            SpawnEnemy(request.EnemyId, request.Position, request.TargetBaseId);
        }
    }
}