using Zenject;
using Gameplay.Infrastructure.Input;
using Gameplay.Infrastructure.Services; // Добавлено

namespace Gameplay.Infrastructure.Installers
{
    public class ProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<StandaloneInputService>().AsSingle();
            
            // Биндим глобальные сервисы
            Container.BindInterfacesTo<PauseService>().AsSingle();
            Container.BindInterfacesTo<SceneLoaderService>().AsSingle();
        }
    }
}