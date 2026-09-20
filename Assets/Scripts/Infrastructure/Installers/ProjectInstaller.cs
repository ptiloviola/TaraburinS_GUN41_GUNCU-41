using Zenject;
using Gameplay.Infrastructure.Input;
using Gameplay.Infrastructure.Services;
using Gameplay.Core.Data;

namespace Gameplay.Infrastructure.Installers
{
    public class ProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<StandaloneInputService>().AsSingle();
            
            Container.BindInterfacesTo<PauseService>().AsSingle();

            Container.BindInterfacesAndSelfTo<SceneLoaderService>().AsSingle();

            Container.Bind<RunProgressModel>().AsSingle();
            
            
        }
    }
}