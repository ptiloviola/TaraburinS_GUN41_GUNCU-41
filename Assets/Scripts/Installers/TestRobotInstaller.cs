using UnityEngine;
using Zenject;
using VacuumSim.Robotics.Contracts;
using VacuumSim.Robotics.Components;
using VacuumSim.Robotics.Brain;
using VacuumSim.Robotics.Configs;
using VacuumSim.Robotics.Signals;

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

            // 2. Биндим логику. 
            // Zenject сам сделает 'new RandomBounceBrain()', сам подтянет для него
            // _motor и _sensors из биндов выше, и сохранит в памяти как IVacuumBrain.
            Container.Bind<IVacuumBrain>().To<RandomBounceBrain>().AsSingle();
        }
    }
}