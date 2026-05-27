using Gameplay.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI
{
    public class HealthBarView : MonoBehaviour
    {
        [Header("Ссылки")]
        [SerializeField] private HealthComponent _health;
        [SerializeField] private Image _fillImage; // Ссылка на картинку полоски

        private void Awake()
        {
            // Если забыли назначить в инспекторе, ищем на родителе
            if (_health == null) _health = GetComponentInParent<HealthComponent>();
        }

        private void OnEnable()
        {
            if (_health != null)
            {
                _health.OnHealthChanged += UpdateFill;
                // Принудительно обновляем UI при включении, чтобы не было старых значений
                UpdateFill(_health.CurrentHealth, _health.MaxHealth);
            }
        }

        private void OnDisable()
        {
            if (_health != null)
            {
                _health.OnHealthChanged -= UpdateFill;
            }
        }

        private void UpdateFill(float current, float max)
        {
            if (_fillImage != null && max > 0)
            {
                _fillImage.fillAmount = current / max;
            }
        }

        private void LateUpdate()
        {
            // Эффект Billboard: заставляем Canvas всегда смотреть прямо в главную камеру
            if (Camera.main != null)
            {
                transform.forward = Camera.main.transform.forward;
            }
        }
    }
}