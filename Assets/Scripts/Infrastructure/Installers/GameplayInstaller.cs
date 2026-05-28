using UnityEngine;
using Zenject;
using Infrastructure.Signals;
using Gameplay.Grid; // Подключаем нашу сетку
using Gameplay.Enemies; 
using Gameplay.Spawner;
using Gameplay.Base;
using Gameplay.Towers;
using Gameplay.Towers.Behaviors.Weapons;


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


        public override void InstallBindings()
        {
            // Инициализируем встроенную шину сигналов Zenject
            SignalBusInstaller.Install(Container);

            // Регистрируем наши кастомные сигналы в системе
            Container.DeclareSignal<SignalBaseDamaged>();
            Container.DeclareSignal<SignalEnemyDied>();

            // Регистрируем экземпляр нашего ScriptableObject в контейнере.
            // // Теперь любой класс может написать [Inject] private GridConfig _config;
            // Container.Bind<GridConfig>().FromInstance(gridConfig).AsSingle();
            Container.Bind<IGridService>().To<GridService>().AsSingle();

            Debug.Log("<color=green>[Zenject] Сетка и её конфигурация успешно зарегистрированы!</color>");


            // Настройка MemoryPool для врагов
            // Мы связываем наш Фасад и внутренний класс Pool
            Container.BindMemoryPool<EnemyFacade, EnemyFacade.Pool>()
            .WithInitialSize(5) // Игра сразу создаст 5 сфер в памяти при старте (под капотом)
            .FromComponentInNewPrefab(enemyPrefab) // Будет брать их из этого префаба
            .UnderTransformGroup("EnemyPool"); // Спрячет их в иерархии под один пустой объект

            Debug.Log("<color=green>[Zenject] MemoryPool для EnemyFacade успешно настроен!</color>");
            
            // Находим спавнер на сцене и разрешаем его зависимости при старте
            Container.Bind<WaveSpawner>().FromComponentInHierarchy().AsSingle();

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






        
        }
    }
}