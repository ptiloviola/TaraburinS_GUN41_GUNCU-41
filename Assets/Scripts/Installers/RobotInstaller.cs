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
using VacuumSim.Rules;
using VacuumSim.Cat.Brain;
using VacuumSim.GameConfigs;

namespace VacuumSim.Installers
{
    public class RobotInstaller : MonoInstaller
    {
        [Header("Конфигурация")]
        [SerializeField] private VacuumConfig _config;
        [SerializeField] private GameConfig _gameConfig;
        [Header("Ссылки на компоненты пылесоса")]
        [SerializeField] private VacuumMotor _motor;
        [SerializeField] private VacuumRaycastSensors _sensors;

        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);

            Container.DeclareSignal<TrashCollectedSignal>();
            Container.DeclareSignal<BatteryStateSignal>();
            Container.DeclareSignal<DustbinStateSignal>();

            Container.BindInterfacesTo<VacuumBatteryManager>().AsSingle();
            Container.BindInterfacesTo<VacuumDustbinManager>().AsSingle();


            Container.BindInstance(_config).AsSingle();
            Container.BindInstance(_gameConfig).AsSingle();

            Container.Bind<IVacuumMotor>().FromInstance(_motor).AsSingle();
            Container.Bind<IVacuumSensors>().FromInstance(_sensors).AsSingle();


            Container.Bind<CleaningState>().AsSingle();
            Container.Bind<ReturnToBaseState>().AsSingle();

            Container.Bind<ICleaningStrategy>().To<RandomBounceStrategy>().AsSingle();
            Container.Bind<ICleaningStrategy>().To<ZigZagStrategy>().AsSingle();
            Container.Bind<ICleaningStrategy>().To<SpiralStrategy>().AsSingle();

            Container.BindInterfacesAndSelfTo<SmartBrain>().AsSingle();

            Container.Bind<VacuumDashboardView>().FromComponentInHierarchy().AsSingle();

            Container.BindInterfacesTo<VacuumDashboardPresenter>().AsSingle();

            Container.Bind<PathfindingGrid>().FromComponentInHierarchy().AsSingle();
            Container.Bind<BaseStation>().FromComponentInHierarchy().AsSingle();

            Container.Bind<Pathfinder>().AsSingle();

            Container.DeclareSignal<ArrivedAtBaseSignal>().OptionalSubscriber();
            Container.Bind<DockedState>().AsSingle();

            Container.Bind<VacuumCollector>().FromComponentInHierarchy().AsSingle();

            Container.BindInterfacesAndSelfTo<PlayerInputHandler>().AsSingle();
            Container.Bind<ManualTransitState>().AsSingle();

            Container.DeclareSignal<TargetPointSelectedSignal>().OptionalSubscriber();
            Container.DeclareSignal<TransitCompletedSignal>().OptionalSubscriber();

            Container.BindInterfacesTo<GameRuleChecker>().AsSingle();
            Container.DeclareSignal<GameOverSignal>().OptionalSubscriber();



            Container.Bind<CatBrain>().FromComponentInHierarchy().AsSingle();
        }
    }
}