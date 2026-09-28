using UnityEngine;
using Zenject;
using Gameplay.UI.Views;
using Gameplay.UI.Presenters;

namespace Gameplay.UI.Installers
{
    public class MainMenuInstaller : MonoInstaller
    {
        [SerializeField] private MainMenuView _mainMenuView;

        public override void InstallBindings()
        {
            Container.BindInstance(_mainMenuView).AsSingle();

            Container.BindInterfacesTo<MainMenuPresenter>().AsSingle();
            
            Gameplay.Tools.GameLogger.Log("<color=green>[Zenject] MainMenuInstaller: Главное меню успешно собрано.</color>");
        }
    }
}