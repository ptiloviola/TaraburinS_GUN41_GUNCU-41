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

        public event Action OnJump;
        public event Action OnReload;
        public event Action OnMelee;

        public void Initialize()
        {
            _input = new PlayerInputActions();
            _input.Player.Enable();

            _input.Player.Jump.performed += _ => OnJump?.Invoke();
            _input.Player.Reload.performed += _ => OnReload?.Invoke();
            _input.Player.Melee.performed += _ => OnMelee?.Invoke();
        }

        public void Dispose()
        {
            _input.Player.Disable();
            _input.Player.Jump.performed -= _ => OnJump?.Invoke();
            _input.Player.Reload.performed -= _ => OnReload?.Invoke();
            _input.Player.Melee.performed -= _ => OnMelee?.Invoke();
        }
    }
}