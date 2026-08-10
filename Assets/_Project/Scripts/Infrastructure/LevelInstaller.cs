using Zenject;
using TpsShooter.Player;
using TpsShooter.Effects;

namespace TpsShooter.Infrastructure
{
    public class LevelInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // Ищем компонент PlayerFacade на текущей сцене и регистрируем его как синглтон для этого уровня.
            // .NonLazy() заставляет Zenject сразу же инициализировать этот объект и вызвать метод Construct.
            Container.Bind<PlayerFacade>()
                     .FromComponentInHierarchy()
                     .AsSingle()
                     .NonLazy();
            Container.Bind<DecalManager>().FromComponentInHierarchy().AsSingle();
        }
    }
}