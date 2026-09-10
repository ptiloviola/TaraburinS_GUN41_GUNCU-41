using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Gameplay.Infrastructure.Input
{
    public class StandaloneInputService : IInputService, IInitializable, IDisposable
    {
        private InputAction _primaryAction;
        private InputAction _cancelAction;
        private InputAction _pointerPosition;

        public event Action OnPrimaryAction;
        public event Action OnCancelAction;

        public Vector2 PointerPosition => _pointerPosition.ReadValue<Vector2>();
        public bool IsPrimaryActionDown => _primaryAction.WasPressedThisFrame();
        public bool IsCancelActionDown => _cancelAction.WasPressedThisFrame();

        public void Initialize()
        {
            // Настраиваем ЛКМ и касание экрана (Touch)
            _primaryAction = new InputAction(binding: "<Mouse>/leftButton");
            _primaryAction.AddBinding("<Pointer>/press"); // Магия: сразу работает на мобилках!

            // Настраиваем ПКМ и Escape
            _cancelAction = new InputAction(binding: "<Mouse>/rightButton");
            _cancelAction.AddBinding("<Keyboard>/escape");

            // Считываем позицию мыши или пальца
            _pointerPosition = new InputAction(binding: "<Pointer>/position");

            // Реактивная подписка на события
            _primaryAction.performed += _ => OnPrimaryAction?.Invoke();
            _cancelAction.performed += _ => OnCancelAction?.Invoke();

            // Включаем слежение
            _primaryAction.Enable();
            _cancelAction.Enable();
            _pointerPosition.Enable();
        }

        public void Dispose()
        {
            // Защита от утечек памяти при закрытии игры/сцены
            _primaryAction.Disable();
            _cancelAction.Disable();
            _pointerPosition.Disable();
            
            _primaryAction.Dispose();
            _cancelAction.Dispose();
            _pointerPosition.Dispose();
        }
    }
}