using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using Gameplay.Towers.Data;
using System;
using UnityEngine.EventSystems; 

namespace Gameplay.UI
{
    public class TowerButtonView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI Элементы")]
        [SerializeField] private Image _backgroundImage; 
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _costText;
        [SerializeField] private Button _button;

        [Header("Настройки цвета (Подсветка)")]
        [SerializeField] private Color _normalColor = new Color(0.2f, 0.2f, 0.2f, 1f); 
        [SerializeField] private Color _selectedColor = new Color(0.2f, 0.6f, 0.2f, 1f); 

        public string TowerId { get; private set; } 

        private Action<string> _onClickedCallback; 
        private Action<string> _onHoverEnterCallback;
        private Action _onHoverExitCallback;
        
        // ИЗМЕНЕНО: Принимаем TowerConfig напрямую
        public void Init(TowerConfig config, Action<string> onClicked, Action<string> onHoverEnter, Action onHoverExit)
        {
            TowerId = config.TowerId;
            _nameText.text = config.DisplayName;
            _costText.text = $"{config.BaseCost} $";
            
            if (config.Icon != null) 
                _iconImage.sprite = config.Icon;

            _onClickedCallback = onClicked;
            _onHoverEnterCallback = onHoverEnter;
            _onHoverExitCallback = onHoverExit;
            
            _button.onClick.AddListener(HandleClick);
            SetSelected(false);
        }

        private void HandleClick()
        {
            _onClickedCallback?.Invoke(TowerId);
        }

        public void SetSelected(bool isSelected)
        {
            if (_backgroundImage != null)
            {
                _backgroundImage.color = isSelected ? _selectedColor : _normalColor;
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            _onHoverEnterCallback?.Invoke(TowerId);
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            _onHoverExitCallback?.Invoke();
        }
        
        private void OnDestroy()
        {
            _button.onClick.RemoveListener(HandleClick);
        }
    }
}