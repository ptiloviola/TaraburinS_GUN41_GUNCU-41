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
        
        private static readonly int IsArmedHash = Animator.StringToHash("IsArmed");

        public WeaponBase CurrentWeapon { get; private set; }
        public bool IsArmed { get; private set; } = false; 

        public PlayerWeaponController(Transform handSocket, Transform backSocket1, 
            Transform backSocket2, Animator animator, Transform leftHandIkTarget, Rig weaponRig)
        {
            _handSocket = handSocket;
            _backSocket1 = backSocket1;
            _backSocket2 = backSocket2;
            _animator = animator;
            _leftHandIkTarget = leftHandIkTarget;
            _weaponRig = weaponRig;
            
            _animator.SetBool(IsArmedHash, IsArmed);
            if (_weaponRig != null) _weaponRig.weight = 0f;
        }

        public void OnWeaponEquipped(WeaponBase newWeapon)
        {
            CurrentWeapon = newWeapon;
            IsArmed = true;
            _animator.SetBool(IsArmedHash, IsArmed);
            
            // Больше никаких SetParent! Мы просто запомнили текущую пушку.
        }

        // НОВЫЙ МЕТОД: Автономное управление руками
        public void Tick(float deltaTime)
        {
            // Если без оружия — плавно выключаем Риг
            if (!IsArmed || CurrentWeapon == null)
            {
                if (_weaponRig != null) 
                    _weaponRig.weight = Mathf.Lerp(_weaponRig.weight, 0f, deltaTime * 10f);
                return;
            }

            // Если у пушки есть точка хвата (винтовка, дробовик)
            if (CurrentWeapon.LeftHandGripPoint != null && _leftHandIkTarget != null)
            {
                // 1. Математически привязываем таргет к цевью (без изменения иерархии)
                _leftHandIkTarget.position = CurrentWeapon.LeftHandGripPoint.position;
                _leftHandIkTarget.rotation = CurrentWeapon.LeftHandGripPoint.rotation;

                // 2. Плавно включаем IK. Никакие стейты больше не могут его сбить!
                if (_weaponRig != null) 
                    _weaponRig.weight = Mathf.Lerp(_weaponRig.weight, 1f, deltaTime * 10f);
            }
            else
            {
                // Если хвата нет (пистолет) — выключаем левую руку
                if (_weaponRig != null) 
                    _weaponRig.weight = Mathf.Lerp(_weaponRig.weight, 0f, deltaTime * 10f);
            }
        }
    }
}