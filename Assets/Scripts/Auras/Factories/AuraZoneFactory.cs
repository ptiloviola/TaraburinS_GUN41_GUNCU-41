using System.Collections.Generic;
using Zenject;

namespace Gameplay.Auras.Factories
{
    public class AuraZoneFactory
    {
        private const string PoolGroupNameFormat = "[Pool] Auras_{0}";
        
        private readonly DiContainer _container;
        private readonly Dictionary<LingeringAuraFacade, LingeringAuraFacade.Pool> _pools = new();

        public AuraZoneFactory(DiContainer container)
        {
            _container = container;
        }

        public LingeringAuraFacade.Pool GetPool(LingeringAuraFacade prefab)
        {
            if (_pools.TryGetValue(prefab, out LingeringAuraFacade.Pool existingPool))
            {
                return existingPool;
            }

            DiContainer subContainer = _container.CreateSubContainer();
            
            subContainer.BindMemoryPool<LingeringAuraFacade, LingeringAuraFacade.Pool>()
                .WithInitialSize(5) 
                .FromComponentInNewPrefab(prefab)
                .UnderTransformGroup(string.Format(PoolGroupNameFormat, prefab.name));

            LingeringAuraFacade.Pool newPool = subContainer.Resolve<LingeringAuraFacade.Pool>();
            
            _pools[prefab] = newPool;
            
            return newPool;
        }
    }
}