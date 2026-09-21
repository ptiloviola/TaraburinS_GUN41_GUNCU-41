using System;
using UnityEngine;
using Zenject;
using Gameplay.Infrastructure.Signals;
using Gameplay.Core.Data;
using Gameplay.Core.Services;
using Gameplay.Levels.Data;

namespace Gameplay.Levels.Services
{
    public class RunResultProcessor : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly SaveLoadService _saveLoadService;
        private readonly RunProgressModel _runProgress;
        
        private bool _isProcessed;

        public RunResultProcessor(
            SignalBus signalBus, 
            SaveLoadService saveLoadService, 
            RunProgressModel runProgress)
        {
            _signalBus = signalBus;
            _saveLoadService = saveLoadService;
            _runProgress = runProgress;
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
            profile.TotalRunsPlayed++;


            LevelBlueprintConfig blueprint = _runProgress.CurrentLevelBlueprint;
            if (blueprint != null && blueprint.LevelIndex > profile.MaxCompletedLevelIndex)
            {
                profile.MaxCompletedLevelIndex = blueprint.LevelIndex;
                Debug.Log($"<color=yellow>[RunResultProcessor] Новый рекорд! Открыт уровень {profile.MaxCompletedLevelIndex + 1}</color>");
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