using UnityEngine;
using TpsShooter.Weapons.Core;
using TpsShooter.Services.Input;

namespace TpsShooter.Player.Weapons
{
    public class PlayerWeaponController
    {
        private readonly IInputService _inputService;
        private readonly Transform _cameraTransform;
        private readonly Transform _aimTarget;
        private readonly PlayerRigController _rigController;

        private const float AimTargetDistance = 50f;
        private const float MaxRaycastDistance = 100f;

        public WeaponBase CurrentWeapon { get; private set; }
        public bool IsArmed { get; private set; } = false; 
        public bool IsAiming { get; private set; } = false;

        private bool _wasFiring = false;

        public PlayerWeaponController(
            IInputService inputService, 
            Transform cameraTransform, 
            Transform aimTarget, 
            PlayerRigController rigController)
        {
            _inputService = inputService;
            _cameraTransform = cameraTransform;
            _aimTarget = aimTarget;
            _rigController = rigController;
        }

        public void OnWeaponEquipped(WeaponBase newWeapon)
        {
            CurrentWeapon = newWeapon;
            IsArmed = newWeapon != null;
            
            _rigController.SetArmedState(IsArmed);
        }

        public void SetAiming(bool isAiming)
        {
            IsAiming = isAiming;
        }

        public void Tick(float deltaTime)
        {
            HandleShooting();
            _rigController.Tick(deltaTime, IsArmed, IsAiming, CurrentWeapon);
        }

        private void HandleShooting()
        {
            if (_cameraTransform == null || _aimTarget == null) return;

            _aimTarget.position = _cameraTransform.position + _cameraTransform.forward * AimTargetDistance;
            Vector3 shootTargetPosition = _aimTarget.position;

            if (IsArmed && CurrentWeapon != null && CurrentWeapon.Config != null)
            {
                int safeMask = CurrentWeapon.Config.HitMask & ~LayerMask.GetMask("Player", "Ignore Raycast");

                if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit camHit, MaxRaycastDistance, safeMask))
                {
                    shootTargetPosition = camHit.point; 
                }
            }

            bool isFiringNow = _inputService != null && _inputService.IsFiring;
            bool isTriggerPulled = isFiringNow && !_wasFiring; 
            _wasFiring = isFiringNow; 

            if (IsArmed && CurrentWeapon != null && CurrentWeapon.Config != null)
            {
                bool canFire = CurrentWeapon.Config.IsAutomatic ? isFiringNow : isTriggerPulled;
                if (canFire)
                {
                    CurrentWeapon.TryFire(shootTargetPosition);
                }
            }
        }
    }
}