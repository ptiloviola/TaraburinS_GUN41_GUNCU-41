using UnityEngine;
using Zenject;
using Gameplay.Towers.Behaviors.Weapons;
using Gameplay.Projectiles; // Для доступа к ModularProjectile
using Gameplay.Projectiles.Contracts; // Для доступа к IProjectilePayload
// Подключаем папку с реализациями наших "посылок"
using Gameplay.Projectiles.Payloads; 

namespace Gameplay.Towers.Behaviors.Weapons
{
    public class ProjectileExecutor : MonoBehaviour, IAttackExecutor
    {
        // ТЕПЕРЬ ИСПОЛЬЗУЕМ ПУЛ НОВЫХ МОДУЛЬНЫХ СНАРЯДОВ
        private ModularProjectile.Pool _projectilePool;
        
        [Inject]
        public void Construct(ModularProjectile.Pool projectilePool)
        {
            _projectilePool = projectilePool;
        }   

        public void ExecuteAttack(Transform target, IProjectilePayload payload, Transform firePoint)
        {
            if (_projectilePool == null)
            {
                Debug.LogError("[ProjectileExecutor] Пул снарядов не установлен в Zenject!");
                return;
            }

            // 1. Достаем пустую оболочку (модульный снаряд) из пула
            var projectile = _projectilePool.Spawn(); 
            
            // 2. Ставим снаряд точно в дуло пушки
            projectile.transform.position = firePoint.position;
            projectile.transform.rotation = firePoint.rotation;

            // 3. УПАКОВКА: Создаем полезную нагрузку (Payload) с конкретным уроном.
            // (Если ты назвал класс по-другому, например SingleTargetDamageEffect, измени название здесь)
            // 4. ЗАПУСК: Отдаем снаряду цель, посылку и ссылку на его родной пул для возврата
            projectile.Launch(target, payload, _projectilePool);
        }
    }
}