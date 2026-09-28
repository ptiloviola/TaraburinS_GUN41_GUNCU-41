using UnityEngine;
using Gameplay.Enemies;

namespace Gameplay.Combat.Statuses
{
    public class FreezeStatus : IStatusEffect
    {
        public string Id => "Freeze";
        public StatusType Type => StatusType.Control;
        
        public float SpeedModifier { get; private set; }
        public float DamageTakenModifier => 1f;
        public bool IsFinished { get; private set; }

        private float _duration;
        private EnemyFacade _enemy;

        public FreezeStatus(float baseDuration, float slowPercent)
        {
            _duration = baseDuration;
            SpeedModifier = 1f - Mathf.Clamp01(slowPercent);
        }

        public void ApplyResistance(float durationMultiplier)
        {
            _duration *= durationMultiplier;
        }

        public void OnApply(EnemyFacade enemy)
        {
            _enemy = enemy;
            Gameplay.Tools.GameLogger.Log($"<color=cyan>[Status] Применена заморозка. Итоговое время после резистов: {_duration} сек. Множитель скорости: {SpeedModifier}</color>");
        }

        public void Tick(float deltaTime)
        {
            if (IsFinished) return;

            _duration -= deltaTime;
            if (_duration <= 0f) IsFinished = true;
        }

        public void OnRemove()
        {
            Gameplay.Tools.GameLogger.Log("<color=cyan>[Status] Заморозка спала.</color>");
        }
    }
}