using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Zenject;

namespace Gameplay.UI.Views
{
    public class TowerButtonView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI Элементы")]
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _costText;
        [SerializeField] private Button _button;

        [Header("Цвета")]
        [SerializeField] private Color _normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        [SerializeField] private Color _selectedColor = new Color(0.2f, 0.6f, 0.2f, 1f);

        public string TowerId { get; private set; }

        private TowerShopView _parentView;

        // Инжектим родительский View напрямую из контейнера!
        [Inject]
        public void Construct(TowerShopView parentView)
        {
            _parentView = parentView;
        }

        // Пул стал меньше и надежнее: только ID, Имя, Цена и Иконка
        public class Pool : MonoMemoryPool<string, string, int, Sprite, TowerButtonView>
        {
            protected override void Reinitialize(string id, string name, int cost, Sprite icon, TowerButtonView button)
            {
                button.Init(id, name, cost, icon);
            }
        }

        public void Init(string id, string name, int cost, Sprite icon)
        {
            TowerId = id;
            _nameText.text = name;
            _costText.text = $"{cost} $";
            if (icon != null) _iconImage.sprite = icon;

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnClick);
            
            SetSelected(false);
            transform.SetAsLastSibling(); 
        }

        public void SetSelected(bool isSelected)
        {
            if (_backgroundImage != null)
                _backgroundImage.color = isSelected ? _selectedColor : _normalColor;
        }

        private void OnClick() => _parentView.HandleClick(TowerId);
        public void OnPointerEnter(PointerEventData eventData) => _parentView.HandleHoverEnter(TowerId);
        public void OnPointerExit(PointerEventData eventData) => _parentView.HandleHoverExit();
    }
}