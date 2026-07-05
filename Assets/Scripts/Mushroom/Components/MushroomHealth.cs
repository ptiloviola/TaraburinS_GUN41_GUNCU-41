using System;
using MeatMushrooms.Mushroom.Configs;
using MeatMushrooms.Mushroom.Contracts;
using UnityEngine;

namespace MeatMushrooms.Mushroom.Components
{
    public class MushroomHealth : MonoBehaviour, IEdible 
    {
        public float CurrentHealth { get; private set; }
        public Transform Transform => transform; 

        private Action _onDeathCallback;
        private bool _isDead;

        public void Init(MushroomConfig config, Action onDeath)
        {
            CurrentHealth = config.NutritionValue;
            _onDeathCallback = onDeath;
            _isDead = false;
        }

        public float Consume(float amount)
        {
            if (_isDead) return 0f;

            float eatenAmount = Mathf.Min(amount, CurrentHealth);
            CurrentHealth -= eatenAmount;
            if (CurrentHealth <= 0)
            {
                _isDead = true;
                _onDeathCallback?.Invoke();
            }
            
            return eatenAmount; 
        }
    }
}