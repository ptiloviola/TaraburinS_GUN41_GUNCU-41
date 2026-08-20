using UnityEngine;
using TpsShooter.Combat;

namespace TpsShooter.Weapons.Core
{
    public class HitscanWeapon : WeaponBase
    {
        protected override void PerformFire(Vector3 targetPoint)
        {
            Vector3 direction = (targetPoint - _muzzlePoint.position).normalized;
            FireRaycast(direction);
        }
        protected void FireRaycast(Vector3 direction)
        {
            if (Physics.Raycast(_muzzlePoint.position, direction, out RaycastHit hit, _config.Range, _config.HitMask))
            {
                IDamageable target = hit.collider.GetComponentInParent<IDamageable>();
                
                if (target != null)
                {
                    target.TakeDamage(_config.Damage);
                    _vfxService?.SpawnImpact(hit.point, hit.normal, isEnemy: true);
                }
                else 
                {
                    if (_decalManager != null) _decalManager.SpawnDecal(hit.point, hit.normal, hit.collider.transform);
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
    }
}