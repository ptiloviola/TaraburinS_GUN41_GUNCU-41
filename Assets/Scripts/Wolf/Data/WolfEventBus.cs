using System;

namespace MeatMushrooms.Wolf.Components
{
    public class WolfEventBus
    {
        public event Action OnHowl;
        public event Action OnCombatGrowl;
        public event Action OnLowGrowl;
        public event Action OnEat;

        public void FireHowl() => OnHowl?.Invoke();
        public void FireCombatGrowl() => OnCombatGrowl?.Invoke();
        public void FireLowGrowl() => OnLowGrowl?.Invoke();
        public void FireEat() => OnEat?.Invoke();
    }
}