using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Zenject;
using Gameplay.Towers;
using Gameplay.Towers.Data;
using Gameplay.Towers.Data.Modules;
using Gameplay.Economy; 
using Gameplay.Towers.Visuals; 
using Gameplay.Grid;
using Gameplay.Interaction;

namespace Gameplay.UI
{
    public class TowerContextMenuPanel : MonoBehaviour
    {
        [Header("UI Элементы")]
        [SerializeField] private GameObject _panelRoot; 
        [SerializeField] private TMP_Text _towerNameText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _statsText;
        
        [Header("Кнопки")]
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private TMP_Text _upgradeCostText;
        [SerializeField] private Button _sellButton;
        [SerializeField] private TMP_Text _sellPriceText;
        [SerializeField] private UIHoverListener _upgradeButtonHoverListener; 

        private TowerSelectionService _selectionService;
        private BankService _bankService; 
        private IGridService _gridService; 
        
        // ИЗМЕНЕНО: Мы удалили TowerRegistry, он здесь больше не нужен!
        private TowerFacade _currentTower;
        private int _currentCalculatedSellValue; 

        [Inject]
        public void Construct(TowerSelectionService selectionService, BankService bankService, IGridService gridService)
        {
            _selectionService = selectionService;
            _bankService = bankService;
            _gridService = gridService;
        }

        private void Start()
        {
            _selectionService.OnTowerSelected += HandleTowerSelected;
            _selectionService.OnTowerDeselected += HandleTowerDeselected;
            
            _upgradeButton.onClick.AddListener(OnUpgradeClicked);
            _sellButton.onClick.AddListener(OnSellClicked);

            _upgradeButtonHoverListener.OnHoverEnter += HandleUpgradeHoverEnter;
            _upgradeButtonHoverListener.OnHoverExit += HandleUpgradeHoverExit;

            _panelRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_selectionService != null)
            {
                _selectionService.OnTowerSelected -= HandleTowerSelected;
                _selectionService.OnTowerDeselected -= HandleTowerDeselected;
            }
        }

        private void HandleTowerSelected(TowerFacade tower)
        {
            _currentTower = tower;
            UpdateUI();
            _panelRoot.SetActive(true);
            ShowCurrentRadiusOnly();
        }

        private void HandleTowerDeselected()
        {
            if (_currentTower != null)
            {
                if (_currentTower.TryGetComponent(out TowerRadiusVisualizer visualizer))
                {
                    visualizer.HidePreview();
                }
            }
            _currentTower = null;
            _panelRoot.SetActive(false);
        }

        private void UpdateUI()
        {
            if (_currentTower == null) return;

            TowerConfig config = _currentTower.Config;
            TowerLevelData currentLevelData = _currentTower.GetCurrentStats();

            _towerNameText.text = config.DisplayName;
            _levelText.text = $"Уровень {_currentTower.CurrentLevel + 1} / {config.MaxLevel + 1}";

            string stats = "";
            foreach (IModuleDescriptor module in currentLevelData.GetActiveModules())
            {
                stats += module.GetStatsDescription();
            }
            _statsText.text = stats.Trim();

            if (!_currentTower.CanUpgrade())
            {
                _upgradeButton.interactable = false;
                _upgradeCostText.text = "MAX";
            }
            else
            {
                _upgradeButton.interactable = true;
                int cost = config.Levels[_currentTower.CurrentLevel + 1].UpgradeCost;
                _upgradeCostText.text = $"Улучшить\n{cost} $";
            }

            CalculateSellValue();
            _sellPriceText.text = $"Продать\n+{_currentCalculatedSellValue} $";
        }

        // ИЗМЕНЕНО: Честный калькулятор продажи теперь берет данные напрямую из башни
        private void CalculateSellValue()
        {
            _currentCalculatedSellValue = 0;
            
            if (_currentTower != null && _currentTower.Config != null)
            {
                TowerConfig config = _currentTower.Config;

                // 1. Стартовая инвестиция (базовая цена постройки)
                int totalInvested = config.BaseCost;

                // 2. Добавляем стоимость всех купленных апгрейдов
                for (int i = 1; i <= _currentTower.CurrentLevel; i++)
                {
                    totalInvested += config.Levels[i].UpgradeCost;
                }

                // 3. Применяем множитель возврата и округляем
                _currentCalculatedSellValue = Mathf.RoundToInt(totalInvested * config.SellRefundMultiplier);
            }
        }

        private void ShowCurrentRadiusOnly()
        {
            if (_currentTower != null && _currentTower.TryGetComponent(out TowerRadiusVisualizer visualizer))
            {
                float currentRadius = GetRadiusFromLevel(_currentTower.GetCurrentStats());
                visualizer.ShowPreview(currentRadius, 0f); 
            }
        }

        private void HandleUpgradeHoverEnter()
        {
            if (_currentTower == null || !_currentTower.CanUpgrade()) return; 

            if (_currentTower.TryGetComponent(out TowerRadiusVisualizer visualizer))
            {
                float currentRadius = GetRadiusFromLevel(_currentTower.GetCurrentStats());
                
                TowerLevelData nextLevel = _currentTower.Config.Levels[_currentTower.CurrentLevel + 1];
                float nextRadius = GetRadiusFromLevel(nextLevel);
                
                visualizer.ShowPreview(currentRadius, nextRadius);
            }
        }

        private void HandleUpgradeHoverExit()
        {
            ShowCurrentRadiusOnly();
        }

        private float GetRadiusFromLevel(TowerLevelData levelData)
        {
            if (levelData.Attack != null && levelData.Attack.Range > 0) return levelData.Attack.Range;
            if (levelData.Aura != null && levelData.Aura.Radius > 0) return levelData.Aura.Radius;
            return 0f;
        }

        private void OnUpgradeClicked()
        {
            if (_currentTower != null && _currentTower.CanUpgrade())
            {
               int cost = _currentTower.Config.Levels[_currentTower.CurrentLevel + 1].UpgradeCost;
                if (_bankService.SpendMoney(cost))
                {
                    _currentTower.Upgrade();
                    UpdateUI();
                }
            }
        }

        private void OnSellClicked()
        {
            if (_currentTower != null)
            {
                _bankService.AddMoney(_currentCalculatedSellValue);
                
                GridNode node = _gridService.GetNode(_currentTower.GridPosition);
                if (node != null)
                {
                    node.IsOccupied = false;
                }

                GameObject towerObject = _currentTower.gameObject;
                _selectionService.Deselect();
                Destroy(towerObject);
            }
        }
    }
}