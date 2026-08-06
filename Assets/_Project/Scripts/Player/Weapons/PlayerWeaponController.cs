using UnityEngine;
using UnityEngine.Animations.Rigging;
using TpsShooter.Weapons.Core;

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
        
        private static readonly int IsArmedHash = Animator.StringToHash("IsArmed");
        private const int UpperBodyLayerIndex = 1; // Индекс слоя прицеливания в Аниматоре

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
            TwoBoneIKConstraint leftHandIK) 
        {
            _handSocket = handSocket;
            _backSocket1 = backSocket1;
            _backSocket2 = backSocket2;
            _animator = animator;
            _leftHandIkTarget = leftHandIkTarget;
            _weaponRig = weaponRig;
            _leftHandIK = leftHandIK;
            
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
            // 1. Вычисляем целевой вес
            float targetAimWeight = (IsArmed && IsAiming) ? 1f : 0f;
            float lerpSpeed = deltaTime * 15f;

            // 2. Слои Аниматора
            float currentLayerWeight = _animator.GetLayerWeight(UpperBodyLayerIndex);
            _animator.SetLayerWeight(UpperBodyLayerIndex, Mathf.Lerp(currentLayerWeight, targetAimWeight, lerpSpeed));

            // 3. Поворот спины
            if (_weaponRig != null) 
            {
                _weaponRig.weight = Mathf.Lerp(_weaponRig.weight, targetAimWeight, lerpSpeed);
            }

            // --- НОВЫЙ БЛОК: СМЕЩЕНИЕ ОРУЖИЯ (ADS OFFSET) ---
            if (IsArmed && CurrentWeapon != null)
            {
                Transform weaponTransform = CurrentWeapon.transform;
                
                if (IsAiming)
                {
                    // Плавно сдвигаем к заданным координатам прицела
                    weaponTransform.localPosition = Vector3.Lerp(weaponTransform.localPosition, CurrentWeapon.AimPositionOffset, lerpSpeed);
                    weaponTransform.localRotation = Quaternion.Slerp(weaponTransform.localRotation, Quaternion.Euler(CurrentWeapon.AimRotationOffset), lerpSpeed);
                }
                else
                {
                    // Плавно возвращаем в нули (стандартное положение в руке)
                    weaponTransform.localPosition = Vector3.Lerp(weaponTransform.localPosition, Vector3.zero, lerpSpeed);
                    weaponTransform.localRotation = Quaternion.Slerp(weaponTransform.localRotation, Quaternion.identity, lerpSpeed);
                }
            }
            // ------------------------------------------------

            // 4. Левая рука (только в режиме прицеливания)
            if (!IsArmed || CurrentWeapon == null || CurrentWeapon.LeftHandGripPoint == null)
            {
                if (_leftHandIK != null) _leftHandIK.weight = Mathf.Lerp(_leftHandIK.weight, 0f, lerpSpeed);
                return;
            }

            // Магнитим таргет к точке на пушке каждый кадр
            if (_leftHandIkTarget != null)
            {
                _leftHandIkTarget.position = CurrentWeapon.LeftHandGripPoint.position;
                _leftHandIkTarget.rotation = CurrentWeapon.LeftHandGripPoint.rotation;
                
                // ВАЖНО: Включаем вес IK только когда целимся (targetAimWeight)
                if (_leftHandIK != null) 
                    _leftHandIK.weight = Mathf.Lerp(_leftHandIK.weight, targetAimWeight, lerpSpeed);
            }
        }
    }
}