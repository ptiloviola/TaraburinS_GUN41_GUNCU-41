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
                if (hit.collider.TryGetComponent(out IDamageable target))
                {
                    target.TakeDamage(_config.Damage);
                }

                if (_decalManager != null && !hit.collider.CompareTag("Enemy"))
                {
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