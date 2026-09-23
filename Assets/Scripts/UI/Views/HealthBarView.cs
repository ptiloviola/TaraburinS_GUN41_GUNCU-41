using Gameplay.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Views
{
    public class HealthBarView : MonoBehaviour
    {
        [Header("Ссылки")]
        [SerializeField] private HealthComponent _health;
        [SerializeField] private Image _fillImage;

        private Camera _cachedCamera;

        private void Awake()
        {
            if (_health == null) _health = GetComponentInParent<HealthComponent>();
            
            _cachedCamera = Camera.main;
        }

        private void OnEnable()
        {
            if (_health != null)
            {
                _health.OnHealthChanged += UpdateFill;
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
            if (_cachedCamera != null)
            {
                transform.forward = _cachedCamera.transform.forward;
            }
        }
    }
}