using UnityEngine;
using Zenject;
using Gameplay.Projectiles;
using Gameplay.Projectiles.Contracts;

namespace Gameplay.Towers.Behaviors.Weapons
{
    public class ProjectileExecutor : MonoBehaviour, IAttackExecutor
    {
        [Header("Боеприпас")]
        [Tooltip("Перетащите сюда префаб снаряда, которым должна стрелять эта башня")]
        [SerializeField] private ModularProjectile _projectilePrefab;
        
        private IInstantiator _instantiator;
        
        [Inject]
        public void Construct(IInstantiator instantiator)
        {
            _instantiator = instantiator;
        }   

        public void ExecuteAttack(Transform target, IProjectilePayload payload, Transform firePoint)
        {
            if (_projectilePrefab == null)
            {
                Debug.LogError($"[ProjectileExecutor] На башне {gameObject.name} не назначен префаб снаряда!");
                return;
            }

            // 1. Просим Zenject создать конкретный префаб снаряда (он автоматически прокинет в него зависимости, если нужно)
            var projectile = _instantiator.InstantiatePrefabForComponent<ModularProjectile>(
                _projectilePrefab, firePoint.position, firePoint.rotation, null);
            
            // 2. Запускаем! (Передаем null вместо пула. Снаряд сам вызовет Destroy при попадании)
            projectile.Launch(target, payload, null);
        }
    }
}