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
            CurrentHealth = config.NutritionValue; // Вот они, наши 50 калорий из конфига!
            _onDeathCallback = onDeath;
            _isDead = false;
        }

        public float Consume(float amount)
        {
            if (_isDead) return 0f;

            // Считаем, сколько реально удалось откусить (не больше, чем осталось здоровья)
            float eatenAmount = Mathf.Min(amount, CurrentHealth);
            
            CurrentHealth -= eatenAmount;
            
            if (CurrentHealth <= 0)
            {
                _isDead = true;
                _onDeathCallback?.Invoke();
            }
            
            // Возвращаем откушенный кусок волку
            return eatenAmount; 
        }
    }
}