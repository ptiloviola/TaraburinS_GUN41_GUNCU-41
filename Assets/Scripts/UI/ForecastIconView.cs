using UnityEngine;
using TMPro;
using Zenject;
using UnityEngine.UI;

namespace Gameplay.UI
{
    public class ForecastIconView : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _nameText;  // Имя или ID врага
        [SerializeField] private TextMeshProUGUI _countText; // Текст "x10"
        
        // В будущем сюда можно добавить поле для Image, чтобы менять картинки по EnemyId

        public void Setup(string enemyId, int count, Sprite iconSprite)
        {
            // Пока у нас нет картинок, будем просто писать ID врага
            if (_nameText != null) _nameText.text = enemyId.ToUpper();
            if (_countText != null) _countText.text = $"x{count}";
            if (iconSprite != null && _iconImage != null)
            {
                // Если картинка есть — показываем её, а текстовый ID скрываем
                _iconImage.sprite = iconSprite;
                _iconImage.gameObject.SetActive(true);
                if (_nameText != null) _nameText.gameObject.SetActive(false);
            }
            else
            {
                // Если картинки нет (забыли добавить в конфиг) — показываем просто текст
                if (_iconImage != null) _iconImage.gameObject.SetActive(false);
                if (_nameText != null) 
                {
                    _nameText.gameObject.SetActive(true);
                    // Выводим имя большими буквами, например "GOBLIN"
                    _nameText.text = enemyId.ToUpper(); 
                }
            }
        }

        // Фабрика Zenject для спавна таких иконок прямо в UI
        public class Factory : PlaceholderFactory<ForecastIconView> { }
    }
}
