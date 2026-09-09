using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Gameplay.Projectiles.Factories
{
    /// <summary>
    /// Фабрика снарядов. Сама управляет пулами для разных префабов.
    /// </summary>
    public class ProjectileFactory
    {
        private const string PoolGroupNameFormat = "[Pool] Projectiles_{0}";
        
        private readonly DiContainer _container;
        
        // Словарь для хранения пулов. Ключ - префаб, Значение - его пул.
        private readonly Dictionary<ModularProjectile, ModularProjectile.Pool> _pools = new();

        public ProjectileFactory(DiContainer container)
        {
            _container = container;
        }

        // Метод выдает готовый пул для конкретного префаба
        public ModularProjectile.Pool GetPool(ModularProjectile prefab)
        {
            // Если пул для этого снаряда уже существует — просто возвращаем его
            if (_pools.TryGetValue(prefab, out ModularProjectile.Pool existingPool))
            {
                return existingPool;
            }

            // Если пула еще нет — создаем его "лениво" (Lazy Initialization)
            DiContainer subContainer = _container.CreateSubContainer();
            
            subContainer.BindMemoryPool<ModularProjectile, ModularProjectile.Pool>()
                .WithInitialSize(10) // Для общего пула можно взять запас побольше
                .FromComponentInNewPrefab(prefab)
                .UnderTransformGroup(string.Format(PoolGroupNameFormat, prefab.name));

            ModularProjectile.Pool newPool = subContainer.Resolve<ModularProjectile.Pool>();
            
            // Сохраняем в словарь, чтобы другие башни могли им пользоваться
            _pools[prefab] = newPool;
            
            return newPool;
        }
    }
}