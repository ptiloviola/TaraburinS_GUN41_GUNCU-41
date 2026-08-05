using UnityEngine;
using Zenject;
using TpsShooter.Services.Input;
using TpsShooter.Player.States;
using TpsShooter.Player.Core;
using TpsShooter.Player.Configs;
using TpsShooter.Player.Camera;
using TpsShooter.Player.Weapons;
using TpsShooter.Weapons;


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
        
        // Добавляем наши новые сокеты для оружия
        [SerializeField] private Transform _weaponHandSocket;
        [SerializeField] private Transform _weaponBackSocket1;
        [SerializeField] private Transform _weaponBackSocket2;
        [SerializeField] private Transform _leftHandIkTarget;

        [Header("Test Spawning")]
        [SerializeField] private WeaponBase _testPistolPrefab;

        private PlayerCameraController _cameraController;
        private PlayerWeaponController _weaponController;

        [Inject]
        public void Construct(IInputService inputService, PlayerConfig config)
        {
            _inputService = inputService;
            Transform camTransform = UnityEngine.Camera.main != null ? UnityEngine.Camera.main.transform : null;
            var groundSensor = new GroundSensor(transform, config);
            Animator animator = GetComponentInChildren<Animator>();
            
            // 1. Создаем наши контроллеры (без MonoBehaviour!)
            _cameraController = new PlayerCameraController(config, _cameraTarget, _normalCamera);
            
            // Создаем контроллер оружия, передавая this как MonoBehaviour для запуска корутин
            _weaponController = new PlayerWeaponController(
                _weaponHandSocket, 
                _weaponBackSocket1, 
                _weaponBackSocket2, 
                animator, 
                _leftHandIkTarget
            );

            // 2. Упаковываем всё в Контекст
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
                _weaponController // <-- Добавили контроллер оружия в конец
            );

            _stateMachine = new PlayerStateMachine();
            
            // Регистрируем все возможные состояния
            _stateMachine.AddState(new PlayerIdleState(_context, _stateMachine));
            _stateMachine.AddState(new PlayerMoveState(_context, _stateMachine));
            _stateMachine.AddState(new PlayerAimState(_context, _stateMachine));
            _stateMachine.AddState(new PlayerAirborneState(_context, _stateMachine));
            _stateMachine.AddState(new PlayerCrouchState(_context, _stateMachine));
            _stateMachine.AddState(new PlayerRollState(_context, _stateMachine));
            
            _stateMachine.SwitchState<PlayerIdleState>();
            // НОВЫЙ ВЫЗОВ: Спавним оружие при старте игры!
            if (_testPistolPrefab != null)
            {
                _weaponController.TestEquipWeapon(_testPistolPrefab);
            }
            else
            {
                Debug.LogWarning("Test Pistol Prefab is not assigned in PlayerFacade!");
            }
        }

        private void OnEnable()
        {
            if (_inputService != null)
                _inputService.OnJump += OnJump;
        }

        private void OnDisable()
        {
            if (_inputService != null)
                _inputService.OnJump -= OnJump;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            _context.GroundSensor.Tick(); 
            
            _cameraController?.Tick(deltaTime);

            _stateMachine?.Tick(deltaTime);
        }

        private void OnJump()
        {
            _stateMachine.HandleJump();
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