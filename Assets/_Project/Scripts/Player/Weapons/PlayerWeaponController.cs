using UnityEngine;
using UnityEngine.Animations.Rigging;
using TpsShooter.Weapons.Core;
using TpsShooter.Services.Input; // Обязательно подключаем инпут

namespace TpsShooter.Player.Weapons
{
    public class PlayerWeaponController
    {
        private readonly Transform _handSocket;
        private readonly Transform _backSocket1;
        private readonly Transform _backSocket2;
        private readonly Animator _animator;
        private readonly Transform _leftHandIkTarget;
        private readonly Rig _weaponRig;
        private readonly TwoBoneIKConstraint _leftHandIK;
        
        // Новые зависимости для стрельбы
        private readonly IInputService _inputService;
        private readonly Transform _cameraTransform;
        private readonly Transform _aimTarget;
        
        private static readonly int IsArmedHash = Animator.StringToHash("IsArmed");
        private const int UpperBodyLayerIndex = 1;

        public WeaponBase CurrentWeapon { get; private set; }
        public bool IsArmed { get; private set; } = false; 
        public bool IsAiming { get; private set; } = false;

        private bool _wasFiring = false;

        public PlayerWeaponController(
            Transform handSocket, 
            Transform backSocket1, 
            Transform backSocket2, 
            Animator animator, 
            Transform leftHandIkTarget, 
            Rig weaponRig, 
            TwoBoneIKConstraint leftHandIK,
            IInputService inputService,       // + Инпут
            Transform cameraTransform,        // + Камера
            Transform aimTarget)              // + Таргет
        {
            _handSocket = handSocket;
            _backSocket1 = backSocket1;
            _backSocket2 = backSocket2;
            _animator = animator;
            _leftHandIkTarget = leftHandIkTarget;
            _weaponRig = weaponRig;
            _leftHandIK = leftHandIK;
            
            _inputService = inputService;
            _cameraTransform = cameraTransform;
            _aimTarget = aimTarget;
            
            _animator.SetBool(IsArmedHash, IsArmed);
            
            if (_weaponRig != null) _weaponRig.weight = 0f;
            if (_leftHandIK != null) _leftHandIK.weight = 0f;
        }

        public void OnWeaponEquipped(WeaponBase newWeapon)
        {
            CurrentWeapon = newWeapon;
            IsArmed = true;
            _animator.SetBool(IsArmedHash, IsArmed);
        }

        public void SetAiming(bool isAiming)
        {
            IsAiming = isAiming;
        }

        public void Tick(float deltaTime)
        {
            // --- 1. ГЛОБАЛЬНЫЙ ПРИЦЕЛ И СТРЕЛЬБА ---
            if (_cameraTransform != null && _aimTarget != null)
            {
                // По умолчанию цель далеко впереди
                Vector3 finalTargetPosition = _cameraTransform.position + _cameraTransform.forward * 50f;

                // УМНОЕ ПРИЦЕЛИВАНИЕ ИЗ КАМЕРЫ:
                // Если мы вооружены, пускаем луч из камеры, чтобы узнать, на что реально смотрит крестик
                if (IsArmed && CurrentWeapon != null && CurrentWeapon.Config != null)
                {
                    if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit camHit, 100f, CurrentWeapon.Config.HitMask))
                    {
                        finalTargetPosition = camHit.point; // Крестик смотрит прямо на объект!
                    }
                }

                _aimTarget.position = finalTargetPosition;
            }

            // Обработка инпута (Edge Detection)
            bool isFiringNow = _inputService != null && _inputService.IsFiring;
            bool isTriggerPulled = isFiringNow && !_wasFiring; // Срабатывает только в 1-й кадр клика
            _wasFiring = isFiringNow; // Запоминаем для следующего кадра

            if (IsArmed && CurrentWeapon != null && CurrentWeapon.Config != null)
            {
                // Если автомат - реагируем на удержание (isFiringNow)
                // Если пистолет - только на свежий клик (isTriggerPulled)
                bool canFire = CurrentWeapon.Config.IsAutomatic ? isFiringNow : isTriggerPulled;
                
                if (canFire)
                {
                    CurrentWeapon.TryFire(_aimTarget.position);
                }
            }
            // ---------------------------------------

            // 2. Вычисляем целевой вес для анимаций прицеливания
            float targetAimWeight = (IsArmed && IsAiming) ? 1f : 0f;
            float lerpSpeed = deltaTime * 15f;

            // 3. Слои Аниматора
            float currentLayerWeight = _animator.GetLayerWeight(UpperBodyLayerIndex);
            _animator.SetLayerWeight(UpperBodyLayerIndex, Mathf.Lerp(currentLayerWeight, targetAimWeight, lerpSpeed));

            // 4. Поворот спины
            if (_weaponRig != null) 
            {
                _weaponRig.weight = Mathf.Lerp(_weaponRig.weight, targetAimWeight, lerpSpeed);
            }

            // 5. Смещение оружия (ADS OFFSET)
            if (IsArmed && CurrentWeapon != null)
            {
                Transform weaponTransform = CurrentWeapon.transform;
                
                if (IsAiming)
                {
                    weaponTransform.localPosition = Vector3.Lerp(weaponTransform.localPosition, CurrentWeapon.AimPositionOffset, lerpSpeed);
                    weaponTransform.localRotation = Quaternion.Slerp(weaponTransform.localRotation, Quaternion.Euler(CurrentWeapon.AimRotationOffset), lerpSpeed);
                }
                else
                {
                    weaponTransform.localPosition = Vector3.Lerp(weaponTransform.localPosition, Vector3.zero, lerpSpeed);
                    weaponTransform.localRotation = Quaternion.Slerp(weaponTransform.localRotation, Quaternion.identity, lerpSpeed);
                }
            }

            // 6. Левая рука
            if (!IsArmed || CurrentWeapon == null || CurrentWeapon.LeftHandGripPoint == null)
            {
                if (_leftHandIK != null) _leftHandIK.weight = Mathf.Lerp(_leftHandIK.weight, 0f, lerpSpeed);
                return;
            }

            if (_leftHandIkTarget != null)
            {
                _leftHandIkTarget.position = CurrentWeapon.LeftHandGripPoint.position;
                _leftHandIkTarget.rotation = CurrentWeapon.LeftHandGripPoint.rotation;
                
                if (_leftHandIK != null) 
                    _leftHandIK.weight = Mathf.Lerp(_leftHandIK.weight, targetAimWeight, lerpSpeed);
            }
        }
    }
}