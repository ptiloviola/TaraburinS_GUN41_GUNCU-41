using MeatMushrooms.Player.Configs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MeatMushrooms.Player.Components
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerConfig _config;
        [SerializeField] private Animator _animator;
        [SerializeField] private SphereCollider _noiseRadar;

        private CharacterController _controller;
        private InputAction _moveAction;
        private InputAction _runAction;
        private PlayerInput _playerInput;

        private float _currentSpeed;
        private float _turnSmoothVelocity;
        private Transform _mainCamera;

        private float _verticalVelocity;
        private const float Gravity = -9.81f;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _playerInput = GetComponent<PlayerInput>();
            
            _moveAction = _playerInput.actions["Move"];
            _runAction = _playerInput.actions["Run"];
        }

        private void Start()
        {
            // Если камеры нет, Unity может ругнуться, лучше перестраховаться
            if (Camera.main != null)
            {
                _mainCamera = Camera.main.transform;
            }
        }

        private void Update()
        {
            HandleMovementAndNoise();
        }

        private void HandleMovementAndNoise()
        {
            Vector2 input = _moveAction.ReadValue<Vector2>();
            bool isRunning = _runAction.IsPressed();

            // 1. СЧИТАЕМ ГРАВИТАЦИЮ (Работает всегда!)
            if (_controller.isGrounded)
            {
                // Прижимаем к земле, чтобы не скакала на кочках
                _verticalVelocity = -2f; 
            }
            else
            {
                // Падение
                _verticalVelocity += Gravity * Time.deltaTime; 
            }

            // Вектор, который мы в итоге передадим контроллеру
            Vector3 movement = Vector3.zero;

            // 2. СЧИТАЕМ ГОРИЗОНТАЛЬНОЕ ДВИЖЕНИЕ (Только если жмем кнопки)
            if (input.magnitude >= 0.1f)
            {
                Vector3 camForward = _mainCamera != null ? _mainCamera.forward : Vector3.forward;
                Vector3 camRight = _mainCamera != null ? _mainCamera.right : Vector3.right;
                
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();

                Vector3 direction = (camForward * input.y + camRight * input.x).normalized;

                float targetSpeed = isRunning ? _config.RunSpeed : _config.WalkSpeed;
                float targetNoise = isRunning ? _config.RunNoise : _config.WalkNoise;
                float animationSpeedValue = isRunning ? 1f : 0.5f;

                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _config.RotationSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                
                // Записываем горизонтальную скорость
                movement = moveDir.normalized * targetSpeed;

                _currentSpeed = Mathf.Lerp(_currentSpeed, animationSpeedValue, Time.deltaTime * 10f);
                _noiseRadar.radius = targetNoise;
            }
            else
            {
                _currentSpeed = Mathf.Lerp(_currentSpeed, 0f, Time.deltaTime * 10f);
                _noiseRadar.radius = _config.IdleNoise;
            }

            _animator.SetFloat(SpeedHash, _currentSpeed);

            // 3. ОБЪЕДИНЯЕМ ОСИ И ДВИГАЕМ КАПСУЛУ
            movement.y = _verticalVelocity; // Добавляем гравитацию к нашему XZ движению
            _controller.Move(movement * Time.deltaTime); // ЕДИНСТВЕННЫЙ вызов Move за кадр!
        }

        // --- МЕТОДЫ ДЛЯ ЗВУКОВ ШАГОВ ---
        
        public void PlayFootstepWalkSound()
        {
            Debug.Log("[Player] Тихий шаг");
        }

        public void PlayFootstepRunSound()
        {
            Debug.Log("[Player] ГРОМКИЙ БЕГ!");
        }
    }
}