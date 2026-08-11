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
        private static readonly int MeleePunchStateHash = Animator.StringToHash("MeleePunch");
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
                Vector3 finalTargetPosition = _cameraTransform.position + _cameraTransform.forward * 50f;

                if (IsArmed && CurrentWeapon != null && CurrentWeapon.Config != null)
                {
                    if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit camHit, 100f, CurrentWeapon.Config.HitMask))
                    {
                        finalTargetPosition = camHit.point; 
                    }
                }
                _aimTarget.position = finalTargetPosition;
            }

            bool isFiringNow = _inputService != null && _inputService.IsFiring;
            bool isTriggerPulled = isFiringNow && !_wasFiring; 
            _wasFiring = isFiringNow; 

            if (IsArmed && CurrentWeapon != null && CurrentWeapon.Config != null)
            {
                bool canFire = CurrentWeapon.Config.IsAutomatic ? isFiringNow : isTriggerPulled;
                if (canFire)
                {
                    CurrentWeapon.TryFire(_aimTarget.position);
                }
            }

            // --- 2. ВЫЧИСЛЕНИЕ ВЕСОВ (ИСПРАВЛЕНО ДЛЯ MELEE) ---
            
            // Проверяем, проигрывается ли сейчас анимация удара (по нашему хэшу)
            bool isMeleeing = _animator.GetCurrentAnimatorStateInfo(UpperBodyLayerIndex).shortNameHash == MeleePunchStateHash;

            // Вес слоя: 1, если целимся ИЛИ если бьем
            float targetLayerWeight = (IsArmed && IsAiming) || isMeleeing ? 1f : 0f;
            
            // Вес прицеливания (для спины и рук): 1, только если целимся
            float targetAimWeight = (IsArmed && IsAiming) ? 1f : 0f;
            
            float lerpSpeed = deltaTime * 15f;

            // 3. Слои Аниматора (теперь слой включается во время удара!)
            float currentLayerWeight = _animator.GetLayerWeight(UpperBodyLayerIndex);
            _animator.SetLayerWeight(UpperBodyLayerIndex, Mathf.Lerp(currentLayerWeight, targetLayerWeight, lerpSpeed));

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
                {
                    // Если бьем - отпускаем левую руку (вес 0)
                    float leftHandTargetWeight = (targetAimWeight > 0f && !isMeleeing) ? 1f : 0f;
                    _leftHandIK.weight = Mathf.Lerp(_leftHandIK.weight, leftHandTargetWeight, lerpSpeed * 2f);
                }
            }
        }
    }
}