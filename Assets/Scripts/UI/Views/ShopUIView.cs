using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gameplay.Campaign.Data;
using Gameplay.Modifiers.Data;

namespace Gameplay.UI.Views
{
    public class ShopUIView : MonoBehaviour
    {
        [SerializeField] private GameObject _panelObject; 
        [SerializeField] private Transform _itemsContainer; 
        [SerializeField] private ShopItemView _itemPrefab; 
        [SerializeField] private Button _leaveButton;
        [SerializeField] private TextMeshProUGUI _shopGoldText; 

        public event Action<ItemConfig, ShopItemView> OnItemBuyClicked;
        public event Action OnLeaveClicked;

        private void Awake()
        {
            _leaveButton.onClick.AddListener(() => OnLeaveClicked?.Invoke());
        }

        public void ShowShop(ShopConfig config, int playerGold)
        {
            _panelObject.SetActive(true);
            UpdateGoldUI(playerGold);

            foreach (Transform child in _itemsContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (ItemConfig item in config.AvailableItems)
            {
                ShopItemView itemView = Instantiate(_itemPrefab, _itemsContainer);
                itemView.Setup(item, playerGold, HandleItemBought);
            }
        }

        public void HideShop()
        {
            _panelObject.SetActive(false);
        }

        public void UpdateGoldUI(int playerGold)
        {
            if (_shopGoldText != null) _shopGoldText.text = $"GOLD: {playerGold}";
        }

        public void RefreshAfterPurchase(int playerGold)
        {
            UpdateGoldUI(playerGold);
            foreach (Transform child in _itemsContainer)
            {
                var itemView = child.GetComponent<ShopItemView>();
                if (itemView != null)
                {
                    itemView.CheckAffordability(playerGold);
                }
            }
        }

        private void HandleItemBought(ItemConfig item, ShopItemView view)
        {
            OnItemBuyClicked?.Invoke(item, view);
        }
    }
}