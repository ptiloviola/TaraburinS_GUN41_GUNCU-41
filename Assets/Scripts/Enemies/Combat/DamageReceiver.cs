using UnityEngine;
using Zenject;
using Gameplay.Combat;
using System;

namespace Gameplay.Enemies.Combat
{
    [RequireComponent(typeof(Collider))]
    public class DamageReceiver : MonoBehaviour, IDamageable
    {
        private HealthComponent _health;
        private ArmorCalculator _armorCalculator;
        private SignalBus _signalBus;

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

            float finalDamage = _armorCalculator.CalculateFinalDamage(payload);

            DamagePayload finalPayload = new DamagePayload(finalDamage, payload.Type);

            Vector3 textSpawnPos = transform.position + Vector3.up * 1.5f;
            _signalBus.Fire(new DamageReceivedSignal(textSpawnPos, finalPayload));

            OnHitReceived?.Invoke(finalPayload);

            _health.TakeRawDamage(finalDamage);
        }
    }
}