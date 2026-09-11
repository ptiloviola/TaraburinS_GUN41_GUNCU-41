using System;
using System.Collections.Generic;
using System.Text;
using Zenject;
using Gameplay.UI.Views;
using Gameplay.Towers.Data;
using Gameplay.Towers.Data.Modules;
using Gameplay.Interaction;

namespace Gameplay.UI.Presenters
{
    public class TowerShopPresenter : IInitializable, IDisposable
    {
        private readonly TowerShopView _view;
        private readonly TowerRegistry _registry;
        private readonly TowerPlacementSystem _placementSystem;
        private readonly TowerButtonView.Pool _buttonPool;

        private readonly List<TowerButtonView> _activeButtons = new List<TowerButtonView>();
        private string _currentSelectedId = null;

        public TowerShopPresenter(
            TowerShopView view, 
            TowerRegistry registry, 
            TowerPlacementSystem placementSystem,
            TowerButtonView.Pool buttonPool)
        {
            _view = view;
            _registry = registry;
            _placementSystem = placementSystem;
            _buttonPool = buttonPool;
        }

        public void Initialize()
        {
            _view.HideTooltip();
            GenerateButtons();

            // Подписки на View
            _view.OnTowerClicked += HandleTowerClicked;
            _view.OnTowerHoverEntered += HandleTowerHovered;
            _view.OnTowerHoverExited += HandleHoverExited;

            // Подписка на геймплей
            _placementSystem.OnTowerDeselected += HandleDeselectedFromGrid;
        }

        public void Dispose()
        {
            // Жесткое правило: всегда отписываемся, чтобы избежать утечек памяти
            _view.OnTowerClicked -= HandleTowerClicked;
            _view.OnTowerHoverEntered -= HandleTowerHovered;
            _view.OnTowerHoverExited -= HandleHoverExited;
            
            if (_placementSystem != null)
            {
                _placementSystem.OnTowerDeselected -= HandleDeselectedFromGrid;
            }

            // Возвращаем все кнопки в пул
            foreach (var button in _activeButtons)
            {
                _buttonPool.Despawn(button);
            }
            _activeButtons.Clear();
        }

        private void GenerateButtons()
        {
            foreach (TowerConfig config in _registry.Towers)
            {
                // Больше не передаем _view, Zenject сделает это сам!
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
            
            // Используем StringBuilder вместо '+=' для строк, чтобы не нагружать Garbage Collector
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
    }
}