using System;
using Zenject;
using Cysharp.Threading.Tasks;
using Gameplay.Core.Services;
using Gameplay.Core.Data;
using Gameplay.Infrastructure.Services;
using Gameplay.UI.Views;
using Gameplay.Campaign.Data;

namespace Gameplay.UI.Presenters
{
    public class HubUIPresenter : IInitializable, IDisposable
    {
        private readonly HubUIView _view;
        private readonly IRunDirectorService _runDirector;
        private readonly RunProgressModel _progressModel;
        private readonly ISceneLoaderService _sceneLoader;

        private MapNode _nextNode;

        public HubUIPresenter(
            HubUIView view,
            IRunDirectorService runDirector,
            RunProgressModel progressModel,
            ISceneLoaderService sceneLoader)
        {
            _view = view;
            _runDirector = runDirector;
            _progressModel = progressModel;
            _sceneLoader = sceneLoader;
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
        }

        public void Dispose()
        {
            _view.OnStartBattleClicked -= HandleActionClicked;
            _view.OnMainMenuClicked -= HandleMainMenu;
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
                    
                    _progressModel.CurrentRunDepth++;
                    _sceneLoader.LoadSceneAsync("HubScene").Forget();
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
    }
}