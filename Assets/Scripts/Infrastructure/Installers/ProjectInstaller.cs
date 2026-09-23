using Zenject;
using UnityEngine;
using Gameplay.Infrastructure.Input;
using Gameplay.Infrastructure.Services;
using Gameplay.Combat.Data;
using Gameplay.Combat.Services;
using Gameplay.Core.Services;

namespace Gameplay.Infrastructure.Installers
{
    public class ProjectInstaller : MonoInstaller
    {

        [SerializeField] private CampaignConfig _mainCampaign;

        public override void InstallBindings()
        {
            Container.BindInterfacesTo<StandaloneInputService>().AsSingle();
            
            Container.BindInterfacesTo<PauseService>().AsSingle();

            Container.BindInterfacesAndSelfTo<SceneLoaderService>().AsSingle();

            Container.Bind<RunProgressModel>().AsSingle();
            Container.Bind<SaveLoadService>().AsSingle();

            Container.BindInstance(_mainCampaign).IfNotBound();
            Container.Bind<IRunDirectorService>().To<LinearRunDirector>().AsSingle();

        }
    }
}