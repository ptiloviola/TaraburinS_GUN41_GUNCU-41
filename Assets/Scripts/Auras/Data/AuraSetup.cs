using System.Collections.Generic;
using Gameplay.Combat.Statuses.Data;

namespace Gameplay.Auras.Data
{
    public struct AuraSetup
    {
        public float Radius;
        public float Duration;
        public float TickRate;
        public IReadOnlyList<IStatusConfig> StatusEffects;

        public AuraSetup(float radius, float duration, float tickRate, IReadOnlyList<IStatusConfig> statusEffects)
        {
            Radius = radius;
            Duration = duration;
            TickRate = tickRate;
            StatusEffects = statusEffects;
        }
    }
}