using UnityEngine;
using Gameplay.Projectiles;
using Gameplay.Projectiles.Contracts;
using Gameplay.Projectiles.Factories;

namespace Gameplay.Towers.Behaviors.Weapons
{
    public class ProjectileExecutor : IAttackExecutor
    {
        private readonly ModularProjectile _projectilePrefab;
        private readonly ModularProjectile.Pool _pool;
        

        public ProjectileExecutor(ModularProjectile projectilePrefab, ProjectileFactory factory)
        {
            _projectilePrefab = projectilePrefab;

#if UNITY_EDITOR
            if (_projectilePrefab == null)
            {
                Debug.LogError("[ProjectileExecutor] Префаб снаряда не назначен в AttackStats!");
                return;
            }
#endif
            _pool = factory.GetPool(_projectilePrefab);
        }

        public void ExecuteAttack(Transform target, IProjectilePayload payload, Transform firePoint)
        {
            if (_pool == null) return;

            ModularProjectile projectile = _pool.Spawn();
            
            projectile.transform.position = firePoint.position;
            projectile.transform.rotation = firePoint.rotation;
            
            projectile.Launch(target, payload, _pool);
        }
    }
}