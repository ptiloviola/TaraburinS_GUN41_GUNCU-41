using UnityEngine;
using Zenject;
using Gameplay.Projectiles;
using Gameplay.Projectiles.Contracts;

namespace Gameplay.Towers.Behaviors.Weapons
{
    // Никакого MonoBehaviour!
    public class ProjectileExecutor : IAttackExecutor
    {
        private readonly ModularProjectile _projectilePrefab;
        private readonly IInstantiator _instantiator;
        
        // Передаем префаб из конфига и инстанциатор от адаптера
        public ProjectileExecutor(ModularProjectile projectilePrefab, IInstantiator instantiator)
        {
            _projectilePrefab = projectilePrefab;
            _instantiator = instantiator;
        }

        public void ExecuteAttack(Transform target, IProjectilePayload payload, Transform firePoint)
        {
            if (_projectilePrefab == null)
            {
                Debug.LogError("[ProjectileExecutor] Префаб снаряда не назначен в AttackStats!");
                return;
            }

            var projectile = _instantiator.InstantiatePrefabForComponent<ModularProjectile>(
                _projectilePrefab, firePoint.position, firePoint.rotation, null);
            
            projectile.Launch(target, payload, null);
        }
    }
}