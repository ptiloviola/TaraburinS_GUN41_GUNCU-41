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
#if UNITY_EDITOR
                Debug.DrawLine(_muzzlePoint.position, hit.point, Color.green, 2f);
#endif
                if (hit.collider.TryGetComponent(out IDamageable target))
                {
                    target.TakeDamage(_config.Damage);
                }

                // --- БЛОК: СПАВН ДЕКАЛИ ---
                // --- БЛОК: СПАВН ДЕКАЛИ ---
                if (_decalManager != null)
                {
                    _decalManager.SpawnDecal(hit.point, hit.normal, hit.collider.transform);
                }
            }
            else
            {
#if UNITY_EDITOR
                // --- ВИЗУАЛЬНЫЙ ДЕБАГ (КРАСНЫЙ ЛУЧ - ПРОМАХ) ---
                Debug.DrawLine(_muzzlePoint.position, _muzzlePoint.position + direction * _config.Range, Color.red, 2f);
#endif
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