using System;
using UnityEngine;

namespace Gameplay.Combat
{
    public class HealthComponent : MonoBehaviour
    {
        [Header("Настройки")]
        [SerializeField] private float _maxHealth = 100f;

        private float _currentHealth;

        public event Action<float, float> OnHealthChanged; 
        public event Action OnDied;
        public event Action OnDamaged;

        public bool IsDead => _currentHealth <= 0f;
        public float CurrentHealth => _currentHealth; 
        public float MaxHealth => _maxHealth;

        public void Initialize(float maxHealthOverride = -1f)
        {
            if (maxHealthOverride > 0)
                _maxHealth = maxHealthOverride;

            _currentHealth = _maxHealth;
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        public void TakeRawDamage(float amount)
        {
            if (IsDead) return;
            
            _currentHealth -= amount;
            _currentHealth = Mathf.Clamp(_currentHealth, 0f, _maxHealth); 
            
            Gameplay.Tools.GameLogger.Log($"<color=orange>[Health] {gameObject.name} получил {amount:F1} чистого урона. Осталось: {_currentHealth:F1}/{_maxHealth}</color>");
            
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            OnDamaged?.Invoke();
            
            if (IsDead) Die();
        }

        private void Die()
        {
            Gameplay.Tools.GameLogger.Log($"<color=red>[Health] {gameObject.name} уничтожен!</color>");
            OnDied?.Invoke();
        }
    }
}