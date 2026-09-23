using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Gameplay.Modifiers.Data;

namespace Gameplay.UI.Views
{
    public class ShopItemView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _costText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Button _buyButton;
        
        [Header("Hover Settings")]
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Color _normalColor = new Color(1f, 1f, 1f, 0.39f);
        [SerializeField] private Color _highlightColor = new Color(1f, 1f, 1f, 0.8f);

        private ItemConfig _currentItem;
        private Action<ItemConfig, ShopItemView> _onBuyCallback;

        public void Setup(ItemConfig item, int playerGold, Action<ItemConfig, ShopItemView> onBuyCallback)
        {
            _currentItem = item;
            _onBuyCallback = onBuyCallback;

            _nameText.text = item.DisplayName;
            _descriptionText.text = item.Description;
            _costText.text = item.BaseCost.ToString();
            
            if (item.Icon != null) 
            {
                _iconImage.sprite = item.Icon;
                _iconImage.preserveAspect = true; 
            }

            _backgroundImage.color = _normalColor;
            CheckAffordability(playerGold);

            _buyButton.onClick.RemoveAllListeners();
            _buyButton.onClick.AddListener(() => _onBuyCallback?.Invoke(_currentItem, this));
        }

        public void CheckAffordability(int playerGold)
        {
            if (_currentItem != null)
            {
                _buyButton.interactable = playerGold >= _currentItem.BaseCost;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _backgroundImage.color = _highlightColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _backgroundImage.color = _normalColor;
        }
    }
}