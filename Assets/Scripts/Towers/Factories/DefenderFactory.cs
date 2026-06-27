using Zenject;
using Gameplay.Units;

namespace Gameplay.Towers.Factories
{
    public class DefenderFactory
    {
        private readonly DiContainer _container;

        // Только эта фабрика знает про DiContainer
        public DefenderFactory(DiContainer container)
        {
            _container = container;
        }

        public DefenderFacade Create(string defenderId)
        {
            // Здесь мы прячем логику получения нужного пула по ID
            var pool = _container.ResolveId<DefenderFacade.Pool>(defenderId);
            return pool.Spawn();
        }
    }
}