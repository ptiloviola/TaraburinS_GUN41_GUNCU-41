using UnityEngine;
using Zenject;
using Gameplay.Enemies;
using Gameplay.Enemies.Data;
using Gameplay.Units;
using Gameplay.Units.Data;
using Gameplay.Spawning;
using Gameplay.Base;
using Gameplay.Spawning.Factories;

namespace Gameplay.Spawning.Installers
{
    public class LevelSpawningInstaller : MonoInstaller
    {
        [SerializeField] private EnemyRegistry _enemyRegistry;
        [SerializeField] private DefenderRegistry _defenderRegistry;
        [SerializeField] private BaseCore _basePrefab;
        [SerializeField] private EnemySpawnPoint _spawnMarkerPrefab;

        public override void InstallBindings()
        {
            Container.BindInstance(_enemyRegistry).AsSingle();
            Container.BindInterfacesAndSelfTo<EnemyTrackerService>().AsSingle();

            // Реестры сущностей уровня
            Container.Bind<BaseRegistry>().AsSingle();
            Container.Bind<SpawnRegistry>().AsSingle();

            Container.Bind<BaseLocatorService>().AsSingle();

            // Фабрики для спавна маркеров/баз на этапе генерации уровня
            Container.BindFactory<BaseCore, BaseCore.Factory>()
                     .FromComponentInNewPrefab(_basePrefab).UnderTransformGroup("Bases");
            Container.BindFactory<EnemySpawnPoint, EnemySpawnPoint.Factory>()
                     .FromComponentInNewPrefab(_spawnMarkerPrefab).UnderTransformGroup("Spawns");

            // Настройка пулов Врагов
            if (_enemyRegistry != null)
            {
                foreach (var config in _enemyRegistry.Enemies)
                {
                    if (config.Prefab != null)
                    {
                        Container.BindMemoryPool<EnemyFacade, EnemyFacade.Pool>()
                                 .WithId(config.EnemyId)
                                 .WithInitialSize(5)
                                 .FromComponentInNewPrefab(config.Prefab)
                                 .UnderTransformGroup($"EnemyPool_{config.EnemyId}");
                    }
                }
            }

            Container.Bind<EnemyFactory>().AsSingle();

            // Настройка пулов Защитников
            if (_defenderRegistry != null)
            {
                foreach (var config in _defenderRegistry.Defenders)
                {
                    if (config.Prefab != null)
                    {
                        Container.BindMemoryPool<DefenderFacade, DefenderFacade.Pool>()
                                 .WithId(config.DefenderId)
                                 .WithInitialSize(3)
                                 .FromComponentInNewPrefab(config.Prefab)
                                 .UnderTransformGroup($"DefenderPool_{config.DefenderId}");
                    }
                }
            }
            
            Debug.Log("<color=green>[Zenject] LevelSpawningInstaller: Мульти-пулы и спавн-системы зарегистрированы.</color>");
        }
    }
}