using System;
using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;
using Gameplay.Infrastructure.Services;
using Gameplay.UI.Views;
using Gameplay.Core.Services;
using Gameplay.Core.Data;

namespace Gameplay.UI.Presenters
{
    public class MainMenuPresenter : IInitializable, IDisposable
    {
        private readonly MainMenuView _view;
        private readonly ISceneLoaderService _sceneLoader;
        private readonly SaveLoadService _saveLoadService;
        private readonly RunSaveService _runSaveService;

        private const string StartSceneName = "HubScene"; 

        public MainMenuPresenter(
            MainMenuView view, 
            ISceneLoaderService sceneLoader,
            SaveLoadService saveLoadService,
            RunSaveService runSaveService)
        {
            _view = view;
            _sceneLoader = sceneLoader;
            _saveLoadService = saveLoadService;
            _runSaveService = runSaveService;
        }

        public void Initialize()
        {
            _view.OnPlayClicked += HandlePlayClicked;
            _view.OnLoadSaveClicked += HandleLoadSaveClicked;
            _view.OnSettingsClicked += HandleSettingsClicked;

            PlayerProfileModel profile = _saveLoadService.LoadProfile();
            _view.UpdateStatsDisplay(profile.TotalRunsPlayed, profile.MaxCompletedLevelIndex, profile.MetaCurrency);

            _view.SetInteractable(true);
            
            bool hasRunSave = _runSaveService.HasSave();
            _view.SetLoadButtonInteractable(hasRunSave); 
        }

        public void Dispose()
        {
            _view.OnPlayClicked -= HandlePlayClicked;
            _view.OnLoadSaveClicked -= HandleLoadSaveClicked;
            _view.OnSettingsClicked -= HandleSettingsClicked;
        }

        private void HandlePlayClicked()
        {
            _view.SetInteractable(false);
            
            _runSaveService.DeleteSave();
            
            _sceneLoader.LoadSceneAsync(StartSceneName).Forget();
        }

        private void HandleLoadSaveClicked()
        {
            _view.SetInteractable(false);
            
            Gameplay.Tools.GameLogger.Log("<color=green>[MainMenuPresenter] Загружаем существующий забег...</color>");
            _sceneLoader.LoadSceneAsync(StartSceneName).Forget();
        }

        private void HandleSettingsClicked()
        {
            Gameplay.Tools.GameLogger.Log("<color=yellow>[MainMenuPresenter] ЗАГЛУШКА: Открытие окна настроек...</color>");
        }
    }
}