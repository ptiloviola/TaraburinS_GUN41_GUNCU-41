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
                if (hit.collider.TryGetComponent(out IDamageable target))
                {
                    target.TakeDamage(_config.Damage);
                }

                // Спавним декали только если это НЕ враг
                if (_decalManager != null && !hit.collider.CompareTag("Enemy"))
                {
                    _decalManager.SpawnDecal(hit.point, hit.normal, hit.collider.transform);
                }

                // Запрашиваем трассер до точки попадания
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