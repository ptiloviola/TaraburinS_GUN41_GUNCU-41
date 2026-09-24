using Zenject;
using UnityEngine;
using Gameplay.Infrastructure.Input;
using Gameplay.Infrastructure.Services;
using Gameplay.Core.Services;
using Gameplay.Campaign.Data;

namespace Gameplay.Infrastructure.Installers
{
    public class ProjectInstaller : MonoInstaller
    {
        [Header("Режим игры")]
        [Tooltip("Перетащите сюда LinearConfig или ProceduralConfig")]
        [SerializeField] private RunModeConfig _activeRunMode;

        public override void InstallBindings()
        {
            Container.BindInterfacesTo<StandaloneInputService>().AsSingle();
            Container.BindInterfacesTo<PauseService>().AsSingle();
            Container.BindInterfacesAndSelfTo<SceneLoaderService>().AsSingle();

            Container.Bind<RunProgressModel>().AsSingle();
            Container.Bind<SaveLoadService>().AsSingle();

            if (_activeRunMode != null)
            {
                _activeRunMode.InstallModeBindings(Container);
            }
            else
            {
                Debug.LogError("[ProjectInstaller] Не назначен конфиг режима игры (_activeRunMode)!");
            }
        }
    }
}