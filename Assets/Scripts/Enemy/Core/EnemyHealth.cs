using UnityEngine;
using Infrastructure.Interfaces;
using System;
namespace Enemy.Core
{
    public class EnemyHealth : MonoBehaviour, IHealth
    {
        private int _maxHealth;
        private int _currentHealth;

        public int CurrentHealth => _currentHealth;
        public int MaxHealth => _maxHealth;

        public event Action OnDeath;

        public void Initialize(int maxHealth)
        {
            _maxHealth = maxHealth;
            _currentHealth = maxHealth;
        }

        public void TakeDamage(int amount, Vector3 hitPoint)
        {
            if (_currentHealth <= 0) return;

            _currentHealth -= amount;
            
            if (_currentHealth <= 0)
            {
                OnDeath?.Invoke();
            }
        }
    }
}