using System;
using Gameplay.Modifiers.Enums;

namespace Gameplay.Modifiers.Data
{
    [Serializable]
    public struct StatModifierData
    {
        public StatType StatType;
        public float Multiplier; 
    }
}