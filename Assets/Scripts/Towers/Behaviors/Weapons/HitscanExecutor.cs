using Gameplay.Core;
using UnityEngine;

namespace Gameplay.Towers.Behaviors.Weapons
{
    public class HitscanExecutor : MonoBehaviour, IAttackExecutor
    {
        public void ExecuteAttack(Transform target, float damage, Transform firePoint)
        {
            // Здесь живет старая добрая логика мгновенного выстрела
            Debug.DrawRay(firePoint.position, firePoint.forward * 5f, Color.red, 0.2f);
            
            var damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
        }
    }
}