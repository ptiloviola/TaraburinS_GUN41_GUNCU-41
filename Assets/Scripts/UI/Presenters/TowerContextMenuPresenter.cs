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

namespace Gameplay.UI.Presenters
{
    public class TowerContextMenuPresenter : IInitializable, IDisposable
    {
        private readonly TowerContextMenuView _view;
        private readonly TowerSelectionService _selectionService;
        private readonly BankService _bankService;
        private readonly TowerLifecycleService _lifecycleService;

        private TowerFacade _currentTower;

        public TowerContextMenuPresenter(
            TowerContextMenuView view,
            TowerSelectionService selectionService,
            BankService bankService,
            TowerLifecycleService lifecycleService)
        {
            _view = view;
            _selectionService = selectionService;
            _bankService = bankService;
            _lifecycleService = lifecycleService;
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
        }

        private void HandleTowerSelected(TowerFacade tower)
        {
            _currentTower = tower;
            UpdateViewData();
            _view.ShowPanel();
            
            // Чистая команда фасаду!
            _currentTower.ShowRadiusPreview();
        }

        private void HandleTowerDeselected()
        {
            if (_currentTower != null)
            {
                // Чистая команда фасаду!
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

            int cost = TowerEconomyCalculator.GetUpgradeCost(_currentTower.Config, _currentTower.CurrentLevel);
            if (_bankService.SpendMoney(cost))
            {
                _currentTower.Upgrade();
                UpdateViewData();
                
                // Обновляем радиус на земле после апгрейда
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
            
            // Чистая команда фасаду!
            _currentTower.ShowUpgradePreview();
        }

        private void HandleUpgradeHoverExit()
        {
            if (_currentTower == null) return;
            
            // Возвращаем обычный радиус
            _currentTower.ShowRadiusPreview();
        }
    }
}