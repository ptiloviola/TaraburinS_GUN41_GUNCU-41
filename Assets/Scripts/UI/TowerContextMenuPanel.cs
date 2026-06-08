using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Zenject;
using Gameplay.Towers;
using Gameplay.Towers.Data;
using Gameplay.Towers.Data.Modules;
using Gameplay.Economy; // Подключаем экономику!
using Gameplay.Towers.Visuals; // Добавили пространство имен визуала
using Gameplay.Grid;

namespace Gameplay.UI
{
    public class TowerContextMenuPanel : MonoBehaviour
    {
        [Header("UI Элементы")]
        [SerializeField] private GameObject _panelRoot; // Вся панель целиком (чтобы скрывать)
        [SerializeField] private TMP_Text _towerNameText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _statsText;
        
        [Header("Кнопки")]
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private TMP_Text _upgradeCostText;
        [SerializeField] private Button _sellButton;
        [SerializeField] private TMP_Text _sellPriceText;
        [SerializeField] private UIHoverListener _upgradeButtonHoverListener; // НОВОЕ

        private TowerSelectionService _selectionService;
        private BankService _bankService; // НОВОЕ: Ссылка на банк
        private IGridService _gridService; // НОВОЕ: Ссылка на сетку
        private TowerRegistry _towerRegistry; // НОВОЕ: Доступ к каталогу магазина
        private TowerFacade _currentTower;
        private int _currentCalculatedSellValue; // Кешируем сумму продажи, чтобы не считать дважды

        [Inject]
        public void Construct(TowerSelectionService selectionService, BankService bankService, IGridService gridService, TowerRegistry towerRegistry)
        {
            _selectionService = selectionService;
            _bankService = bankService;
            _gridService = gridService;
            _towerRegistry = towerRegistry;
        }

        private void Start()
        {
            // Подписываемся на события выделения
            _selectionService.OnTowerSelected += HandleTowerSelected;
            _selectionService.OnTowerDeselected += HandleTowerDeselected;
            
            // Настраиваем кнопки
            _upgradeButton.onClick.AddListener(OnUpgradeClicked);
            _sellButton.onClick.AddListener(OnSellClicked);

            // НОВОЕ: Слушаем наведение мышки на кнопку
            _upgradeButtonHoverListener.OnHoverEnter += HandleUpgradeHoverEnter;
            _upgradeButtonHoverListener.OnHoverExit += HandleUpgradeHoverExit;

            // Прячем меню при старте
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
            // Если у нас уже есть радиус при простом выделении башни, показываем его
            ShowCurrentRadiusOnly();
        }

        private void HandleTowerDeselected()
        {
            if (_currentTower != null)
            {
                // Прячем радиусы при снятии выделения
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

            // Собираем статы через наш элегантный IModuleDescriptor!
            string stats = "";
            foreach (IModuleDescriptor module in currentLevelData.GetActiveModules())
            {
                stats += module.GetStatsDescription();
            }
            _statsText.text = stats.Trim();

            // Логика кнопки Upgrade
            if (!_currentTower.CanUpgrade())
            {
                _upgradeButton.interactable = false;
                _upgradeCostText.text = "MAX";
            }
            else
            {
                _upgradeButton.interactable = true;
                // Берем цену СЛЕДУЮЩЕГО уровня
                int cost = config.Levels[_currentTower.CurrentLevel + 1].UpgradeCost;
                _upgradeCostText.text = $"Улучшить\n{cost} $";
            }

            // Логика кнопки Sell
            // НОВОЕ: Честный калькулятор стоимости продажи
            CalculateSellValue();
            _sellPriceText.text = $"Продать\n+{_currentCalculatedSellValue} $";
        }

        private void CalculateSellValue()
        {
            _currentCalculatedSellValue = 0;
            
            // 1. Ищем данные башни в магазине по её ID
            TowerShopData shopData = _towerRegistry.GetTowerByConfig(_currentTower.Config);
            
            if (shopData != null)
            {
                // 2. Стартовая инвестиция (базовая цена постройки)
                int totalInvested = shopData.Cost;

                // 3. Добавляем стоимость всех купленных апгрейдов (от 1-го уровня до текущего)
                // Начинаем с i=1, так как Levels[0] дается бесплатно при постройке
                for (int i = 1; i <= _currentTower.CurrentLevel; i++)
                {
                    totalInvested += _currentTower.Config.Levels[i].UpgradeCost;
                }

                // 4. Применяем множитель возврата (например, 50%) и округляем
                _currentCalculatedSellValue = Mathf.RoundToInt(totalInvested * shopData.SellRefundMultiplier);
            }
            else
            {
                Debug.LogWarning($"[UI] Не удалось найти башню {_currentTower.Config.TowerId} в магазине для расчета цены!");
            }
        }




        private void ShowCurrentRadiusOnly()
        {
            Debug.LogWarning("<color=red>[UI] ShowCurrentRadiusOnly</color>");
            if (_currentTower != null && _currentTower.TryGetComponent(out TowerRadiusVisualizer visualizer))
            {
                float currentRadius = GetRadiusFromLevel(_currentTower.GetCurrentStats());
                visualizer.ShowPreview(currentRadius, 0f); // Будущий радиус = 0 (не показываем)
            }
        }

        // НОВОЕ: Игрок навел мышку на кнопку "Улучшить"
        private void HandleUpgradeHoverEnter()
        {
            Debug.LogWarning("<color=red>[UI] Игрок навел мышку на кнопку Улучшить</color>");
            if (_currentTower == null || !_currentTower.CanUpgrade()) return; // У нас теперь CanUpgrade() в фасаде, используй его!

            if (_currentTower.TryGetComponent(out TowerRadiusVisualizer visualizer))
            {
                float currentRadius = GetRadiusFromLevel(_currentTower.GetCurrentStats());
                
                // Берем статы следующего уровня
                TowerLevelData nextLevel = _currentTower.Config.Levels[_currentTower.CurrentLevel + 1];
                float nextRadius = GetRadiusFromLevel(nextLevel);
                
                visualizer.ShowPreview(currentRadius, nextRadius);
            }
        }

        // НОВОЕ: Игрок убрал мышку с кнопки "Улучшить"
        private void HandleUpgradeHoverExit()
        {
            // Возвращаем показ только текущего радиуса
            ShowCurrentRadiusOnly();
        }

        // Вспомогательный метод (как мы делали в GridInteractor)
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
                // 1. Узнаем, сколько стоит СЛЕДУЮЩИЙ уровень
               int cost = _currentTower.Config.Levels[_currentTower.CurrentLevel + 1].UpgradeCost;
                // 2. Пытаемся списать деньги
                if (_bankService.SpendMoney(cost))
                {
                    Debug.Log($"<color=green>[UI] Успешная покупка апгрейда за {cost}$</color>");
                    // 3. Если деньги списались - прокачиваем башню!
                    _currentTower.Upgrade();
                    // 4. Обновляем текст в UI (чтобы показать новые статы и цену следующего уровня)
                    UpdateUI();
                }
                else
                {
                    // Денег не хватило
                    Debug.LogWarning("<color=red>[UI] Недостаточно золота для апгрейда!</color>");
                    // В будущем здесь можно добавить визуальный фидбек, например, покрасить текст цены в красный на полсекунды
                }
                
            }
        }

        private void OnSellClicked()
        {
            if (_currentTower != null)
            {
                // 1. Начисляем деньги в банк (используем уже посчитанную сумму)
                _bankService.AddMoney(_currentCalculatedSellValue);
                
                // 2. Освобождаем клетку сетки (делаем ее снова доступной для застройки)
                GridNode node = _gridService.GetNode(_currentTower.GridPosition);
                if (node != null)
                {
                    node.IsOccupied = false;
                }

                // 3. Запоминаем саму "тушку" башни
                GameObject towerObject = _currentTower.gameObject;
                Debug.Log($"<color=red>[UI] Башня {_currentTower.Config.DisplayName} продана за {_currentCalculatedSellValue}$. Клетка {_currentTower.GridPosition} освобождена.</color>");

                // 5. Сбрасываем выделение (это автоматически скроет UI и выключит цилиндры радиусов)
                _selectionService.Deselect();

                // 6. Уничтожаем башню
                Destroy(towerObject);

                

            }
        }
    }
}