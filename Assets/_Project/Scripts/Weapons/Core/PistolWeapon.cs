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
                IDamageable target = hit.collider.GetComponentInParent<IDamageable>();
                
                if (target != null)
                {
                    target.TakeDamage(_config.Damage);
                    // СПАВН КРОВИ
                    _vfxService?.SpawnImpact(hit.point, hit.normal, isEnemy: true);
                }
                else 
                {
                    if (_decalManager != null) _decalManager.SpawnDecal(hit.point, hit.normal, hit.collider.transform);
                    // СПАВН ИСКР/ПЫЛИ
                    _vfxService?.SpawnImpact(hit.point, hit.normal, isEnemy: false);
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