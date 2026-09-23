using UnityEngine;
using Zenject;
using Gameplay.Combat;
using Gameplay.Enemies.Data;
using System;

namespace Gameplay.Enemies
{
    [RequireComponent(typeof(Collider))]
    public class DamageReceiver : MonoBehaviour, IDamageable
    {
        private HealthComponent _health;
        private ArmorCalculator _armorCalculator;
        private SignalBus _signalBus;

        // ВОЗВРАЩАЕМ ЛОКАЛЬНОЕ СОБЫТИЕ: Его слушает DamageFlashVisualizer
        public event Action<DamagePayload> OnHitReceived;

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void Initialize(HealthComponent health, ArmorCalculator armorCalculator)
        {
            _health = health;
            _armorCalculator = armorCalculator;
        }

        public void TakeDamage(DamagePayload payload)
        {
            if (_health == null || _health.IsDead) return;

            // 1. Сначала считаем финальный урон через резисты гусеницы
            float finalDamage = _armorCalculator.CalculateFinalDamage(payload);

            // 2. Упаковываем финальный урон обратно в структуру для UI и вспышек
            DamagePayload finalPayload = new DamagePayload(finalDamage, payload.Type);

            // 3. Отправляем в глобальную шину уже ПРАВИЛЬНЫЕ цифры
            Vector3 textSpawnPos = transform.position + Vector3.up * 1.5f;
            _signalBus.Fire(new DamageReceivedSignal(textSpawnPos, finalPayload));

            // 4. Оповещаем локальные визуалы (вспышки)
            OnHitReceived?.Invoke(finalPayload);

            // 5. Отнимаем здоровье
            _health.TakeRawDamage(finalDamage);
        }
    }
}