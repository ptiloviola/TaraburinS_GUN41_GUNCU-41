using System;
using UnityEngine;
using Zenject;

namespace Gameplay.Infrastructure.Input
{
    public class StandaloneInputService : IInputService, IInitializable, IDisposable
    {
        // Ссылка на наш автосгенерированный класс настроек ввода
        private GameInput _gameInput;

        public event Action OnPrimaryAction;
        public event Action OnCancelAction;

        // Читаем данные напрямую из сгенерированного класса
        public Vector2 PointerPosition => _gameInput.Gameplay.PointerPosition.ReadValue<Vector2>();
        public bool IsPrimaryActionDown => _gameInput.Gameplay.PrimaryAction.WasPressedThisFrame();
        public bool IsCancelActionDown => _gameInput.Gameplay.CancelAction.WasPressedThisFrame();

        // Метод IsPressed() отлично подходит для проверки удержания кнопки (Drag)
        public bool IsDragPanning => _gameInput.Gameplay.MiddleClick.IsPressed();
        public Vector2 PointerDelta => _gameInput.Gameplay.PointerDelta.ReadValue<Vector2>();
        
        // Новые данные для камеры
        public Vector2 PanDelta => _gameInput.Gameplay.Move.ReadValue<Vector2>();
        public float ZoomDelta => _gameInput.Gameplay.Zoom.ReadValue<float>();

        

        public void Initialize()
        {
            // Создаем экземпляр настроек
            _gameInput = new GameInput();

            // Подписываемся на события кликов
            _gameInput.Gameplay.PrimaryAction.performed += _ => OnPrimaryAction?.Invoke();
            _gameInput.Gameplay.CancelAction.performed += _ => OnCancelAction?.Invoke();

            // Включаем слежение
            _gameInput.Enable();
        }

        public void Dispose()
        {
            // Корректно очищаем память при выходе
            if (_gameInput != null)
            {
                _gameInput.Disable();
                _gameInput.Dispose();
            }
        }
    }
}