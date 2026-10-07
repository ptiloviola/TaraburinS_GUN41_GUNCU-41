using Gameplay.Base;
using Gameplay.Enemies.Data;
using Gameplay.Infrastructure.Signals;
using Zenject;

namespace Gameplay.Enemies.Services
{
    public class BaseArrivalHandler
    {
        private readonly SignalBus _signalBus;

        public BaseArrivalHandler(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void ProcessArrival(BaseCore baseCore, EnemyConfig config)
        {
            baseCore.TakeDamage(config.Stats.DamageToBase);
            _signalBus.Fire<SignalEnemyReachedBase>();
        }
    }
}