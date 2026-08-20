using System;
using UnityEngine;
using Zenject;

namespace TpsShooter.Services.Input
{
    public class UnityInputService : IInputService, IInitializable, IDisposable
    {
        private PlayerInputActions _input;

        public Vector2 MoveAxis => _input.Player.Move.ReadValue<Vector2>();
        public Vector2 LookAxis => _input.Player.Look.ReadValue<Vector2>();
        public bool IsFiring => _input.Player.Fire.IsPressed();
        public bool IsAiming => _input.Player.Aim.IsPressed();
        public bool IsRunning => _input.Player.Run.IsPressed();
        public bool IsCrouching => _input.Player.Crouch.IsPressed();
        public bool IsRollTriggered => _input.Player.Roll.IsPressed();


        public event Action<int> OnWeaponSelect;
        public event Action<int> OnWeaponScroll;
        public event Action OnDropWeapon;

        public event Action OnJump;
        public event Action OnReload;
        public event Action OnMelee;

        public void Initialize()
        {
            _input = new PlayerInputActions();
            _input.Player.Enable();


            _input.Player.Jump.performed += OnJumpPerformed;
            _input.Player.Reload.performed += OnReloadPerformed;
            _input.Player.Melee.performed += OnMeleePerformed;


            _input.Player.Weapon1.performed += OnWeapon1Performed;
            _input.Player.Weapon2.performed += OnWeapon2Performed;
            _input.Player.ScrollWeapon.performed += OnScrollPerformed;
            

            _input.Player.DropWeapon.performed += OnDropWeaponPerformed;
        }

        public void Dispose()
        {
            _input.Player.Disable();


            _input.Player.Jump.performed -= OnJumpPerformed;
            _input.Player.Reload.performed -= OnReloadPerformed;
            _input.Player.Melee.performed -= OnMeleePerformed;

            _input.Player.Weapon1.performed -= OnWeapon1Performed;
            _input.Player.Weapon2.performed -= OnWeapon2Performed;
            _input.Player.ScrollWeapon.performed -= OnScrollPerformed;
            
            _input.Player.DropWeapon.performed -= OnDropWeaponPerformed;
            
            _input.Dispose();
        }

        private void OnJumpPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => OnJump?.Invoke();
        private void OnReloadPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => OnReload?.Invoke();
        private void OnMeleePerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => OnMelee?.Invoke();

        private void OnWeapon1Performed(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => OnWeaponSelect?.Invoke(0);
        private void OnWeapon2Performed(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => OnWeaponSelect?.Invoke(1);

        private void OnScrollPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            float scrollY = ctx.ReadValue<Vector2>().y;
            if (scrollY > 0) OnWeaponScroll?.Invoke(1);
            else if (scrollY < 0) OnWeaponScroll?.Invoke(-1);
        }

        private void OnDropWeaponPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => OnDropWeapon?.Invoke();
    }
}