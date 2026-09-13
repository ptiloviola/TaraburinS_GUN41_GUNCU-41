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
            // Биндим View как инстанс, так как он уже висит на сцене
            Container.BindInstance(_mainMenuView).AsSingle();

            // Биндим Presenter, заставляя Zenject управлять его Initialize() и Dispose()
            Container.BindInterfacesTo<MainMenuPresenter>().AsSingle();
            
            Debug.Log("<color=green>[Zenject] MainMenuInstaller: Главное меню успешно собрано.</color>");
        }
    }
}