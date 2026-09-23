using System;
using Zenject;
using Cysharp.Threading.Tasks;
using Gameplay.Combat.Services;
using Gameplay.Combat.Data;
using Gameplay.Infrastructure.Services;
using Gameplay.UI.Views;

namespace Gameplay.UI.Presenters
{
    public class HubUIPresenter : IInitializable, IDisposable
    {
        private readonly HubUIView _view;
        private readonly IRunDirectorService _runDirector;
        private readonly RunProgressModel _progressModel;
        private readonly ISceneLoaderService _sceneLoader;

        private readonly ShopUIPresenter _shopPresenter;

        private MapNode _nextNode;

        public HubUIPresenter(
            HubUIView view,
            IRunDirectorService runDirector,
            RunProgressModel progressModel,
            ISceneLoaderService sceneLoader,
            ShopUIPresenter shopPresenter)
        {
            _view = view;
            _runDirector = runDirector;
            _progressModel = progressModel;
            _sceneLoader = sceneLoader;
            _shopPresenter = shopPresenter;
        }

        public void Initialize()
        {
            _view.OnStartBattleClicked += HandleActionClicked;
            _view.OnMainMenuClicked += HandleMainMenu;

            _view.UpdateRunInventory(_progressModel.CurrentRunGold);

            if (_runDirector.HasNextNode(_progressModel))
            {
                _nextNode = _runDirector.GetNextNode(_progressModel);
                
                _view.ShowNextLevelInfo(_nextNode.NodeDisplayName);
            }
            else
            {
                _view.ShowCampaignCompleted();
            }
            _shopPresenter.OnShopClosed += HandleShopClosed;
        }

        public void Dispose()
        {
            _view.OnStartBattleClicked -= HandleActionClicked;
            _view.OnMainMenuClicked -= HandleMainMenu;
            _shopPresenter.OnShopClosed -= HandleShopClosed;
        }

        private void HandleActionClicked()
        {
            if (_nextNode == null) return;
            _view.SetInteractable(false);
            
            _progressModel.CurrentNode = _nextNode;

            switch (_nextNode.NodeType)
            {
                case MapNodeType.Combat:
                    _sceneLoader.LoadSceneAsync("BattleScene").Forget();
                    break;
                
                case MapNodeType.Shop:
                    
                    UnityEngine.Debug.Log("<color=cyan>[MapScene] Игрок зашел в Магазин! (UI магазина пока не реализован)</color>");
                    _shopPresenter.OpenShop(_nextNode.ShopData);
                    break;
                
                case MapNodeType.Event:
                    UnityEngine.Debug.Log("Событие пока не реализовано.");
                    break;
            }
        }

        private void HandleMainMenu()
        {
            _view.SetInteractable(false);
            _sceneLoader.LoadSceneAsync("MainMenuScene").Forget();
        }

        private void HandleShopClosed()
        {
            _progressModel.CurrentRunDepth++; 
            _sceneLoader.LoadSceneAsync("HubScene").Forget(); 
        }
    }
}