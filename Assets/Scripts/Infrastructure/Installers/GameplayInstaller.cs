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
using Gameplay.Spawning.Data; // Подключаем данные спавнера
using Gameplay.Spawning;

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

            // Биндим наш Банк
            Container.BindInterfacesAndSelfTo<BankService>().AsSingle();

            // Регистрируем экземпляр нашего ScriptableObject в контейнере.
            // // Теперь любой класс может написать [Inject] private GridConfig _config;
            // Container.Bind<GridConfig>().FromInstance(gridConfig).AsSingle();
            Container.Bind<IGridService>().To<GridService>().AsSingle();

            Debug.Log("<color=green>[Zenject] Сетка и её конфигурация успешно зарегистрированы!</color>");


            // Настройка MemoryPool для врагов
            // Мы связываем наш Фасад и внутренний класс Pool
            // Container.BindMemoryPool<EnemyFacade, EnemyFacade.Pool>()
            // .WithInitialSize(5) // Игра сразу создаст 5 сфер в памяти при старте (под капотом)
            // .FromComponentInNewPrefab(enemyPrefab) // Будет брать их из этого префаба
            // .UnderTransformGroup("EnemyPool"); // Спрячет их в иерархии под один пустой объект
            // Debug.Log("<color=green>[Zenject] MemoryPool для EnemyFacade успешно настроен!</color>");

        
            // Проходим по всем врагам в каталоге и создаем для КАЖДОГО свой собственный Пул!
            foreach (var enemyData in _enemyRegistry.Enemies)
            {
                Container.BindMemoryPool<EnemyFacade, EnemyFacade.Pool>()
                    .WithId(enemyData.EnemyId) // МАГИЯ ЗДЕСЬ: Мы даем пулу имя!
                    .WithInitialSize(5)
                    .FromComponentInNewPrefab(enemyData.Prefab)
                    .UnderTransformGroup($"EnemyPool_{enemyData.EnemyId}"); // Группируем аккуратно
            }

            Debug.Log("<color=green>[Zenject] Мульти-пулы для врагов успешно созданы!</color>");






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
        }
    }
}