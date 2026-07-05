using MeatMushrooms.Wolf.Components;
using MeatMushrooms.Wolf.Contracts;
using MeatMushrooms.Wolf.States;
using Zenject;
using MeatMushrooms.Wolf.Configs;
using UnityEngine;
using MeatMushrooms.Wolf.DebugTools;


namespace MeatMushrooms.Wolf.Installers
{
    public class WolfInstaller : MonoInstaller
    {
        [SerializeField] private WolfConfig _wolfConfig;
        public override void InstallBindings()
        {

            Container.Bind<WolfEventBus>().AsSingle();


            Container.BindInstance(_wolfConfig).AsSingle();

            Container.Bind<WolfLocomotion>().FromComponentOnRoot().AsSingle();
            Container.Bind<WolfSenses>().FromComponentOnRoot().AsSingle();
            Container.Bind<WolfAnimator>().FromComponentOnRoot().AsSingle();


            Container.BindInterfacesAndSelfTo<WolfStats>().AsSingle();


            Container.Bind<IWolfState>().To<IdleState>().AsSingle();
            Container.Bind<IWolfState>().To<WanderState>().AsSingle();
            Container.Bind<IWolfState>().To<HuntFoodState>().AsSingle();
            Container.Bind<IWolfState>().To<EatState>().AsSingle();
            Container.Bind<IWolfState>().To<HowlState>().AsSingle();
            Container.Bind<IWolfState>().To<InvestigateState>().AsSingle();
            Container.Bind<IWolfState>().To<ChaseState>().AsSingle();
  

            Container.Bind<WolfSocial>().FromComponentOnRoot().AsSingle();


            Container.BindInterfacesAndSelfTo<WolfBrain>().AsSingle();

            Container.Bind<WolfPerception>().FromComponentOnRoot().AsSingle();

            Container.Bind<WolfDebugger>().FromComponentOnRoot().AsSingle();
        }
    }
}