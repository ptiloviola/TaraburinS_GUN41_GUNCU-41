using System;
using UnityEngine;
using Zenject;
using Gameplay.Infrastructure.Signals;
using Gameplay.Core.Data;
using Gameplay.Core.Services;

namespace Gameplay.Levels.Services
{
    public class RunResultProcessor : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly SaveLoadService _saveLoadService;
        private readonly RunProgressModel _runProgress;
        private readonly IRunDirectorService _runDirector; 
        private bool _isProcessed;

        public RunResultProcessor(
            SignalBus signalBus, 
            SaveLoadService saveLoadService, 
            RunProgressModel runProgress,
            IRunDirectorService runDirector)
        {
            _signalBus = signalBus;
            _saveLoadService = saveLoadService;
            _runProgress = runProgress;
            _runDirector = runDirector;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<SignalLevelWon>(OnLevelWon);
            _signalBus.Subscribe<SignalLevelLost>(OnLevelLost);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<SignalLevelWon>(OnLevelWon);
            _signalBus.TryUnsubscribe<SignalLevelLost>(OnLevelLost);
        }

        private void OnLevelWon()
        {
            if (_isProcessed) return;
            _isProcessed = true;

            _runProgress.CurrentRunDepth++;

            PlayerProfileModel profile = _saveLoadService.LoadProfile();

            if (_runProgress.CurrentRunDepth > profile.MaxCompletedLevelIndex)
            {
                profile.MaxCompletedLevelIndex = _runProgress.CurrentRunDepth;
                Debug.Log($"<color=yellow>[RunResultProcessor] Новый рекорд! Глубина: {profile.MaxCompletedLevelIndex}</color>");
            }

            if (!_runDirector.HasNextNode(_runProgress))
            {
                profile.TotalRunsPlayed++;
                Debug.Log("<color=green>[RunResultProcessor] Кампания пройдена! Забег завершен.</color>");
            }

            _saveLoadService.SaveProfile();
        }

        private void OnLevelLost()
        {
            if (_isProcessed) return;
            _isProcessed = true;

            PlayerProfileModel profile = _saveLoadService.LoadProfile();
            profile.TotalRunsPlayed++;

            _saveLoadService.SaveProfile();
        }
    }
}