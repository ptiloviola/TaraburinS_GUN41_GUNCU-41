using Zenject;
using Gameplay.Units;

namespace Gameplay.Towers.Factories
{
    public class DefenderFactory
    {
        private readonly DiContainer _container;

        public DefenderFactory(DiContainer container)
        {
            _container = container;
        }

        public DefenderFacade Create(string defenderId)
        {
            var pool = _container.ResolveId<DefenderFacade.Pool>(defenderId);
            return pool.Spawn();
        }
    }
}