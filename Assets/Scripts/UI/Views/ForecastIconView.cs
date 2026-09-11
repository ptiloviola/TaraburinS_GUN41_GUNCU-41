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

        // Zenject Factory (Пул)
        public class Pool : MonoMemoryPool<string, int, Sprite, ForecastIconView>
        {
            protected override void Reinitialize(string enemyId, int count, Sprite iconSprite, ForecastIconView item)
            {
                item.Init(enemyId, count, iconSprite);
            }
        }

        public void Init(string enemyId, int count, Sprite iconSprite)
        {
            if (_countText != null) _countText.text = $"x{count}";
            
            if (iconSprite != null && _iconImage != null)
            {
                _iconImage.sprite = iconSprite;
                _iconImage.gameObject.SetActive(true);
                if (_nameText != null) _nameText.gameObject.SetActive(false);
            }
            else
            {
                if (_iconImage != null) _iconImage.gameObject.SetActive(false);
                if (_nameText != null) 
                {
                    _nameText.gameObject.SetActive(true);
                    _nameText.text = enemyId.ToUpper(); 
                }
            }
            
            // Восстанавливаем масштаб (иногда LayoutGroup может сбрасывать его при спавне из пула)
            transform.localScale = Vector3.one;
        }
    }
}