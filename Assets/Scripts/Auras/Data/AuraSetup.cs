using System.Collections.Generic;
using Gameplay.Combat.Statuses.Data;
using Gameplay.Combat;

namespace Gameplay.Auras.Data
{
    public struct AuraSetup
    {
        public float Radius;
        public float Duration;
        public float TickRate;
        public IReadOnlyList<IStatusConfig> StatusEffects;

        public DamagePayload DamagePayload;

        public AuraSetup(float radius, float duration, float tickRate, IReadOnlyList<IStatusConfig> statusEffects, DamagePayload damagePayload)
        {
            Radius = radius;
            Duration = duration;
            TickRate = tickRate;
            StatusEffects = statusEffects;
            DamagePayload = damagePayload;
        }
    }
}