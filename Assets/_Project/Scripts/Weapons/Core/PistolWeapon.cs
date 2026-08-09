using UnityEngine;
using TpsShooter.Combat; // Для IDamageable

namespace TpsShooter.Weapons.Core
{
    public class PistolWeapon : WeaponBase
    {
        protected override void PerformFire(Vector3 targetPoint)
        {
            Vector3 direction = (targetPoint - _muzzlePoint.position).normalized;

            if (Physics.Raycast(_muzzlePoint.position, direction, out RaycastHit hit, _config.Range, _config.HitMask))
            {
                Debug.Log($"[Pistol] Попали в: {hit.collider.name}");
                
                if (hit.collider.TryGetComponent(out IDamageable target))
                {
                    target.TakeDamage(_config.Damage);
                }
                // Заглушка: Спавн декали попадания через PoolManager
            }
        }

        public override void Reload()
        {
            // У пистолета бесконечный запас, поэтому мы просто восстанавливаем обойму
            if (_currentAmmoInClip == _config.AmmoPerClip) return;

            _currentAmmoInClip = _config.AmmoPerClip;
            Debug.Log($"[PistolWeapon] Перезарядка! Бесконечный запас. В магазине: {_currentAmmoInClip}");
            
            PlayReloadSound();
        }
    }
}