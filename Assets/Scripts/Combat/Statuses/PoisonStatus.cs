using UnityEngine;
using Gameplay.Enemies;

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
        
        private EnemyFacade _enemy;
        private IDamageReceiver _damageReceiver;

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

        public void OnApply(EnemyFacade enemy)
        {
            _enemy = enemy;
            _damageReceiver = enemy.GetComponent<IDamageReceiver>();
            
#if UNITY_EDITOR
            Debug.Log($"<color=green>[Status] Применен яд. Время: {_duration} сек. Урон: {_damagePerTick} раз в {_tickRate} сек.</color>");
#endif
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
            if (_enemy == null || _damageReceiver == null) return;
            
            _damageReceiver.TakeDamage(_damagePerTick);
        }

        public void OnRemove()
        {
#if UNITY_EDITOR
            Debug.Log("<color=green>[Status] Яд спал.</color>");
#endif
        }
    }
}