using System;
using MeatMushrooms.Mushroom.Configs;
using MeatMushrooms.Mushroom.Contracts; // Подключили неймспейс
using UnityEngine;

namespace MeatMushrooms.Mushroom.Components
{
    // Теперь класс реализует IEdible
    public class MushroomHealth : MonoBehaviour, IEdible 
    {
        public float CurrentHealth { get; private set; }
        
        // Реализация требования интерфейса (возвращаем свой же Transform)
        public Transform Transform => transform; 

        private Action _onDeathCallback;
        private bool _isDead;

        public void Init(MushroomConfig config, Action onDeath)
        {
            CurrentHealth = config.NutritionValue;
            _onDeathCallback = onDeath;
            _isDead = false;
        }

        public void Consume(float amount)
        {
            if (_isDead) return;

            CurrentHealth -= amount;
            if (CurrentHealth <= 0)
            {
                _isDead = true;
                _onDeathCallback?.Invoke();
            }
        }
    }
}