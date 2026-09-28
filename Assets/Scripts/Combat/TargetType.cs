using System;

namespace Gameplay.Combat
{
    [Flags]
    public enum TargetType
    {
        None = 0,
        Ground = 1 << 0,
        Air = 1 << 1,
        Underground = 1 << 2
    }
}