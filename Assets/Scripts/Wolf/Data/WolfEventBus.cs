using System;

namespace MeatMushrooms.Wolf.Components
{
    // Легковесный брокер сообщений для конкретного волка
    public class WolfEventBus
    {
        // События, на которые можно подписаться
        public event Action OnHowl;
        public event Action OnCombatGrowl;
        public event Action OnLowGrowl;
        public event Action OnEat;

        // Методы, чтобы Стейты могли "кричать" в шину
        public void FireHowl() => OnHowl?.Invoke();
        public void FireCombatGrowl() => OnCombatGrowl?.Invoke();
        public void FireLowGrowl() => OnLowGrowl?.Invoke();
        public void FireEat() => OnEat?.Invoke();
    }
}