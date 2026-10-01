using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Gameplay.Infrastructure.Signals;
using Gameplay.UI.Views;
using Gameplay.Enemies.Data;
using Gameplay.Interaction;
using Gameplay.Infrastructure.Services;



namespace Gameplay.UI.Presenters
{
    public class WaveUIPresenter : IInitializable, IDisposable
    {
        private readonly WaveUIView _view;
        private readonly SignalBus _signalBus;
        private readonly EnemyRegistry _registry;
        private readonly ForecastIconView.Pool _iconPool;
        
        private readonly List<ForecastIconView> _activeIcons = new List<ForecastIconView>();

        private readonly ITimeScaleService _timeScaleService;

        public WaveUIPresenter(
            WaveUIView view, 
            SignalBus signalBus, 
            EnemyRegistry registry, 
            ForecastIconView.Pool iconPool,
            ITimeScaleService timeScaleService)
        {
            _view = view;
            _signalBus = signalBus;
            _registry = registry;
            _iconPool = iconPool;
            _timeScaleService = timeScaleService;
        }

        public void Initialize()
        {
            _view.OnForceStartClicked += HandleForceStartClicked;

            _signalBus.Subscribe<SignalWaveTimerUpdated>(OnTimerUpdated);
            _signalBus.Subscribe<SignalWaveStateChanged>(OnWaveStateChanged);
            _signalBus.Subscribe<SignalWaveForecastUpdated>(OnForecastUpdated);

            _signalBus.Subscribe<SignalPauseStateChanged>(OnPauseStateChanged);
            _signalBus.Subscribe<SignalInteractionModeChanged>(OnModeChanged);

            _view.OnSpeedClicked += HandleSpeedClicked;
            _signalBus.Subscribe<SignalTimeScaleChanged>(OnTimeScaleChanged);
            _view.UpdateSpeedText($"x{_timeScaleService.CurrentScale}");
        }

        public void Dispose()
        {
            _view.OnForceStartClicked -= HandleForceStartClicked;

            _signalBus.Unsubscribe<SignalWaveTimerUpdated>(OnTimerUpdated);
            _signalBus.Unsubscribe<SignalWaveStateChanged>(OnWaveStateChanged);
            _signalBus.Unsubscribe<SignalWaveForecastUpdated>(OnForecastUpdated);

            _signalBus.Unsubscribe<SignalPauseStateChanged>(OnPauseStateChanged);
            _signalBus.Unsubscribe<SignalInteractionModeChanged>(OnModeChanged);

            _view.OnSpeedClicked -= HandleSpeedClicked;
            _signalBus.Unsubscribe<SignalTimeScaleChanged>(OnTimeScaleChanged);
            
            ClearIcons();
        }

        private void HandleForceStartClicked()
        {
            _signalBus.Fire<SignalForceStartWave>();
        }

        private void OnTimerUpdated(SignalWaveTimerUpdated signal)
        {
            if (signal.TimeLeft > 0)
            {
                string timeText = Mathf.CeilToInt(signal.TimeLeft).ToString();
                _view.UpdateTimer(signal.Progress, timeText, true);
            }
            else
            {
                _view.UpdateTimer(signal.Progress, "Attack", false);
            }
        }

        private void OnWaveStateChanged(SignalWaveStateChanged signal)
        {
            _view.UpdateWaveNumber($"Волна {signal.CurrentWave}/{signal.TotalWaves}");
        }

        private void OnForecastUpdated(SignalWaveForecastUpdated signal)
        {
            ClearIcons();

            foreach (var kvp in signal.EnemyCounts)
            {
                EnemyConfig config = _registry.GetEnemyById(kvp.Key);
                Sprite iconSprite = config != null ? config.UIIcon : null;

                var icon = _iconPool.Spawn(kvp.Key, kvp.Value, iconSprite);
                _activeIcons.Add(icon);
            }
        }
        
        private void ClearIcons()
        {
            foreach (var icon in _activeIcons)
            {
                if (icon != null) 
                {
                    _iconPool.Despawn(icon);
                }
            }
            _activeIcons.Clear();
        }

        private void OnPauseStateChanged(SignalPauseStateChanged signal)
        {
            _view.SetInteractable(!signal.IsPaused);
        }

        private void OnModeChanged(SignalInteractionModeChanged signal)
        {
            if (signal.Mode == InteractionMode.TacticalClaim)
            {
                _view.gameObject.SetActive(false); 
            }
            else
            {
                _view.gameObject.SetActive(true); 
            }
        }

        private void HandleSpeedClicked()
        {
            _timeScaleService.CycleSpeed();
        }

        private void OnTimeScaleChanged(SignalTimeScaleChanged signal)
        {
            _view.UpdateSpeedText($"x{signal.TimeScale}");
        }
    }
}