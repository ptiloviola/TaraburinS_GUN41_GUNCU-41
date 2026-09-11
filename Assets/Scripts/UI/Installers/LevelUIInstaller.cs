using UnityEngine;
using Zenject;
using Gameplay.UI.Views;
using Gameplay.UI.Presenters;

namespace Gameplay.UI.Installers
{
    public class LevelUIInstaller : MonoInstaller
    {
        [Header("Магазин башен")]
        [SerializeField] private TowerShopView _shopView;
        [SerializeField] private TowerButtonView _buttonPrefab;

        [Header("Контекстное меню")]
        [SerializeField] private TowerContextMenuView _contextMenuView;

        public override void InstallBindings()
        {
            // 1. Биндим View (глупый интерфейс на сцене)
            Container.BindInstance(_shopView).AsSingle();

            // 2. Биндим Presenter (чистая логика). 
            // Zenject сам вызовет его методы Initialize() и Dispose()
            Container.BindInterfacesAndSelfTo<TowerShopPresenter>().AsSingle();

            // 3. Биндим пул для кнопок магазина (избавляемся от Instantiate!)
            Container.BindMemoryPool<TowerButtonView, TowerButtonView.Pool>()
                     .WithInitialSize(5)
                     .FromComponentInNewPrefab(_buttonPrefab)
                     .UnderTransform(_shopView.ButtonsContainer); 

            Container.BindInstance(_contextMenuView).AsSingle();
            Container.BindInterfacesAndSelfTo<TowerContextMenuPresenter>().AsSingle();

        }
    }
}