using MeatMushrooms.Wolf.Components;
using MeatMushrooms.Wolf.Contracts;
using MeatMushrooms.Wolf.States;
using Zenject;
using MeatMushrooms.Wolf.Configs;
using UnityEngine;
using MeatMushrooms.Wolf.DebugTools;
using MeatMushrooms.Player;
using MeatMushrooms.Player.Components;
using MeatMushrooms.Wolf.Signals;

namespace MeatMushrooms.Wolf.Installers
{
    public class WolfInstaller : MonoInstaller
    {
        // 1. Поле для нашего конфига
        [SerializeField] private WolfConfig _wolfConfig;
        public override void InstallBindings()
        {

            Container.Bind<WolfEventBus>().AsSingle();

            Container.DeclareSignal<WolfHowlSignal>();
            Container.DeclareSignal<WolfCombatGrowlSignal>();
            Container.DeclareSignal<WolfLowGrowlSignal>();
            Container.DeclareSignal<WolfEatSignal>();



            Container.BindInstance(_wolfConfig).AsSingle();
            // --- 1. Компоненты на самом GameObject ---
            Container.Bind<WolfLocomotion>().FromComponentOnRoot().AsSingle();
            Container.Bind<WolfSenses>().FromComponentOnRoot().AsSingle();
            Container.Bind<WolfAnimator>().FromComponentOnRoot().AsSingle();

            // --- 2. Чистые C# классы ---
            Container.BindInterfacesAndSelfTo<WolfStats>().AsSingle();

            // --- 3. Состояния ---
            Container.Bind<IWolfState>().To<IdleState>().AsSingle();
            Container.Bind<IWolfState>().To<WanderState>().AsSingle();
            Container.Bind<IWolfState>().To<HuntFoodState>().AsSingle();
            Container.Bind<IWolfState>().To<EatState>().AsSingle();
            Container.Bind<IWolfState>().To<HowlState>().AsSingle();
            Container.Bind<IWolfState>().To<InvestigateState>().AsSingle();
            Container.Bind<IWolfState>().To<ChaseState>().AsSingle();
  

            Container.Bind<WolfSocial>().FromComponentOnRoot().AsSingle();

            // --- 4. Мозг ---
            Container.BindInterfacesAndSelfTo<WolfBrain>().AsSingle();

            Container.Bind<WolfPerception>().FromComponentOnRoot().AsSingle();

            Container.Bind<WolfDebugger>().FromComponentOnRoot().AsSingle();
        }
    }
}