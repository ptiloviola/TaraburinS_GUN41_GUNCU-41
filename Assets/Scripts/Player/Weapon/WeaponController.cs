using UnityEngine;
using Infrastructure.Interfaces;
using Services;
using DG.Tweening;
namespace Player.Weapon
{
    public class WeaponController
    {
        private readonly IWeaponView _view;
        private readonly ICrosshairView _crosshair;
        private readonly IShooter _shooter;
        private readonly IImpactHandler _impactHandler;
        private readonly AudioService _audioService;
        private readonly PlayerConfig _config;
        private readonly AudioSource _audioSource;
        private readonly Transform _firePoint;

        private int _currentAmmo;
        private float _lastFireTime;
        private bool _isAiming;
        private bool _isReloading;

        public WeaponController(
            IWeaponView view, 
            ICrosshairView crosshair, 
            IShooter shooter, 
            IImpactHandler impactHandler, 
            AudioService audioService, 
            AudioSource audioSource,
            Transform firePoint,
            PlayerConfig config)
        {
            _view = view;
            _crosshair = crosshair;
            _shooter = shooter;
            _impactHandler = impactHandler;
            _audioService = audioService;
            _audioSource = audioSource;
            _firePoint = firePoint;
            _config = config;

            _currentAmmo = _config.maxAmmo;
        }

        public void PlayBobbing(bool isMoving) => _view.PlayBobbing(isMoving, _isAiming);

        public void ToggleAim(bool isAiming)
        {
            _isAiming = isAiming;
            _view.SetAimState(_isAiming);
            _crosshair.SetAimState(_isAiming);
        }

        public void Fire()
        {
            if (_isReloading || Time.time - _lastFireTime < _config.fireRate) return;
            if (_currentAmmo <= 0) return;

            _lastFireTime = Time.time;
            _currentAmmo--;

            _audioService.PlaySound(_audioSource, _config.fireSound, randomizePitch: true);
            _view.PlayFireAnimation();

            float currentSpread = _isAiming ? _config.aimSpread : _config.hipSpread;
            
            // Считаем физику
            _shooter.TryShot(currentSpread, _config.shootMask, out RaycastHit hit, out Vector3 targetPoint);
            
            // Передаем результат фабрике эффектов
            _impactHandler.HandleImpact(_firePoint.position, targetPoint, hit.collider, hit.normal);
        }

        public void Reload()
        {
            if (_isReloading || _currentAmmo == _config.maxAmmo) return;

            _isReloading = true;
            ToggleAim(false);

            _audioService.PlaySound(_audioSource, _config.reloadSound, randomizePitch: false);
            _view.PlayReloadAnimation(_config.reloadDuration);

            DOVirtual.DelayedCall(_config.reloadDuration, () =>
            {
                _currentAmmo = _config.maxAmmo;
                _isReloading = false;
            }, false);
        }
    }
}