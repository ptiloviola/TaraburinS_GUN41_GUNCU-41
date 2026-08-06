using UnityEngine;
using TpsShooter.Weapons.Configs;

namespace TpsShooter.Weapons.Core
{
    public abstract class WeaponBase : MonoBehaviour, IWeapon
    {
        [SerializeField] protected Transform _muzzlePoint; // Точка вылета пули/луча
        [SerializeField] public Transform LeftHandGripPoint; // Для нашего IK

        [Header("Aiming Offsets (ADS)")]
        public Vector3 AimPositionOffset;
        public Vector3 AimRotationOffset;

        protected WeaponConfig _config;
        protected int _currentAmmoInClip;
        protected int _currentReserveAmmo;
        protected float _lastFireTime;

        public virtual void Initialize(WeaponConfig config)
        {
            _config = config;
            _currentAmmoInClip = config.AmmoPerClip;
            _currentReserveAmmo = config.MaxReserveAmmo;
        }

        // Этот метод дергает игрок или ИИ
        public void TryFire(Vector3 targetPoint)
        {
            // Проверка кулдауна (скорострельности)
            if (Time.time - _lastFireTime < _config.FireRate) return;

            // Проверка патронов
            if (_currentAmmoInClip <= 0)
            {
                PlayEmptySound();
                return;
            }

            _currentAmmoInClip--;
            _lastFireTime = Time.time;

            PlayFireVfx();
            PlayFireSound();

            // Вызываем специфичную механику выстрела у наследников!
            PerformFire(targetPoint); 
        }

        public virtual void Reload()
        {
            if (_currentAmmoInClip == _config.AmmoPerClip || _currentReserveAmmo <= 0)
            {
                Debug.Log("[WeaponBase] Перезарядка невозможна (полный магазин или нет запаса).");
                return;
            }

            int ammoNeeded = _config.AmmoPerClip - _currentAmmoInClip;
            int ammoToReload = Mathf.Min(ammoNeeded, _currentReserveAmmo);

            _currentAmmoInClip += ammoToReload;
            _currentReserveAmmo -= ammoToReload;

            Debug.Log($"[WeaponBase] Перезарядка! В магазине: {_currentAmmoInClip}, в запасе: {_currentReserveAmmo}");
        }

        public bool IsClipEmpty() => _currentAmmoInClip <= 0;

        // АБСТРАКТНЫЙ МЕТОД: Наследники обязаны реализовать механику урона
        protected abstract void PerformFire(Vector3 targetPoint);

        private void PlayFireVfx()
        {
            if (_config.MuzzleFlashPrefab != null && _muzzlePoint != null)
            {
                // Позже заменим на PoolManager.Spawn()
                Instantiate(_config.MuzzleFlashPrefab, _muzzlePoint.position, _muzzlePoint.rotation);
            }
        }

        private void PlayFireSound() { /* Заглушка для AudioSource */ }
        private void PlayEmptySound() { /* Заглушка для AudioSource */ }
    }
}