using System;
using UnityEngine;
using Zenject;
using Gameplay.Infrastructure.Signals;
using Gameplay.Campaign.Data;
using Gameplay.Core.Services;
using Gameplay.Levels.Data;
using Gameplay.Core.Data;


namespace Gameplay.Campaign.Services
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

            LevelBlueprintConfig wonLevel = _runProgress.CurrentNode?.CombatLevel;
            PlayerProfileModel profile = _saveLoadService.LoadProfile();

            if (wonLevel != null)
            {
                _runProgress.CurrentRunGold += wonLevel.RunCurrencyReward;
                profile.MetaCurrency += wonLevel.MetaCurrencyReward;
                
                Debug.Log($"<color=yellow>[RunResultProcessor] Награда: +{wonLevel.RunCurrencyReward} Золота забега, +{wonLevel.MetaCurrencyReward} Мета-очков.</color>");
            }

            _runProgress.CurrentRunDepth++;

            if (_runProgress.CurrentRunDepth > profile.MaxCompletedLevelIndex)
            {
                profile.MaxCompletedLevelIndex = _runProgress.CurrentRunDepth;
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