using MeatMushrooms.Player.Configs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MeatMushrooms.Player.Components
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerHealth), typeof(PlayerStealth))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerConfig _config;
        [SerializeField] private Animator _animator;

        private CharacterController _controller;
        private PlayerInput _playerInput;
        private PlayerHealth _health;
        private PlayerStealth _stealth;

        private InputAction _moveAction;
        private InputAction _runAction;
        
        private Transform _mainCamera;
        
        private float _currentSpeed;
        private float _turnSmoothVelocity;
        private float _verticalVelocity;
        private const float Gravity = -9.81f;
        private static readonly int SpeedHash = Animator.StringToHash("Speed");

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _playerInput = GetComponent<PlayerInput>();
            _health = GetComponent<PlayerHealth>();
            _stealth = GetComponent<PlayerStealth>();
            
            _moveAction = _playerInput.actions["Move"];
            _runAction = _playerInput.actions["Run"];
        }

        private void Start()
        {
            if (Camera.main != null)
            {
                _mainCamera = Camera.main.transform;
            }
        }

        private void Update()
        {
            if (_health.IsDead)
            {
                StopMovement();
                return;
            }

            HandleMovement();
        }

        private void HandleMovement()
        {
            Vector2 input = _moveAction.ReadValue<Vector2>();
            bool isRunning = _runAction.IsPressed();
            bool isMoving = input.magnitude >= 0.1f;

            _verticalVelocity = _controller.isGrounded ? -2f : _verticalVelocity + (Gravity * Time.deltaTime);
            Vector3 movement = Vector3.zero;

            if (isMoving)
            {
                Vector3 camForward = _mainCamera != null ? _mainCamera.forward : Vector3.forward;
                Vector3 camRight = _mainCamera != null ? _mainCamera.right : Vector3.right;
                
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();

                Vector3 direction = (camForward * input.y + camRight * input.x).normalized;

                float targetSpeed = isRunning ? _config.RunSpeed : _config.WalkSpeed;
                float animationSpeedValue = isRunning ? 1f : 0.5f;

                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _config.RotationSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                movement = moveDir.normalized * targetSpeed;

                _currentSpeed = Mathf.Lerp(_currentSpeed, animationSpeedValue, Time.deltaTime * 10f);
            }
            else
            {
                _currentSpeed = Mathf.Lerp(_currentSpeed, 0f, Time.deltaTime * 10f);
            }

            _stealth.UpdateNoiseLevel(isMoving, isRunning);
            _animator.SetFloat(SpeedHash, _currentSpeed);

            movement.y = _verticalVelocity;
            _controller.Move(movement * Time.deltaTime);
        }

        private void StopMovement()
        {
            _currentSpeed = 0f;
            _animator.SetFloat(SpeedHash, 0f);
        }
    }
}