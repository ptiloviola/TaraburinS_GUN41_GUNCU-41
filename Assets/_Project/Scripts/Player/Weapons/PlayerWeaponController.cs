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
                // Точка прицела всегда в 50 метрах по центру экрана, в любом стейте
                _aimTarget.position = _cameraTransform.position + _cameraTransform.forward * 50f;
            }

            // Стрельба (убедись, что переменная в IInputService называется IsFiring)
            if (IsArmed && CurrentWeapon != null && _inputService != null)
            {
                if (_inputService.IsFiring)
                {
                    Debug.Log("<color=green>[INPUT]</color> Кнопка стрельбы зажата!"); // <--- Добавили лог
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