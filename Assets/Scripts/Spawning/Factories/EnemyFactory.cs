using UnityEngine;
using Zenject;
using Gameplay.Enemies;
using Gameplay.Base;
using Infrastructure.Signals;
using Gameplay.Enemies.Data;

namespace Gameplay.Spawning.Factories
{
    /// <summary>
    /// Фабрика для безопасного извлечения врагов из Zenject MemoryPool и их инициализации.
    /// </summary>
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
                    Debug.LogError($"[EnemyFactory] Враг '{enemyId}' не найден в EnemyRegistry!");
#endif
                    return;
                }
                
                // 1. Извлекаем врага из пула по его ID
                EnemyFacade.Pool specificPool = _container.ResolveId<EnemyFacade.Pool>(enemyId);
                EnemyFacade enemy = specificPool.Spawn();
                
                // 2. Инициализация базовых данных (связь с пулом и конфигом)
                enemy.SetPool(specificPool);
                enemy.InitConfig(config);
                
                // 3. Установка позиции через инкапсулированное свойство агента (без GetComponent!)
                if (enemy.Agent != null)
                {
                    enemy.Agent.enabled = false;
                    
                    // Поднимаем точку спавна на высоту полета (Base Offset),
                    // чтобы невидимые "ноги" агента точно попали на NavMesh
                    Vector3 finalPos = spawnPos;
                    finalPos.y += enemy.Agent.baseOffset;
                    
                    enemy.transform.position = finalPos;
                    enemy.Agent.enabled = true;
                }

                // 4. Поиск цели
                if (string.IsNullOrEmpty(targetBaseId))
                {
                    targetBaseId = BaseLocatorService.NearestByPathTag; 
                }
                
                BaseCore targetBase = _baseLocatorService.LocateTargetBase(targetBaseId, spawnPos);
                
                // 5. Выдача приказа на движение
                if (targetBase != null)
                {
                    IMovementStrategy movement = config.Movement.CreateStrategy(targetBase.transform.position);
                    enemy.InitializeMovement(movement);
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogError("[EnemyFactory] Ошибка! Враг заспавнен, но в реестре BaseRegistry нет активной базы!");
#endif
                }

                // 6. Уведомляем систему ТОЛЬКО когда враг полностью готов к бою
                _signalBus.Fire<SignalEnemySpawned>();
            }
            catch (ZenjectException)
            {
#if UNITY_EDITOR
                Debug.LogError($"[EnemyFactory] Ошибка спавна! Пул для врага '{enemyId}' не найден. Проверь Installer!");
#endif
            }
        }
    }
}