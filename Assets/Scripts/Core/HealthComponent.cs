using System;
using UnityEngine;


namespace Gameplay.Core
{
    public class HealthComponent : MonoBehaviour, IDamageable
    {

        [Header("Настройки")]
        [SerializeField] private float _maxHealth = 100f;

        private float _currentHealth;

        // События для UI и других систем
        public event Action<float, float> OnHealthChanged; // Текущее ХП, Максимальное ХП
        public event Action OnDied;
        public event Action OnDamaged;

        public bool IsDead => _currentHealth <= 0f;
        public float CurrentHealth => _currentHealth; 
        public float MaxHealth => _maxHealth;

        public void Initialize(float maxHealthOverride = -1f)
        {
            if (maxHealthOverride > 0)
            {
                _maxHealth = maxHealthOverride;
            }

            _currentHealth = _maxHealth;
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        public void TakeDamage(float amount)
        {
            if (IsDead) return;
            _currentHealth -= amount;
            _currentHealth = Mathf.Clamp(_currentHealth, 0f, _maxHealth); // ХП не может упасть ниже нуля
            Debug.Log($"<color=orange>[Health] {gameObject.name} получил {amount} урона. Осталось: {_currentHealth}/{_maxHealth}</color>");
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            OnDamaged?.Invoke();
            if (IsDead)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log($"<color=red>[Health] {gameObject.name} уничтожен!</color>");
            OnDied?.Invoke();
        }


    }

}
