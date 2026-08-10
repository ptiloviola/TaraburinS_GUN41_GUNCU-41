using UnityEngine;
using TpsShooter.Combat;

namespace TpsShooter.Weapons.Core
{
    public class AssaultRifleWeapon : WeaponBase
    {
        // ЗДЕСЬ БОЛЬШЕ НЕТ ОБЪЯВЛЕНИЯ _currentSpread! Мы берем его из WeaponBase.

        private void Update()
        {
            if (_config == null) return;

            if (Time.time - _lastFireTime > _config.FireRate)
            {
                // Используем унаследованное поле _currentSpread
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
                Debug.Log($"[AssaultRifle] Попали в: {hit.collider.name}");
                
#if UNITY_EDITOR
                Debug.DrawLine(_muzzlePoint.position, hit.point, Color.green, 2f);
#endif
                
                if (hit.collider.TryGetComponent(out IDamageable target))
                {
                    target.TakeDamage(_config.Damage);
                }

                // --- БЛОК: СПАВН ДЕКАЛИ ---
                if (_decalManager != null)
                {
                    _decalManager.SpawnDecal(hit.point, hit.normal, hit.collider.transform);
                }
            }
            else
            {
#if UNITY_EDITOR
                Debug.DrawLine(_muzzlePoint.position, _muzzlePoint.position + direction * _config.Range, Color.red, 2f);
#endif
            }

            _currentSpread = Mathf.Min(_currentSpread + _config.SpreadIncreaseRate, _config.MaxSpread);
            
            // Заглушка для отдачи
            // ApplyCameraRecoil();
        }
    }
}