using Zenject;
using Gameplay.Infrastructure.Input;

namespace Gameplay.Infrastructure.Installers
{
    public class ProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // Главный контроллер ввода — один на весь проект
            Container.BindInterfacesTo<StandaloneInputService>().AsSingle();
        }
    }
}