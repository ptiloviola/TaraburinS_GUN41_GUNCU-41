using Gameplay.Core;
using UnityEngine;
// НОВОЕ: Подключаем контракты
using Gameplay.Projectiles.Contracts;

namespace Gameplay.Towers.Behaviors.Weapons
{
    public class HitscanExecutor : MonoBehaviour, IAttackExecutor
    {
        // ИСПРАВЛЕНО: Меняем float damage на IProjectilePayload payload
        public void ExecuteAttack(Transform target, IProjectilePayload payload, Transform firePoint)
        {
            Debug.DrawRay(firePoint.position, firePoint.forward * 5f, Color.red, 0.2f);
            
            // Хитскан доставляет посылку мгновенно! Никаких полетов и снарядов.
            if (payload != null)
            {
                // Применяем эффект прямо в координаты цели
                payload.Apply(target, target.position);
            }
        }
    }
}