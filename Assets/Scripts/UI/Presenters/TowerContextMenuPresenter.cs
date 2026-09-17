using System;
using System.Text;
using Zenject;
using Gameplay.UI.Views;
using Gameplay.Towers;
using Gameplay.Towers.Data;
using Gameplay.Towers.Data.Modules;
using Gameplay.Economy;
using Gameplay.Interaction;
using Gameplay.Towers.Economy;
using Gameplay.Towers.Services;
using Gameplay.Infrastructure.Signals;

namespace Gameplay.UI.Presenters
{
    public class TowerContextMenuPresenter : IInitializable, IDisposable
    {
        private readonly TowerContextMenuView _view;
        private readonly TowerSelectionService _selectionService;
        private readonly TowerLifecycleService _lifecycleService;
        private readonly TowerUpgradeService _upgradeService;
        private readonly SignalBus _signalBus;

        private TowerFacade _currentTower;

        public TowerContextMenuPresenter(
            TowerContextMenuView view,
            TowerSelectionService selectionService,
            TowerLifecycleService lifecycleService,
            TowerUpgradeService upgradeService,
            SignalBus signalBus)
        {
            _view = view;
            _selectionService = selectionService;
            _lifecycleService = lifecycleService;
            _upgradeService = upgradeService;
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _view.HidePanel();

            _selectionService.OnTowerSelected += HandleTowerSelected;
            _selectionService.OnTowerDeselected += HandleTowerDeselected;

            _view.OnUpgradeClicked += HandleUpgradeClicked;
            _view.OnSellClicked += HandleSellClicked;
            _view.OnUpgradeHoverEntered += HandleUpgradeHoverEnter;
            _view.OnUpgradeHoverExited += HandleUpgradeHoverExit;
            

            _upgradeService.OnTowerUpgraded += HandleTowerUpgraded;

            _signalBus.Subscribe<SignalPauseStateChanged>(OnPauseStateChanged);
        }

        public void Dispose()
        {
            if (_selectionService != null)
            {
                _selectionService.OnTowerSelected -= HandleTowerSelected;
                _selectionService.OnTowerDeselected -= HandleTowerDeselected;
            }

            _view.OnUpgradeClicked -= HandleUpgradeClicked;
            _view.OnSellClicked -= HandleSellClicked;
            _view.OnUpgradeHoverEntered -= HandleUpgradeHoverEnter;
            _view.OnUpgradeHoverExited -= HandleUpgradeHoverExit;
            
            if (_upgradeService != null)
            {
                _upgradeService.OnTowerUpgraded -= HandleTowerUpgraded;
            }

            _signalBus.Unsubscribe<SignalPauseStateChanged>(OnPauseStateChanged);
        }

        private void HandleTowerSelected(TowerFacade tower)
        {
            _currentTower = tower;
            UpdateViewData();
            _view.ShowPanel();
            
            _currentTower.ShowRadiusPreview();
        }

        private void HandleTowerDeselected()
        {
            if (_currentTower != null)
            {
                _currentTower.HideRadiusPreview();
            }
            
            _currentTower = null;
            _view.HidePanel();
        }

        private void UpdateViewData()
        {
            if (_currentTower == null) return;

            TowerConfig config = _currentTower.Config;
            int currentLevel = _currentTower.CurrentLevel;
            TowerLevelData currentLevelData = _currentTower.GetCurrentStats();

            string levelText = $"Уровень {currentLevel + 1} / {config.MaxLevel + 1}";
            
            StringBuilder statsBuilder = new StringBuilder();
            foreach (IModuleDescriptor module in currentLevelData.GetActiveModules())
            {
                statsBuilder.Append(module.GetStatsDescription());
            }

            int sellValue = TowerEconomyCalculator.CalculateSellValue(config, currentLevel);
            string sellPriceText = $"Продать\n+{sellValue} $";

            _view.UpdateInfo(config.DisplayName, levelText, statsBuilder.ToString().Trim(), sellPriceText);

            if (!_currentTower.CanUpgrade())
            {
                _view.SetUpgradeState(false, "MAX");
            }
            else
            {
                int cost = TowerEconomyCalculator.GetUpgradeCost(config, currentLevel);
                _view.SetUpgradeState(true, $"Улучшить\n{cost} $");
            }
        }

        private void HandleUpgradeClicked()
        {
            if (_currentTower == null || !_currentTower.CanUpgrade()) return;

            _upgradeService.TryUpgradeTower(_currentTower); 
        }

        private void HandleTowerUpgraded(TowerFacade oldTower, TowerFacade newTower)
        {
            if (_currentTower == oldTower)
            {
                _currentTower = newTower;
                
                _selectionService.ForceSelect(newTower); 
                
                UpdateViewData();
                _currentTower.ShowRadiusPreview();
            }
        }

        private void HandleSellClicked()
        {
            if (_currentTower == null) return;

            _lifecycleService.SellTower(_currentTower);
            _selectionService.Deselect();
        }

        private void HandleUpgradeHoverEnter()
        {
            if (_currentTower == null || !_currentTower.CanUpgrade()) return;
            _currentTower.ShowUpgradePreview();
        }

        private void HandleUpgradeHoverExit()
        {
            if (_currentTower == null) return;
            _currentTower.ShowRadiusPreview();
        }

        private void OnPauseStateChanged(SignalPauseStateChanged signal)
        {
            _view.SetInteractable(!signal.IsPaused);
        }
    }
}