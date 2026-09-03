using UnityEngine;
using Gameplay.Core;
using Gameplay.Enemies.Data;
using System;


namespace Gameplay.Enemies
{
    // Тонкий адаптер Unity. Отвечает ТОЛЬКО за прием урона.
    [RequireComponent(typeof(Collider))]
    public class DamageReceiver : MonoBehaviour, IDamageable
    {
        private HealthComponent _health;
        private ArmorCalculator _armorCalculator;
        public event Action<DamagePayload> OnHitReceived;

        // Инициализируется извне (Фасадом)
        public void Initialize(HealthComponent health, ArmorCalculator armorCalculator)
        {
            _health = health;
            _armorCalculator = armorCalculator;
        }

        public void TakeDamage(DamagePayload payload)
        {
            if (_health == null || _health.IsDead) return;
            // Сначала кричим визуалу: "В нас попали вот ЭТИМ!"
            OnHitReceived?.Invoke(payload);

            // 1. Считаем броню через чистый класс
            float finalDamage = _armorCalculator.CalculateFinalDamage(payload);

            // 2. Отнимаем ХП
            _health.TakeRawDamage(finalDamage);
        }
    }
}