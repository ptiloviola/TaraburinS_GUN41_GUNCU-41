using Gameplay.Combat;
using Gameplay.Enemies.Data;

namespace Gameplay.Enemies.Combat
{
    public class ArmorCalculator
    {
        private readonly ArmorStats _stats;

        public ArmorCalculator(ArmorStats stats)
        {
            _stats = stats;
        }

        public float CalculateFinalDamage(DamagePayload payload)
        {
            float multiplier = payload.Type switch
            {
                DamageType.Physical => _stats.PhysicalMultiplier,
                DamageType.Energy => _stats.EnergyMultiplier,
                DamageType.Explosive => _stats.ExplosiveMultiplier,
                _ => 1f
            };

            return payload.Amount * multiplier;
        }
    }
}