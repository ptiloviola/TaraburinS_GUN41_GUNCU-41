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

        [Header("Базовый UI (Экономика и Жизни)")]
        [SerializeField] private BaseUIView _baseUIView;

        [Header("Управление Волнами")]
        [SerializeField] private WaveUIView _waveUIView;
        [SerializeField] private ForecastIconView _forecastIconPrefab;

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

            Container.BindInstance(_baseUIView).AsSingle();
            Container.BindInterfacesAndSelfTo<BaseUIPresenter>().AsSingle();

            // 1. Биндим View и Presenter волн
            Container.BindInstance(_waveUIView).AsSingle();
            Container.BindInterfacesAndSelfTo<WaveUIPresenter>().AsSingle();

            // 2. Создаем пул иконок и сразу кладем их в ForecastContainer
            Container.BindMemoryPool<ForecastIconView, ForecastIconView.Pool>()
                     .WithInitialSize(3)
                     .FromComponentInNewPrefab(_forecastIconPrefab)
                     .UnderTransform(_waveUIView.ForecastContainer);

        }
    }
}