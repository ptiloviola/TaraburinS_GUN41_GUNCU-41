using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Zenject;

namespace Gameplay.UI.Views
{
    public class ForecastIconView : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _nameText;  
        [SerializeField] private TextMeshProUGUI _countText; 
        
        [Header("Спрайт для скрытого врага (опционально)")]
        [SerializeField] private Sprite _unknownSprite; 

        public class Pool : MonoMemoryPool<string, int, Sprite, ForecastIconView>
        {
            protected override void Reinitialize(string enemyId, int count, Sprite iconSprite, ForecastIconView item)
            {
                item.Init(enemyId, count, iconSprite);
            }
        }

        public void Init(string enemyId, int count, Sprite iconSprite)
        {
            // 1. Отрисовка количества (если < 0, значит скрыто)
            if (_countText != null)
            {
                _countText.text = count < 0 ? "x?" : $"x{count}";
            }
            
            // 2. Определение данных: известны они или скрыты
            bool isUnknown = string.IsNullOrEmpty(enemyId);
            Sprite finalSprite = isUnknown ? _unknownSprite : iconSprite;
            string finalName = isUnknown ? "???" : enemyId.ToUpper();

            // 3. Логика отображения (Картинка приоритетнее текста)
            if (finalSprite != null && _iconImage != null)
            {
                _iconImage.sprite = finalSprite;
                _iconImage.gameObject.SetActive(true);
                if (_nameText != null) _nameText.gameObject.SetActive(false);
            }
            else
            {
                if (_iconImage != null) _iconImage.gameObject.SetActive(false);
                if (_nameText != null) 
                {
                    _nameText.gameObject.SetActive(true);
                    _nameText.text = finalName; 
                }
            }
            
            transform.localScale = Vector3.one;
        }
    }
}