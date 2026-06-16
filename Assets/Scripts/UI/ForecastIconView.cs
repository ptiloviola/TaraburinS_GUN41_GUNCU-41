using UnityEngine;
using TMPro;
using Zenject;

namespace Gameplay.UI
{
    public class ForecastIconView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;  // Имя или ID врага
        [SerializeField] private TextMeshProUGUI _countText; // Текст "x10"
        
        // В будущем сюда можно добавить поле для Image, чтобы менять картинки по EnemyId

        public void Setup(string enemyId, int count)
        {
            // Пока у нас нет картинок, будем просто писать ID врага
            if (_nameText != null) _nameText.text = enemyId.ToUpper();
            if (_countText != null) _countText.text = $"x{count}";
        }

        // Фабрика Zenject для спавна таких иконок прямо в UI
        public class Factory : PlaceholderFactory<ForecastIconView> { }
    }
}
