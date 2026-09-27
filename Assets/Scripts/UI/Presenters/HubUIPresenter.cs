using System;
using Zenject;
using Cysharp.Threading.Tasks;
using Gameplay.Campaign.Services;
using Gameplay.Campaign.Data;
using Gameplay.Infrastructure.Services;
using Gameplay.UI.Views;
using Gameplay.MapScene.Services;
using Gameplay.Levels.Data;

namespace Gameplay.UI.Presenters
{
    public class HubUIPresenter : IInitializable, IDisposable, IEncounterVisitor
    {
        private readonly HubUIView _view;
        private readonly IRunDirectorService _runDirector;
        private readonly RunProgressModel _progressModel;
        private readonly ISceneLoaderService _sceneLoader;
        private readonly ShopUIPresenter _shopPresenter;
        private readonly MapSceneBuilder _mapBuilder;

        public HubUIPresenter(
            HubUIView view,
            IRunDirectorService runDirector,
            RunProgressModel progressModel,
            ISceneLoaderService sceneLoader,
            ShopUIPresenter shopPresenter,
            MapSceneBuilder mapBuilder)
        {
            _view = view;
            _runDirector = runDirector;
            _progressModel = progressModel;
            _sceneLoader = sceneLoader;
            _shopPresenter = shopPresenter;
            _mapBuilder = mapBuilder;
        }

        public void Initialize()
        {
            _view.OnMainMenuClicked += HandleMainMenu;
            _view.UpdateRunInventory(_progressModel.CurrentRunGold);

            if (_runDirector.IsCampaignCompleted(_progressModel))
            {
                _view.ShowCampaignCompleted();
                return;
            }

            _mapBuilder.OnNodeSelected += HandleNodeSelected;
            _shopPresenter.OnShopClosed += HandleShopClosed;
        }

        public void Dispose()
        {
            _view.OnMainMenuClicked -= HandleMainMenu;
            _mapBuilder.OnNodeSelected -= HandleNodeSelected;
            _shopPresenter.OnShopClosed -= HandleShopClosed;
        }

        private void HandleNodeSelected(MapNode nextNode)
        {
            _view.SetInteractable(false);
            
            _runDirector.AdvanceToNode(_progressModel, nextNode.Id);

            nextNode.Encounter.Accept(this);
        }


        public void VisitCombat(LevelBlueprintConfig config)
        {
            _sceneLoader.LoadSceneAsync("BattleScene").Forget();
        }

        public void VisitShop(ShopConfig config)
        {
            _shopPresenter.OpenShop(config);
        }

        public void VisitEvent(EventConfig config)
        {
            UnityEngine.Debug.Log("<color=cyan>[MapScene] Событие пропущено (UI в разработке).</color>");
            _sceneLoader.LoadSceneAsync("HubScene").Forget();
        }

        public void VisitStart()
        {
            _sceneLoader.LoadSceneAsync("HubScene").Forget();
        }


        private void HandleMainMenu()
        {
            _view.SetInteractable(false);
            _sceneLoader.LoadSceneAsync("MainMenuScene").Forget();
        }

        private void HandleShopClosed()
        {
            _sceneLoader.LoadSceneAsync("HubScene").Forget(); 
        }
    }
}