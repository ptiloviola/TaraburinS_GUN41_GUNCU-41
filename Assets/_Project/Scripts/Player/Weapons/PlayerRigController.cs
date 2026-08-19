using UnityEngine;
using UnityEngine.Animations.Rigging;
using TpsShooter.Weapons.Core;

namespace TpsShooter.Player.Weapons
{
    public class PlayerRigController
    {
        private readonly Animator _animator;
        private readonly Transform _leftHandIkTarget;
        private readonly Rig _weaponRig;
        private readonly TwoBoneIKConstraint _leftHandIK;

        private static readonly int IsArmedHash = Animator.StringToHash("IsArmed");
        private static readonly int MeleePunchStateHash = Animator.StringToHash("MeleePunch");
        private static readonly int RollStateHash = Animator.StringToHash("Roll");
        
        private const int UpperBodyLayerIndex = 1;
        private const float RigLerpSpeed = 15f;

        public PlayerRigController(Animator animator, Transform leftHandIkTarget, Rig weaponRig, TwoBoneIKConstraint leftHandIK)
        {
            _animator = animator;
            _leftHandIkTarget = leftHandIkTarget;
            _weaponRig = weaponRig;
            _leftHandIK = leftHandIK;

            if (_weaponRig != null) _weaponRig.weight = 0f;
            if (_leftHandIK != null) _leftHandIK.weight = 0f;
        }

        public void SetArmedState(bool isArmed)
        {
            _animator.SetBool(IsArmedHash, isArmed);
        }

        public void Tick(float deltaTime, bool isArmed, bool isAiming, WeaponBase currentWeapon)
        {
            bool isMeleeing = _animator.GetCurrentAnimatorStateInfo(UpperBodyLayerIndex).shortNameHash == MeleePunchStateHash;
            bool isRolling = _animator.GetCurrentAnimatorStateInfo(0).shortNameHash == RollStateHash;

            float targetLayerWeight = (!isRolling) ? 1f : 0f;
            float targetRigWeight = (isArmed && isAiming && !isMeleeing && !isRolling) ? 1f : 0f;
            float leftHandTargetWeight = (isArmed && !isMeleeing && !isRolling) ? 1f : 0f;

            float currentLayerWeight = _animator.GetLayerWeight(UpperBodyLayerIndex);
            _animator.SetLayerWeight(UpperBodyLayerIndex, Mathf.Lerp(currentLayerWeight, targetLayerWeight, deltaTime * RigLerpSpeed));

            if (_weaponRig != null)
            {
                _weaponRig.weight = Mathf.Lerp(_weaponRig.weight, targetRigWeight, deltaTime * RigLerpSpeed);
            }

            if (isArmed && currentWeapon != null)
            {
                Transform weaponTransform = currentWeapon.transform;
                if (isAiming && !isMeleeing)
                {
                    weaponTransform.localPosition = Vector3.Lerp(weaponTransform.localPosition, currentWeapon.AimPositionOffset, deltaTime * RigLerpSpeed);
                    weaponTransform.localRotation = Quaternion.Slerp(weaponTransform.localRotation, Quaternion.Euler(currentWeapon.AimRotationOffset), deltaTime * RigLerpSpeed);
                }
                else
                {
                    weaponTransform.localPosition = Vector3.Lerp(weaponTransform.localPosition, Vector3.zero, deltaTime * RigLerpSpeed);
                    weaponTransform.localRotation = Quaternion.Slerp(weaponTransform.localRotation, Quaternion.identity, deltaTime * RigLerpSpeed);
                }
            }

            if (!isArmed || currentWeapon == null || currentWeapon.LeftHandGripPoint == null)
            {
                if (_leftHandIK != null) _leftHandIK.weight = Mathf.Lerp(_leftHandIK.weight, 0f, deltaTime * RigLerpSpeed);
                return;
            }

            if (_leftHandIkTarget != null)
            {
                _leftHandIkTarget.position = currentWeapon.LeftHandGripPoint.position;
                _leftHandIkTarget.rotation = currentWeapon.LeftHandGripPoint.rotation;
                
                if (_leftHandIK != null) 
                {
                    _leftHandIK.weight = Mathf.Lerp(_leftHandIK.weight, leftHandTargetWeight, deltaTime * RigLerpSpeed * 2f);
                }
            }
        }
    }
}