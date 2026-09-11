using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Gameplay.Infrastructure.Input
{
    public class StandaloneInputService : IInputService, IInitializable, IDisposable
    {
        private GameInput _gameInput;

        public event Action OnPrimaryAction;
        public event Action OnCancelAction;

        public Vector2 PointerPosition => _gameInput.Gameplay.PointerPosition.ReadValue<Vector2>();
        public bool IsPrimaryActionDown => _gameInput.Gameplay.PrimaryAction.WasPressedThisFrame();
        public bool IsCancelActionDown => _gameInput.Gameplay.CancelAction.WasPressedThisFrame();

        public bool IsDragPanning => _gameInput.Gameplay.MiddleClick.IsPressed();
        public Vector2 PointerDelta => _gameInput.Gameplay.PointerDelta.ReadValue<Vector2>();
        
        public Vector2 PanDelta => _gameInput.Gameplay.Move.ReadValue<Vector2>();
        public float ZoomDelta => _gameInput.Gameplay.Zoom.ReadValue<float>();

        public void Initialize()
        {
            _gameInput = new GameInput();

            // Подписываемся именованными методами
            _gameInput.Gameplay.PrimaryAction.performed += HandlePrimaryAction;
            _gameInput.Gameplay.CancelAction.performed += HandleCancelAction;

            _gameInput.Enable();
        }

        public void Dispose()
        {
            if (_gameInput != null)
            {
                // Честно отписываемся перед уничтожением
                _gameInput.Gameplay.PrimaryAction.performed -= HandlePrimaryAction;
                _gameInput.Gameplay.CancelAction.performed -= HandleCancelAction;

                _gameInput.Disable();
                _gameInput.Dispose();
                _gameInput = null;
            }
        }

        // Именованные обработчики
        private void HandlePrimaryAction(InputAction.CallbackContext context) => OnPrimaryAction?.Invoke();
        private void HandleCancelAction(InputAction.CallbackContext context) => OnCancelAction?.Invoke();
    }
}