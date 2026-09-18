using System;
using System.Collections.Generic;
using System.Text;
using Zenject;
using Gameplay.UI.Views;
using Gameplay.Towers.Data;
using Gameplay.Towers.Data.Modules;
using Gameplay.Interaction;
using Gameplay.Infrastructure.Signals;

namespace Gameplay.UI.Presenters
{
    public class TowerShopPresenter : IInitializable, IDisposable
    {
        private readonly TowerShopView _view;
        private readonly TowerRegistry _registry;
        private readonly TowerPlacementSystem _placementSystem;
        private readonly TowerButtonView.Pool _buttonPool;
        
        // 1. Объявляем поле для шины
        private readonly SignalBus _signalBus;

        private readonly List<TowerButtonView> _activeButtons = new List<TowerButtonView>();
        private string _currentSelectedId = null;
        

        // 2. ОБЯЗАТЕЛЬНО запрашиваем SignalBus в конструкторе!
        public TowerShopPresenter(
            TowerShopView view, 
            TowerRegistry registry, 
            TowerPlacementSystem placementSystem,
            TowerButtonView.Pool buttonPool,
            SignalBus signalBus) 
        {
            _view = view;
            _registry = registry;
            _placementSystem = placementSystem;
            _buttonPool = buttonPool;
            _signalBus = signalBus; // Сохраняем переданную ссылку
        }

        public void Initialize()
        {
            _view.HideTooltip();
            GenerateButtons();

            _view.OnTowerClicked += HandleTowerClicked;
            _view.OnTowerHoverEntered += HandleTowerHovered;
            _view.OnTowerHoverExited += HandleHoverExited;

            _placementSystem.OnTowerDeselected += HandleDeselectedFromGrid;

            // 3. Теперь _signalBus не null, и мы можем безопасно подписаться!
            _signalBus.Subscribe<SignalPauseStateChanged>(OnPauseStateChanged);
            _signalBus.Subscribe<SignalInteractionModeChanged>(OnModeChanged);
            
        }

        public void Dispose()
        {
            _view.OnTowerClicked -= HandleTowerClicked;
            _view.OnTowerHoverEntered -= HandleTowerHovered;
            _view.OnTowerHoverExited -= HandleHoverExited;
            
            if (_placementSystem != null)
            {
                _placementSystem.OnTowerDeselected -= HandleDeselectedFromGrid;
            }

            // Отписываемся от паузы
            _signalBus.Unsubscribe<SignalPauseStateChanged>(OnPauseStateChanged);
            _signalBus.Unsubscribe<SignalInteractionModeChanged>(OnModeChanged);

            foreach (var button in _activeButtons)
            {
                if (button != null)
                {
                    _buttonPool.Despawn(button);
                }
            }
            _activeButtons.Clear();
        }

        // 4. Метод реакции на паузу
        private void OnPauseStateChanged(SignalPauseStateChanged signal)
        {
            // Блокируем магазин, если игра на паузе
            _view.SetInteractable(!signal.IsPaused);
        }

        private void GenerateButtons()
        {
            foreach (TowerConfig config in _registry.Towers)
            {
                var button = _buttonPool.Spawn(
                    config.TowerId, 
                    config.DisplayName, 
                    config.BaseCost, 
                    config.Icon
                );
                _activeButtons.Add(button);
            }
        }

        private void HandleTowerClicked(string towerId)
        {
            if (_currentSelectedId == towerId)
            {
                _placementSystem.DeselectTower();
                return;
            }

            _currentSelectedId = towerId;
            _placementSystem.SelectTower(towerId);

            UpdateSelectionVisuals();
        }

        private void HandleDeselectedFromGrid()
        {
            _currentSelectedId = null;
            UpdateSelectionVisuals();
        }

        private void UpdateSelectionVisuals()
        {
            foreach (var btn in _activeButtons)
            {
                btn.SetSelected(btn.TowerId == _currentSelectedId);
            }
        }

        private void HandleTowerHovered(string towerId)
        {
            TowerConfig config = _registry.GetTowerById(towerId);
            if (config == null || config.Levels.Count == 0) return;

            TowerLevelData baseLevel = config.Levels[0];
            
            StringBuilder statsBuilder = new StringBuilder();
            foreach (IModuleDescriptor module in baseLevel.GetActiveModules())
            {
                statsBuilder.Append(module.GetStatsDescription());
            }

            _view.ShowTooltip(config.DisplayName, statsBuilder.ToString().Trim());
        }

        private void HandleHoverExited()
        {
            _view.HideTooltip();
        }

        private void OnModeChanged(SignalInteractionModeChanged signal)
        {
            // Магазин скрыт, если идет фаза разметки
            if (signal.Mode == InteractionMode.TacticalClaim) _view.gameObject.SetActive(false);
            else _view.gameObject.SetActive(true);
        }
    }
}