using UnityEngine;
using TpsShooter.Combat;

namespace TpsShooter.Weapons.Core
{
    public class AssaultRifleWeapon : WeaponBase
    {
        private void Update()
        {
            if (_config == null) return;

            if (Time.time - _lastFireTime > _config.FireRate)
            {
                _currentSpread = Mathf.MoveTowards(_currentSpread, _config.BaseSpread, _config.SpreadRecoveryRate * Time.deltaTime);
            }
        }

        protected override void PerformFire(Vector3 targetPoint)
        {
            Vector3 direction = (targetPoint - _muzzlePoint.position).normalized;

            if (_currentSpread > 0f)
            {
                direction += Random.insideUnitSphere * _currentSpread;
                direction.Normalize();
            }

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
                // Если выстрел в молоко, пускаем трассер до конца дистанции
                Vector3 endPoint = _muzzlePoint.position + direction * _config.Range;
                _vfxService?.SpawnTracer(_muzzlePoint.position, endPoint);
            }

            _currentSpread = Mathf.Min(_currentSpread + _config.SpreadIncreaseRate, _config.MaxSpread);
        }
    }
}