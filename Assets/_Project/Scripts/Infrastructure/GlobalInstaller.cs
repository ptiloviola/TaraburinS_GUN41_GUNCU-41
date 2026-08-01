using TpsShooter.Services.Input;
using Zenject;

namespace TpsShooter.Infrastructure
{
    public class GlobalInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<UnityInputService>().AsSingle();
        }
    }
}