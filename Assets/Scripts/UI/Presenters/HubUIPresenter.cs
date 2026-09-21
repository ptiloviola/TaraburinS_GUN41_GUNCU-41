using System;
using Zenject;
using Cysharp.Threading.Tasks;
using Gameplay.Core.Services;
using Gameplay.Core.Data;
using Gameplay.Infrastructure.Services;
using Gameplay.UI.Views;
using Gameplay.Levels.Data;

namespace Gameplay.UI.Presenters
{
    public class HubUIPresenter : IInitializable, IDisposable
    {
        private readonly HubUIView _view;
        private readonly IRunDirectorService _runDirector;
        private readonly RunProgressModel _progressModel;
        private readonly ISceneLoaderService _sceneLoader;

        private LevelBlueprintConfig _nextLevel;

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
            _view.OnStartBattleClicked += HandleStartBattle;
            _view.OnMainMenuClicked += HandleMainMenu;

            if (_runDirector.HasNextLevel(_progressModel))
            {
                _nextLevel = _runDirector.GetNextLevel(_progressModel);
                _view.ShowNextLevelInfo(_nextLevel.DisplayName);
            }
            else
            {
                _view.ShowCampaignCompleted();
            }
        }

        public void Dispose()
        {
            _view.OnStartBattleClicked -= HandleStartBattle;
            _view.OnMainMenuClicked -= HandleMainMenu;
        }

        private void HandleStartBattle()
        {
            if (_nextLevel == null) return;
            
            _view.SetInteractable(false);
            
            _progressModel.CurrentLevelBlueprint = _nextLevel;
            
            _sceneLoader.LoadSceneAsync("BattleScene").Forget();
        }

        private void HandleMainMenu()
        {
            _view.SetInteractable(false);
            _sceneLoader.LoadSceneAsync("MainMenuScene").Forget();
        }
    }
}