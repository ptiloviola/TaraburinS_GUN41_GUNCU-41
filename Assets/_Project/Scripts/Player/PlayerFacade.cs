using UnityEngine;
using Zenject;
using TpsShooter.Services.Input;
using TpsShooter.Player.States;
using TpsShooter.Player.Core;
using TpsShooter.Player.Configs;
using TpsShooter.Player.Camera;
using TpsShooter.Player.Weapons;
using UnityEngine.Animations.Rigging;

namespace TpsShooter.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerFacade : MonoBehaviour
    {
        private PlayerStateMachine _stateMachine;
        private PlayerContext _context;
        private IInputService _inputService;
        
        [Header("Camera Settings")]
        [SerializeField] private Cinemachine.CinemachineFreeLook _normalCamera;
        [SerializeField] private Transform _cameraTarget;
        
        [Header("Weapon & Rigging Settings")]
        [SerializeField] private Transform _aimTarget;
        [SerializeField] private UnityEngine.Animations.Rigging.Rig _weaponRig;
        [SerializeField] private TwoBoneIKConstraint _leftHandIK;
        
        [SerializeField] private Transform _weaponHandSocket;
        [SerializeField] private Transform _weaponBackSocket1;
        [SerializeField] private Transform _weaponBackSocket2;
        [SerializeField] private Transform _leftHandIkTarget;

        private PlayerCameraController _cameraController;
        private PlayerWeaponController _weaponController;
        private WeaponInventory _weaponInventory;
        
        // Добавили поле для нашего нового сенсора
        private PlayerInteractionSensor _interactionSensor;

        // Добавляем публичный геттер в PlayerFacade:
        public float CurrentWeaponSpread 
        {
            get 
            {
                if (_weaponController != null && _weaponController.CurrentWeapon != null)
                    return _weaponController.CurrentWeapon.CurrentSpread;
                return 0f;
            }
        }

        [Inject]
        public void Construct(IInputService inputService, PlayerConfig config, IInstantiator instantiator)
        {
            _inputService = inputService;
            Transform camTransform = UnityEngine.Camera.main != null ? UnityEngine.Camera.main.transform : null;
            var groundSensor = new GroundSensor(transform, config);
            Animator animator = GetComponentInChildren<Animator>();
            
            _cameraController = new PlayerCameraController(config, _cameraTarget, _normalCamera);
            
            // Контроллер теперь принимает ровно 7 аргументов
            _weaponController = new PlayerWeaponController(
                _weaponHandSocket, 
                _weaponBackSocket1, 
                _weaponBackSocket2, 
                animator, 
                _leftHandIkTarget,
                _weaponRig,
                _leftHandIK,
                inputService,        // Передаем инпут
                camTransform,        // Передаем камеру
                _aimTarget          // Передаем таргет
            );

            // 1. Создаем сервис анимации переходов (передаем this, так как Фасад - это MonoBehaviour)
            WeaponTransitionService transitionService = new WeaponTransitionService(this);

            // 2. Создаем Инвентарь, прокидываем в него сервис переходов
            _weaponInventory = new WeaponInventory(
                instantiator,
                _weaponController,
                transitionService,
                _weaponHandSocket,
                _weaponBackSocket1,
                _weaponBackSocket2
            );

            // 3. Создаем сенсор подбора (чистый C#-класс)
            _interactionSensor = new PlayerInteractionSensor(transform, _weaponInventory);

            _context = new PlayerContext(
                GetComponent<CharacterController>(),
                transform,
                camTransform,
                config,
                inputService,
                groundSensor,
                animator,
                this,
                _cameraController,
                _aimTarget,
                _weaponRig,
                _weaponController,
                _weaponInventory
            );

            _stateMachine = new PlayerStateMachine();
            
            _stateMachine.AddState(new PlayerIdleState(_context, _stateMachine));
            _stateMachine.AddState(new PlayerMoveState(_context, _stateMachine));
            _stateMachine.AddState(new PlayerAimState(_context, _stateMachine));
            _stateMachine.AddState(new PlayerAirborneState(_context, _stateMachine));
            _stateMachine.AddState(new PlayerCrouchState(_context, _stateMachine));
            _stateMachine.AddState(new PlayerRollState(_context, _stateMachine));
            
            _stateMachine.SwitchState<PlayerIdleState>();
            
            // Тестовый спавн полностью удален! Теперь оружие берем только с пола.
        }

        private void OnEnable()
        {
            if (_inputService != null)
            {
                _inputService.OnJump += OnJump;
                _inputService.OnReload += OnReload;
            }
        }

        private void OnDisable()
        {
            if (_inputService != null)
            {
                _inputService.OnJump -= OnJump;
                _inputService.OnReload -= OnReload;
            }
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            
            _context.GroundSensor.Tick(); 
            
            // Вызываем проверку физики подбора каждый кадр
            _interactionSensor?.Tick();

            // ИСПРАВЛЕНИЕ 2: Вызываем Tick у контроллера оружия, чтобы работала IK левой руки
            _weaponController?.Tick(deltaTime);
            
            _cameraController?.Tick(deltaTime);
            _stateMachine?.Tick(deltaTime);
        }

        private void OnJump() => _stateMachine.HandleJump();
        
        private void OnReload()
        {
            if (_weaponInventory != null && _weaponInventory.CurrentWeapon != null)
            {
                _weaponInventory.CurrentWeapon.Reload();
            }
        }
        
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Vector3 spherePosition = transform.position + (Vector3.up * 0.1f);
            Gizmos.DrawWireSphere(spherePosition, 0.2f);
        }
        #endif
    }
}