using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Gameplay.Infrastructure.Signals;
using Gameplay.UI.Views;
using Gameplay.Interaction;
using Gameplay.Spawning;
using Gameplay.Spawning.Data;
using Gameplay.Enemies.Data;
using Gameplay.Levels.Services;

namespace Gameplay.UI.Presenters
{
    public class TacticalUIPresenter : IInitializable, IDisposable
    {
        private readonly TacticalUIView _view;
        private readonly SignalBus _signalBus;
        
        private readonly IWaveProvider _waveProvider;
        private readonly EnemyRegistry _enemyRegistry;
        private readonly ForecastIconView.Pool _iconPool;
        private readonly TacticalForecastService _forecastService;

        private readonly List<ForecastIconView> _activeIcons = new List<ForecastIconView>();

        public TacticalUIPresenter(
            TacticalUIView view, 
            SignalBus signalBus,
            TacticalForecastService forecastService,
            EnemyRegistry enemyRegistry,
            ForecastIconView.Pool iconPool)
        {
            _view = view;
            _signalBus = signalBus;
            _forecastService = forecastService;
            _enemyRegistry = enemyRegistry;
            _iconPool = iconPool;
        }

        public void Initialize()
        {
            _view.OnStartCombatClicked += HandleStartCombat;
            _signalBus.Subscribe<SignalInteractionModeChanged>(OnModeChanged);
            _signalBus.Subscribe<SignalTacticalClaimsUpdated>(OnClaimsUpdated);

            _view.Hide(); 
        }

        public void Dispose()
        {
            _view.OnStartCombatClicked -= HandleStartCombat;
            _signalBus.Unsubscribe<SignalInteractionModeChanged>(OnModeChanged);
            _signalBus.Unsubscribe<SignalTacticalClaimsUpdated>(OnClaimsUpdated);
            
            ClearIcons();
        }

        private void HandleStartCombat()
        {
            _signalBus.Fire<SignalStartCombat>();
        }

        private void OnModeChanged(SignalInteractionModeChanged signal)
        {
            if (signal.Mode == InteractionMode.TacticalClaim)
            {
                _view.Show();
                ShowForecast();
            }
            else
            {
                _view.Hide();
                ClearIcons();
            }
        }

        private void OnClaimsUpdated(SignalTacticalClaimsUpdated signal)
        {
            _view.UpdateClaimsText(signal.Available, signal.Max);
        }

        private void ShowForecast()
        {
            ClearIcons();

            var forecastData = _forecastService.GetLevelForecast();

            foreach (var data in forecastData)
            {
                string displayName = null;
                Sprite iconSprite = null;
                int displayCount = data.IsCountHidden ? -1 : data.TotalCount;

                if (!data.IsTypeHidden)
                {
                    EnemyConfig config = _enemyRegistry.GetEnemyById(data.EnemyId);
                    if (config != null)
                    {
                        displayName = config.DisplayName;
                        iconSprite = config.UIIcon;
                    }
                }

                var icon = _iconPool.Spawn(displayName, displayCount, iconSprite);
                icon.transform.SetParent(_view.ForecastContainer, false);
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
    }
}