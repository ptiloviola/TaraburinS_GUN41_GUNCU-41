using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Zenject;
using Gameplay.Towers;
using Gameplay.Towers.Data;
using Gameplay.Towers.Data.Modules;
using Gameplay.Economy; // Подключаем экономику!

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

        private TowerSelectionService _selectionService;
        private BankService _bankService; // НОВОЕ: Ссылка на банк
        private TowerInstance _currentTower;

        [Inject]
        public void Construct(TowerSelectionService selectionService, BankService bankService)
        {
            _selectionService = selectionService;
            _bankService = bankService;
        }

        private void Start()
        {
            // Подписываемся на события выделения
            _selectionService.OnTowerSelected += HandleTowerSelected;
            _selectionService.OnTowerDeselected += HandleTowerDeselected;
            
            // Настраиваем кнопки
            _upgradeButton.onClick.AddListener(OnUpgradeClicked);
            _sellButton.onClick.AddListener(OnSellClicked);

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

        private void HandleTowerSelected(TowerInstance tower)
        {
            _currentTower = tower;
            UpdateUI();
            _panelRoot.SetActive(true);
        }

        private void HandleTowerDeselected()
        {
            _currentTower = null;
            _panelRoot.SetActive(false);
        }

        private void UpdateUI()
        {
            if (_currentTower == null) return;

            TowerConfig config = _currentTower.Config;
            TowerLevelData currentLevelData = _currentTower.GetCurrentLevelData();

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
            if (_currentTower.IsMaxLevel())
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

            // Логика кнопки Sell (Допустим, возвращаем 50% от стоимости текущего уровня)
            // В будущем цену можно брать из TowerShopData или считать сумму всех вложенных денег
            int sellValue = currentLevelData.UpgradeCost > 0 ? currentLevelData.UpgradeCost / 2 : 25; 
            _sellPriceText.text = $"Продать\n+{sellValue} $";
        }

        private void OnUpgradeClicked()
        {
            if (_currentTower != null && !_currentTower.IsMaxLevel())
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
                // Заглушка: тут будет начисление денег и удаление башни
                Debug.Log($"<color=red>[UI] Нажата кнопка Продать для {_currentTower.Config.DisplayName}</color>");
                _selectionService.Deselect(); // Снимаем выделение
            }
        }
    }
}