using System.Collections.Generic;
using Gameplay.Modifiers.Enums;

namespace Gameplay.Modifiers.Services
{
    public class StatsModifierService
    {
        private readonly Dictionary<StatType, float> _multipliers = new Dictionary<StatType, float>();

        public void RegisterMultiplier(StatType stat, float multiplier)
        {
            if (!_multipliers.ContainsKey(stat))
            {
                _multipliers[stat] = 1f;
            }
            

            _multipliers[stat] *= multiplier; 
        }

        public float GetMultiplier(StatType stat)
        {
            return _multipliers.TryGetValue(stat, out float val) ? val : 1f;
        }
    }
}