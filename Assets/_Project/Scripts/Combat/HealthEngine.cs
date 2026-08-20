using System;
using UnityEngine;

namespace TpsShooter.Combat
{
    public class HealthEngine
    {
        public event Action<float, float> OnHealthChanged;
        public event Action OnDeath;

        public float MaxHealth { get; private set; }
        public float CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0;

        public HealthEngine(float maxHealth)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (IsDead) return;

            CurrentHealth -= amount;
            CurrentHealth = Mathf.Max(CurrentHealth, 0);

            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

            if (CurrentHealth <= 0)
            {
                OnDeath?.Invoke();
            }
        }

        public bool TryHeal(float amount)
        {
            if (IsDead || CurrentHealth >= MaxHealth) return false;

            CurrentHealth += amount;
            CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth); 

            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth); 
            return true;
        }
    }
}