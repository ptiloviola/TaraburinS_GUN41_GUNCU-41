using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Gameplay.Projectiles.Factories
{
    public class ProjectileFactory
    {
        private const string PoolGroupNameFormat = "[Pool] Projectiles_{0}";
        
        private readonly DiContainer _container;
        
        private readonly Dictionary<ModularProjectile, ModularProjectile.Pool> _pools = new();

        public ProjectileFactory(DiContainer container)
        {
            _container = container;
        }

        public ModularProjectile.Pool GetPool(ModularProjectile prefab)
        {
            if (_pools.TryGetValue(prefab, out ModularProjectile.Pool existingPool))
            {
                return existingPool;
            }

            DiContainer subContainer = _container.CreateSubContainer();
            
            subContainer.BindMemoryPool<ModularProjectile, ModularProjectile.Pool>()
                .WithInitialSize(10)
                .FromComponentInNewPrefab(prefab)
                .UnderTransformGroup(string.Format(PoolGroupNameFormat, prefab.name));

            ModularProjectile.Pool newPool = subContainer.Resolve<ModularProjectile.Pool>();
            
            _pools[prefab] = newPool;
            
            return newPool;
        }
    }
}