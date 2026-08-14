using TpsShooter.Services.Input;
using TpsShooter.Services.Progress;
using TpsShooter.Services.SceneManagement;
using Zenject;

namespace TpsShooter.Infrastructure
{
    public class GlobalInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // Ввод
            Container.BindInterfacesAndSelfTo<UnityInputService>().AsSingle();

            // Глобальные сервисы прогресса и сцен (AsSingle означает, что это синглтоны в рамках контейнера)
            Container.Bind<GameProgressService>().AsSingle();
            Container.Bind<SceneLoaderService>().AsSingle();
        }
    }
}