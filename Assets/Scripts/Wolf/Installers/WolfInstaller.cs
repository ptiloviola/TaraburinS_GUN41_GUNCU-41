using MeatMushrooms.Wolf.Components;
using MeatMushrooms.Wolf.Contracts;
using MeatMushrooms.Wolf.States;
using Zenject;

namespace MeatMushrooms.Wolf.Installers
{
    public class WolfInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // --- 1. Компоненты на самом GameObject ---
            Container.Bind<WolfLocomotion>().FromComponentOnRoot().AsSingle();
            Container.Bind<WolfSenses>().FromComponentOnRoot().AsSingle();

            // --- 2. Чистые C# классы ---
            Container.BindInterfacesAndSelfTo<WolfStats>().AsSingle();

            // --- 3. Состояния ---
            Container.Bind<IWolfState>().To<IdleState>().AsSingle();
            Container.Bind<IWolfState>().To<WanderState>().AsSingle();
            Container.Bind<IWolfState>().To<HuntFoodState>().AsSingle();
            Container.Bind<IWolfState>().To<EatState>().AsSingle();

            // --- 4. Мозг ---
            Container.BindInterfacesAndSelfTo<WolfBrain>().AsSingle();
        }
    }
}