using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Gameplay.Infrastructure.Signals;
using Gameplay.UI.Views;
using Gameplay.Enemies.Data;
using Gameplay.Interaction;



namespace Gameplay.UI.Presenters
{
    public class WaveUIPresenter : IInitializable, IDisposable
    {
        private readonly WaveUIView _view;
        private readonly SignalBus _signalBus;
        private readonly EnemyRegistry _registry;
        private readonly ForecastIconView.Pool _iconPool;
        
        // Храним активные иконки, чтобы вернуть их в пул перед новой волной
        private readonly List<ForecastIconView> _activeIcons = new List<ForecastIconView>();

        public WaveUIPresenter(
            WaveUIView view, 
            SignalBus signalBus, 
            EnemyRegistry registry, 
            ForecastIconView.Pool iconPool)
        {
            _view = view;
            _signalBus = signalBus;
            _registry = registry;
            _iconPool = iconPool;
        }

        public void Initialize()
        {
            _view.OnForceStartClicked += HandleForceStartClicked;

            _signalBus.Subscribe<SignalWaveTimerUpdated>(OnTimerUpdated);
            _signalBus.Subscribe<SignalWaveStateChanged>(OnWaveStateChanged);
            _signalBus.Subscribe<SignalWaveForecastUpdated>(OnForecastUpdated);

            _signalBus.Subscribe<SignalPauseStateChanged>(OnPauseStateChanged);
            _signalBus.Subscribe<SignalInteractionModeChanged>(OnModeChanged);
        }

        public void Dispose()
        {
            _view.OnForceStartClicked -= HandleForceStartClicked;

            _signalBus.Unsubscribe<SignalWaveTimerUpdated>(OnTimerUpdated);
            _signalBus.Unsubscribe<SignalWaveStateChanged>(OnWaveStateChanged);
            _signalBus.Unsubscribe<SignalWaveForecastUpdated>(OnForecastUpdated);

            _signalBus.Unsubscribe<SignalPauseStateChanged>(OnPauseStateChanged);
            _signalBus.Unsubscribe<SignalInteractionModeChanged>(OnModeChanged);
            
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

                // Запрашиваем готовую иконку из пула
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
                // Прячем UI волн в тактической фазе
                _view.gameObject.SetActive(false); 
            }
            else
            {
                // Показываем обратно в бою
                _view.gameObject.SetActive(true); 
            }
        }
    }
}