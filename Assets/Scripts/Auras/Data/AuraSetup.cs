using System.Collections.Generic;
using Gameplay.Core.Statuses.Data;

namespace Gameplay.Auras.Data
{
    // Чистая структура данных, отвязанная от башен и врагов.
    // Описывает, как именно должна работать конкретная лужа.
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