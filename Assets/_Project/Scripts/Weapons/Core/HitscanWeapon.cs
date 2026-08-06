using UnityEngine;

namespace TpsShooter.Weapons.Core
{
    public class HitscanWeapon : WeaponBase
    {
        protected override void PerformFire(Vector3 targetPoint)
        {
            // Направление от дула до точки, куда смотрит прицел камеры
            Vector3 direction = (targetPoint - _muzzlePoint.position).normalized;

            // Выпускаем луч (Raycast)
            if (Physics.Raycast(_muzzlePoint.position, direction, out RaycastHit hit, _config.Range, _config.HitMask))
            {
                Debug.Log($"[HitscanWeapon] Попали в: {hit.collider.name}");

                // Позже добавим: if (hit.collider.TryGetComponent(out IDamageable target)) target.TakeDamage(_config.Damage);
                
                // Позже добавим вызов из пула: PoolManager.Spawn(_config.BulletDecalPrefab, hit.point, ...)
            }
            else
            {
                Debug.Log("[HitscanWeapon] Выстрел в молоко");
            }
        }
    }
}