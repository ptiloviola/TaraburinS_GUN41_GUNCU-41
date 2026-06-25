using UnityEngine;
using Zenject;
using VacuumSim.Robotics.Contracts;
using VacuumSim.Robotics.Components;
using VacuumSim.Robotics.Brain;
using VacuumSim.Robotics.Configs;
using VacuumSim.Robotics.Signals;
using VacuumSim.UI;
using VacuumSim.Robotics.Brain.States;
using VacuumSim.Robotics.Brain.Strategies;
using VacuumSim.Pathfinding;
using VacuumSim.Input;

namespace VacuumSim.Installers
{
    public class TestRobotInstaller : MonoInstaller
    {
        [Header("Конфигурация")]
        [SerializeField] private VacuumConfig _config;
        [Header("Ссылки на компоненты пылесоса")]
        [SerializeField] private VacuumMotor _motor;
        [SerializeField] private VacuumRaycastSensors _sensors;

        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);

            Container.DeclareSignal<TrashCollectedSignal>();
            Container.DeclareSignal<BatteryStateSignal>();
            Container.DeclareSignal<DustbinStateSignal>();

            // 3. Биндим нашу чистую логику батареи
            // BindInterfacesTo означает: "Свяжи этот класс со всеми интерфейсами, которые он реализует 
            // (IVacuumBattery, IInitializable, ITickable, IDisposable)".
            // AsSingle означает: "Создай его ровно ОДИН раз для этого контекста".
            Container.BindInterfacesTo<VacuumBatteryManager>().AsSingle();
            Container.BindInterfacesTo<VacuumDustbinManager>().AsSingle();


            Container.BindInstance(_config).AsSingle();
            // 1. Отдаем в контейнер ссылки на физические компоненты со сцены
            Container.Bind<IVacuumMotor>().FromInstance(_motor).AsSingle();
            Container.Bind<IVacuumSensors>().FromInstance(_sensors).AsSingle();

            // 1. Регистрируем наши состояния и стратегии
            Container.Bind<CleaningState>().AsSingle();
            Container.Bind<ReturnToBaseState>().AsSingle();

            // Порядок здесь важен! 
            // Индекс 0 = Случайная (если у тебя остался скрипт RandomBounceStrategy)
            // Индекс 1 = Змейка
            // Индекс 2 = Спираль
            Container.Bind<ICleaningStrategy>().To<RandomBounceStrategy>().AsSingle();
            Container.Bind<ICleaningStrategy>().To<ZigZagStrategy>().AsSingle();
            Container.Bind<ICleaningStrategy>().To<SpiralStrategy>().AsSingle();

            // 2. Регистрируем наше новое архитектурное ядро мозга
            // Связываем его и с интерфейсом IVacuumBrain, и с интерфейсом старта IInitializable
            Container.BindInterfacesAndSelfTo<SmartBrain>().AsSingle();

            // Биндим View. 
            // "FromComponentInHierarchy" означает: "Zenject, найди на сцене объект с этим скриптом сам".
            Container.Bind<VacuumDashboardView>().FromComponentInHierarchy().AsSingle();

            // Биндим Presenter. Он чистый класс, поэтому просто "BindInterfacesTo".
            Container.BindInterfacesTo<VacuumDashboardPresenter>().AsSingle();

            // Биндим компоненты со сцены (Zenject сам найдет их на сцене)
            Container.Bind<PathfindingGrid>().FromComponentInHierarchy().AsSingle();
            Container.Bind<BaseStation>().FromComponentInHierarchy().AsSingle();

            // Биндим чистую логику Искателя Пути
            Container.Bind<Pathfinder>().AsSingle();

            Container.DeclareSignal<ArrivedAtBaseSignal>().OptionalSubscriber();
            Container.Bind<DockedState>().AsSingle();

            Container.Bind<VacuumCollector>().FromComponentInHierarchy().AsSingle();

            Container.BindInterfacesAndSelfTo<PlayerInputHandler>().AsSingle();
            Container.Bind<ManualTransitState>().AsSingle();

            Container.DeclareSignal<TargetPointSelectedSignal>().OptionalSubscriber();
            Container.DeclareSignal<TransitCompletedSignal>().OptionalSubscriber();
        }
    }
}