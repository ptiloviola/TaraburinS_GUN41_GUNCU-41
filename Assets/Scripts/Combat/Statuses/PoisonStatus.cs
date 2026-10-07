using UnityEngine;
using Gameplay.Enemies;
using Gameplay.Combat;

namespace Gameplay.Combat.Statuses
{
    public class PoisonStatus : IStatusEffect
    {
        public string Id => "Poison";
        public StatusType Type => StatusType.DamageOverTime;

        public float SpeedModifier => 1f;
        public float DamageTakenModifier => 1f;
        public bool IsFinished { get; private set; }

        private float _duration;
        private float _tickTimer;
        private readonly float _tickRate;
        private readonly int _damagePerTick;
        
        private GameObject _enemy;
        private IDamageable _damageable; 

        public PoisonStatus(float baseDuration, float tickRate, int damagePerTick)
        {
            _duration = baseDuration;
            _tickRate = tickRate;
            _damagePerTick = damagePerTick;
            _tickTimer = tickRate; 
        }

        public void ApplyResistance(float durationMultiplier)
        {
            _duration *= durationMultiplier;
        }

        public void OnApply(GameObject enemy)
        {
            _enemy = enemy;
            _damageable = enemy.GetComponent<IDamageable>(); 
        }

        public void Tick(float deltaTime)
        {
            if (IsFinished) return;

            _duration -= deltaTime;
            _tickTimer -= deltaTime;

            if (_tickTimer <= 0f)
            {
                _tickTimer = _tickRate;
                ApplyPoisonDamage();
            }

            if (_duration <= 0f)
            {
                IsFinished = true;
            }
        }

        private void ApplyPoisonDamage()
        {
            if (_enemy == null || _damageable == null) return;
            
            _damageable.TakeDamage(new DamagePayload(_damagePerTick, DamageType.Physical));
        }

        public void OnRemove() { }
    }
}