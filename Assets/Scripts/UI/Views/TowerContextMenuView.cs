using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

namespace Gameplay.UI.Views
{
    public class TowerContextMenuView : MonoBehaviour
    {
        [Header("Тексты информации")]
        [SerializeField] private TMP_Text _towerNameText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _statsText;

        [Header("Кнопки")]
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private TMP_Text _upgradeCostText;
        [SerializeField] private Button _sellButton;
        [SerializeField] private TMP_Text _sellPriceText;
        
        [Header("Взаимодействие")]
        [SerializeField] private UIHoverListener _upgradeButtonHoverListener;

        public event Action OnUpgradeClicked;
        public event Action OnSellClicked;
        public event Action OnUpgradeHoverEntered;
        public event Action OnUpgradeHoverExited;

        private void Awake()
        {
            _upgradeButton.onClick.AddListener(() => OnUpgradeClicked?.Invoke());
            _sellButton.onClick.AddListener(() => OnSellClicked?.Invoke());

            if (_upgradeButtonHoverListener != null)
            {
                _upgradeButtonHoverListener.OnHoverEnter += () => OnUpgradeHoverEntered?.Invoke();
                _upgradeButtonHoverListener.OnHoverExit += () => OnUpgradeHoverExited?.Invoke();
            }
        }

        public void ShowPanel() => gameObject.SetActive(true);
        public void HidePanel() => gameObject.SetActive(false);

        public void UpdateInfo(string name, string level, string stats, string sellPrice)
        {
            _towerNameText.text = name;
            _levelText.text = level;
            _statsText.text = stats;
            _sellPriceText.text = sellPrice;
        }

        public void SetUpgradeState(bool canUpgrade, string costText)
        {
            _upgradeButton.interactable = canUpgrade;
            _upgradeCostText.text = costText;
        }

        private void OnDestroy()
        {
            _upgradeButton.onClick.RemoveAllListeners();
            _sellButton.onClick.RemoveAllListeners();
        }
    }
}