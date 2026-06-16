using UnityEngine;
using Zenject;
using Infrastructure.Signals;
using Gameplay.Grid; // Подключаем нашу сетку
using Gameplay.Enemies; 
using Gameplay.Base;
using Gameplay.Towers;
using Gameplay.Towers.Behaviors.Weapons;
using Gameplay.Economy;
using Gameplay.Towers.Data; // Подключаем пространство имен каталога
using Gameplay.Spawning;
using Gameplay.UI;
using Gameplay.Enemies.Data;

namespace Infrastructure.Installers
{
    public class GameplayInstaller : MonoInstaller
    {

        // Ссылка на наш конфиг, которую мы укажем в инспекторе SceneContext
        [SerializeField] private GridConfig gridConfig;

        // Ссылка на префаб врага для пула
        [SerializeField] private GameObject enemyPrefab;

        [Header("Настройки систем")]
        // Появится в инспекторе инсталлера, сюда кидаем префаб и маску!
        [SerializeField] private GridInteractor.Settings gridInteractorSettings;

        [SerializeField] private KinematicProjectile _cannonballPrefab;

        [Header("Конфиги и Данные")]
        [SerializeField] private TowerRegistry _towerRegistry; // Ссылка на наш каталог башен в инспекторе

        [Header("Настройки Слоев")]
        [SerializeField] private LayerMask _towerLayerMask; // Назначь в инспекторе слой Tower!

        [Header("Данные Спавнера")]
        [SerializeField] private EnemyRegistry _enemyRegistry; // Перетащи сюда свой SO Каталога

        [Header("Префаб Базы")]
        [SerializeField] private BaseCore _basePrefab; // Сюда перетащим префаб базы из папки проекта
        [Header("Префаб точки спауна")]
        [SerializeField] private EnemySpawnPoint _spawnMarkerPrefab;

        [Header("UI Префабы")]
        [SerializeField] private ForecastIconView _forecastIconPrefab; // Сюда закинем префаб карточки
        
        public override void InstallBindings()
        {
            // Инициализируем встроенную шину сигналов Zenject
            SignalBusInstaller.Install(Container);

            // Регистрируем наши кастомные сигналы в системе
            Container.DeclareSignal<SignalBaseDamaged>();
            Container.DeclareSignal<SignalEnemyKilled>();
            Container.DeclareSignal<SignalGameOver>();
            Container.DeclareSignal<SignalBalanceChanged>();
            Container.DeclareSignal<SignalWaveStarted>();
            Container.DeclareSignal<SignalWaveTimerUpdated>();
            Container.DeclareSignal<SignalForceStartWave>();
            Container.DeclareSignal<SignalWaveStateChanged>();

            // Биндим наш Банк
            Container.BindInterfacesAndSelfTo<BankService>().AsSingle();

            // Регистрируем экземпляр нашего ScriptableObject в контейнере.
            // // Теперь любой класс может написать [Inject] private GridConfig _config;
            // Container.Bind<GridConfig>().FromInstance(gridConfig).AsSingle();
            Container.Bind<IGridService>().To<GridService>().AsSingle();
            // Трекер живых врагов (Радар)
            Container.BindInterfacesAndSelfTo<EnemyTrackerService>().AsSingle();

            Debug.Log("<color=green>[Zenject] Сетка и её конфигурация успешно зарегистрированы!</color>");


            // Настройка MemoryPool для врагов
            // Мы связываем наш Фасад и внутренний класс Pool
            // Container.BindMemoryPool<EnemyFacade, EnemyFacade.Pool>()
            // .WithInitialSize(5) // Игра сразу создаст 5 сфер в памяти при старте (под капотом)
            // .FromComponentInNewPrefab(enemyPrefab) // Будет брать их из этого префаба
            // .UnderTransformGroup("EnemyPool"); // Спрячет их в иерархии под один пустой объект
            // Debug.Log("<color=green>[Zenject] MemoryPool для EnemyFacade успешно настроен!</color>");

        
            // Проходим по всем врагам в каталоге и создаем для КАЖДОГО свой собственный Пул!
            if (_enemyRegistry != null)
            {
                foreach (var config in _enemyRegistry.Enemies)
                {
                    if (config.Prefab != null)
                    {
                        Container.BindMemoryPool<EnemyFacade, EnemyFacade.Pool>()
                        .WithId(config.EnemyId) // МАГИЯ ЗДЕСЬ: Мы даем пулу имя!
                        .WithInitialSize(5)
                        .FromComponentInNewPrefab(config.Prefab)
                        .UnderTransformGroup($"EnemyPool_{config.EnemyId}"); // Группируем аккуратно
                    }
                }
            }
            // Биндим сам реестр, чтобы WaveDirector мог его запросить
                Container.BindInstance(_enemyRegistry).AsSingle();
                Debug.Log("<color=green>[Zenject] Мульти-пулы для врагов успешно созданы из реестра!</color>");
            

            // Находим базу на сцене и делаем ее доступной для инъекций
            Container.Bind<BaseCore>().FromComponentInHierarchy().AsSingle();


            Container.Bind<GridGenerator>().FromComponentInHierarchy().AsSingle();
            // 1. Биндим настройки
            Container.BindInstance(gridInteractorSettings).IfNotBound();

            // 2. Биндим сам интерактор к двум интерфейсам: 
            // как класс (если кто-то захочет его запросить) и как ITickable (чтобы работал Tick)
            Container.BindInterfacesAndSelfTo<GridInteractor>().AsSingle();

            Container.BindMemoryPool<KinematicProjectile, KinematicProjectile.Pool>()
             .WithInitialSize(10)
             .FromComponentInNewPrefab(_cannonballPrefab)
             .UnderTransformGroup("Projectiles");

            Container.BindInstance(_towerRegistry).AsSingle();

            // Регистрируем сервис выделения (передаем ему камеру и маску слоя)
            Container.BindInterfacesAndSelfTo<TowerSelectionService>()
                     .AsSingle()
                     .WithArguments(Camera.main, _towerLayerMask)
                     .NonLazy();

            // 1. Регистрируем реестр как единственный экземпляр на сцену
            Container.Bind<BaseRegistry>().AsSingle();
            Container.Bind<SpawnRegistry>().AsSingle(); // НОВОЕ

            // 2. Регистрируем фабрику префабов баз и точек спауна на сетке
            Container.BindFactory<BaseCore, BaseCore.Factory>()
                .FromComponentInNewPrefab(_basePrefab).UnderTransformGroup("Bases");
                
            Container.BindFactory<EnemySpawnPoint, EnemySpawnPoint.Factory>()
                .FromComponentInNewPrefab(_spawnMarkerPrefab).UnderTransformGroup("Spawns");

            // Регистрируем сервис здоровья игрока (как и банк)
            Container.BindInterfacesAndSelfTo<PlayerHealthService>().AsSingle();

            Container.DeclareSignal<SignalEnemySpawned>();
            Container.DeclareSignal<SignalAllEnemiesCleared>();
            Container.DeclareSignal<SignalEnemyReachedBase>();

            Container.DeclareSignal<SignalWaveForecastUpdated>();

            // Регистрируем фабрику UI-иконок
            Container.BindFactory<ForecastIconView, ForecastIconView.Factory>()
                 .FromComponentInNewPrefab(_forecastIconPrefab);
            
        }
    }
}