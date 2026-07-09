using UnityEngine;
using UnityEngine.InputSystem;
using Infrastructure.Interfaces;
using System;
namespace Player.Input
{
    using UnityEngine;
    using UnityEngine.InputSystem;
    using Infrastructure.Interfaces;
    using System;

    public class PlayerInputHandler : MonoBehaviour, IInputProvider
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference lookAction;
        [SerializeField] private InputActionReference aimAction;
        [SerializeField] private InputActionReference fireAction;
        [SerializeField] private InputActionReference reloadAction;

        public Vector2 MoveInput => moveAction.action.ReadValue<Vector2>();
        public Vector2 LookInput => lookAction.action.ReadValue<Vector2>();

        public event Action OnFireStarted;
        public event Action OnReloadStarted;
        public event Action<bool> OnAimChanged;

        private void OnEnable()
        {
            moveAction.action.Enable();
            lookAction.action.Enable();
            aimAction.action.Enable();
            fireAction.action.Enable();
            reloadAction.action.Enable();

            // Транслируем события Unity Input System в наши чистые C# события
            fireAction.action.started += _ => OnFireStarted?.Invoke();
            reloadAction.action.started += _ => OnReloadStarted?.Invoke();
            aimAction.action.started += _ => OnAimChanged?.Invoke(true);
            aimAction.action.canceled += _ => OnAimChanged?.Invoke(false);
        }

        private void OnDisable()
        {
            moveAction.action.Disable();
            lookAction.action.Disable();
            aimAction.action.Disable();
            fireAction.action.Disable();
            reloadAction.action.Disable();

            fireAction.action.started -= _ => OnFireStarted?.Invoke();
            reloadAction.action.started -= _ => OnReloadStarted?.Invoke();
            aimAction.action.started -= _ => OnAimChanged?.Invoke(true);
            aimAction.action.canceled -= _ => OnAimChanged?.Invoke(false);
        }
    }
}