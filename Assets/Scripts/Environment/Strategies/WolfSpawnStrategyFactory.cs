using System;

namespace MeatMushrooms.Environment.Strategies
{
    public static class WolfSpawnStrategyFactory
    {
        public static IWolfSpawnStrategy Create(WolfSpawnLayout layout)
        {
            return layout switch
            {
                WolfSpawnLayout.CenterCircle => new CenterCircleSpawnStrategy(),
                WolfSpawnLayout.LineAcrossMap => new LineAcrossMapSpawnStrategy(),
                _ => throw new ArgumentOutOfRangeException(nameof(layout), layout, null)
            };
        }
    }
}