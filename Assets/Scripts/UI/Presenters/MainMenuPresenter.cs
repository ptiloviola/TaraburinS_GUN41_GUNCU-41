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
        private readonly RunProgressModel _runProgress;

        
        private const string StartSceneName = "HubScene"; 

        public MainMenuPresenter(
            MainMenuView view, 
            ISceneLoaderService sceneLoader,
            SaveLoadService saveLoadService,
            RunProgressModel runProgress)
        {
            _view = view;
            _sceneLoader = sceneLoader;
            _saveLoadService = saveLoadService;
            _runProgress = runProgress;
        }

        public void Initialize()
        {
            _view.OnPlayClicked += HandlePlayClicked;
            _view.OnLoadSaveClicked += HandleLoadSaveClicked;
            _view.OnSettingsClicked += HandleSettingsClicked;

            PlayerProfileModel profile = _saveLoadService.LoadProfile();
            
            _view.UpdateStatsDisplay(profile.TotalRunsPlayed, profile.MaxCompletedLevelIndex, profile.MetaCurrency);
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
            

            _runProgress.ResetRun();
            
            _sceneLoader.LoadSceneAsync(StartSceneName).Forget();
        }

        private void HandleLoadSaveClicked()
        {
            Debug.Log("<color=yellow>[MainMenuPresenter] ЗАГЛУШКА: Открытие окна загрузки сохранений...</color>");
        }

        private void HandleSettingsClicked()
        {
            Debug.Log("<color=yellow>[MainMenuPresenter] ЗАГЛУШКА: Открытие окна настроек...</color>");
        }
    }
}