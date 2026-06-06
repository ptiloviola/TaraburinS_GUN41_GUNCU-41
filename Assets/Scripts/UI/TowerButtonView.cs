using UnityEngine;
using UnityEngine.UI;
using TMPro; // Обязательно используем TextMeshPro для красивого текста
using Gameplay.Towers.Data;
using System;
using UnityEngine.EventSystems; // Обязательно для событий мыши

namespace Gameplay.UI
{
    // Добавляем интерфейсы IPointerEnterHandler и IPointerExitHandler
    public class TowerButtonView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI Элементы")]
        [SerializeField] private Image _backgroundImage; // Фон самой кнопки
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _costText;
        [SerializeField] private Button _button;

        [Header("Настройки цвета (Подсветка)")]
        [SerializeField] private Color _normalColor = new Color(0.2f, 0.2f, 0.2f, 1f); // Темно-серый
        [SerializeField] private Color _selectedColor = new Color(0.2f, 0.6f, 0.2f, 1f); // Зеленоватый

        public string TowerId { get; private set; } // Добавили геттер для панели


        private Action<string> _onClickedCallback; // Делегат для передачи клика "наверх"

        // НОВОЕ: Делегаты для наведения мыши
        private Action<string> _onHoverEnterCallback;
        private Action _onHoverExitCallback;
        
        // Метод инициализации. Панель вызовет его и передаст данные башни
        public void Init(TowerShopData data, Action<string> onClicked, Action<string> onHoverEnter, Action onHoverExit)
        {
            TowerId = data.TowerId;
            _nameText.text = data.DisplayName;
            _costText.text = $"{data.Cost} $";
            
            if (data.Icon != null) 
                _iconImage.sprite = data.Icon;

            _onClickedCallback = onClicked;
            _onHoverEnterCallback = onHoverEnter;
            _onHoverExitCallback = onHoverExit;
            
            // Подписываемся на клик самой Unity UI кнопки
            _button.onClick.AddListener(HandleClick);
            SetSelected(false); // По умолчанию кнопка не выбрана
        }

        private void HandleClick()
        {
            // Вызываем переданный метод и отдаем ему ID этой башни
            _onClickedCallback?.Invoke(TowerId);
        }

        // НОВЫЙ МЕТОД: Включает или выключает подсветку
        public void SetSelected(bool isSelected)
        {
            if (_backgroundImage != null)
            {
                _backgroundImage.color = isSelected ? _selectedColor : _normalColor;
            }
        }

        
        // НОВОЕ: Метод срабатывает автоматически при входе курсора в зону кнопки
        public void OnPointerEnter(PointerEventData eventData)
        {
            _onHoverEnterCallback?.Invoke(TowerId);
        }
        // НОВОЕ: Метод срабатывает автоматически при выходе курсора
        public void OnPointerExit(PointerEventData eventData)
        {
            _onHoverExitCallback?.Invoke();
        }
        private void OnDestroy()
        {
            // Хороший тон: отписываемся от событий при уничтожении объекта
            _button.onClick.RemoveListener(HandleClick);
        }
        
    }
}