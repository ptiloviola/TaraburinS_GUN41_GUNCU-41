using UnityEngine;
using Zenject;
using Gameplay.Enemies;
using Gameplay.Base;
using Gameplay.Infrastructure.Signals;
using Gameplay.Enemies.Data;

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
            try
            {
                EnemyConfig config = _enemyRegistry.GetEnemyById(enemyId);
                if (config == null)
                {
#if UNITY_EDITOR
                    Gameplay.Tools.GameLogger.LogError($"[EnemyFactory] Враг '{enemyId}' не найден в EnemyRegistry!");
#endif
                    return;
                }
                
                EnemyFacade.Pool specificPool = _container.ResolveId<EnemyFacade.Pool>(enemyId);
                EnemyFacade enemy = specificPool.Spawn();
                
                enemy.SetPool(specificPool);
                enemy.InitConfig(config);
                
                if (enemy.Agent != null)
                {
                    enemy.Agent.enabled = false;
                    
                    Vector3 finalPos = spawnPos;
                    finalPos.y += enemy.Agent.baseOffset;
                    
                    enemy.transform.position = finalPos;
                    enemy.Agent.enabled = true;
                }

                if (string.IsNullOrEmpty(targetBaseId))
                {
                    targetBaseId = BaseLocatorService.NearestByPathTag; 
                }
                
                BaseCore targetBase = _baseLocatorService.LocateTargetBase(targetBaseId, spawnPos);
                
                if (targetBase != null)
                {
                    IMovementStrategy movement = config.Movement.CreateStrategy(targetBase.transform.position);
                    enemy.InitializeMovement(movement);
                }
                else
                {
#if UNITY_EDITOR
                    Gameplay.Tools.GameLogger.LogError("[EnemyFactory] Ошибка! Враг заспавнен, но в реестре BaseRegistry нет активной базы!");
#endif
                }

                _signalBus.Fire<SignalEnemySpawned>();
            }
            catch (ZenjectException)
            {
#if UNITY_EDITOR
                Gameplay.Tools.GameLogger.LogError($"[EnemyFactory] Ошибка спавна! Пул для врага '{enemyId}' не найден. Проверь Installer!");
#endif
            }
        }
        public void OnSpawnEnemyRequested(SignalSpawnEnemyRequest request)
        {
            SpawnEnemy(request.EnemyId, request.Position, request.TargetBaseId);
        }
    }
}