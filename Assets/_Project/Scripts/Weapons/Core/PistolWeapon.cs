using UnityEngine;
using TpsShooter.Combat; 

namespace TpsShooter.Weapons.Core
{
    public class PistolWeapon : WeaponBase
    {
        protected override void PerformFire(Vector3 targetPoint)
        {
            Vector3 direction = (targetPoint - _muzzlePoint.position).normalized;

            if (Physics.Raycast(_muzzlePoint.position, direction, out RaycastHit hit, _config.Range, _config.HitMask))
            {
                // Ищем IDamageable даже на родителях (полезно, если попали в коллайдер кости)
                IDamageable target = hit.collider.GetComponentInParent<IDamageable>();
                
                if (target != null)
                {
                    target.TakeDamage(_config.Damage);
                }
                else if (_decalManager != null) 
                {
                    // Если target == null, значит это не враг и не игрок. Спавним дырку!
                    _decalManager.SpawnDecal(hit.point, hit.normal, hit.collider.transform);
                }

                _vfxService?.SpawnTracer(_muzzlePoint.position, hit.point);
            }
            else
            {
                Vector3 endPoint = _muzzlePoint.position + direction * _config.Range;
                _vfxService?.SpawnTracer(_muzzlePoint.position, endPoint);
            }
        }

        public override void Reload()
        {
            if (_currentAmmoInClip == _config.AmmoPerClip) return;

            _currentAmmoInClip = _config.AmmoPerClip;
            PlayReloadSound();
        }
    }
}