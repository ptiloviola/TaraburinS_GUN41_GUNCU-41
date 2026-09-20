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

        [SerializeField] private PauseMenuView _pauseMenuView;

        [SerializeField] private TacticalUIView _tacticalView;

        [SerializeField] private EndGameUIView _endGameView;

        public override void InstallBindings()
        {

            Container.BindInstance(_shopView).AsSingle();


            Container.BindInterfacesAndSelfTo<TowerShopPresenter>().AsSingle();


            Container.BindMemoryPool<TowerButtonView, TowerButtonView.Pool>()
                     .WithInitialSize(5)
                     .FromComponentInNewPrefab(_buttonPrefab)
                     .UnderTransform(_shopView.ButtonsContainer); 

            Container.BindInstance(_contextMenuView).AsSingle();
            Container.BindInterfacesAndSelfTo<TowerContextMenuPresenter>().AsSingle();

            Container.BindInstance(_baseUIView).AsSingle();
            Container.BindInterfacesAndSelfTo<BaseUIPresenter>().AsSingle();


            Container.BindInstance(_waveUIView).AsSingle();
            Container.BindInterfacesAndSelfTo<WaveUIPresenter>().AsSingle();


            Container.BindMemoryPool<ForecastIconView, ForecastIconView.Pool>()
                     .WithInitialSize(3)
                     .FromComponentInNewPrefab(_forecastIconPrefab)
                     .UnderTransform(_waveUIView.ForecastContainer);

            Container.BindInstance(_pauseMenuView).AsSingle();
            Container.BindInterfacesTo<PauseMenuPresenter>().AsSingle();

            Container.BindInstance(_tacticalView).IfNotBound();
            Container.BindInterfacesTo<TacticalUIPresenter>().AsSingle();

            Container.BindInstance(_endGameView).IfNotBound();
            Container.BindInterfacesTo<EndGameUIPresenter>().AsSingle();

        }
    }
}