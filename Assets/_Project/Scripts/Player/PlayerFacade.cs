using UnityEngine;
using Zenject;
using TpsShooter.Services.Input;
using TpsShooter.Player.States;
using TpsShooter.Player.Core;
using TpsShooter.Player.Configs;
using TpsShooter.Player.Camera;

namespace TpsShooter.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerFacade : MonoBehaviour
    {
        private PlayerStateMachine _stateMachine;
        private PlayerContext _context;
        private IInputService _inputService;
        [SerializeField] private Cinemachine.CinemachineFreeLook _normalCamera;
        [SerializeField] private Transform _cameraTarget;
        [SerializeField] private Transform _aimTarget;
        [SerializeField] private UnityEngine.Animations.Rigging.Rig _weaponRig;

        private PlayerCameraController _cameraController;

        [Inject]
        public void Construct(IInputService inputService, PlayerConfig config)
        {
            _inputService = inputService;
            Transform camTransform = UnityEngine.Camera.main != null ? UnityEngine.Camera.main.transform : null;
            var groundSensor = new GroundSensor(transform, config);
            Animator animator = GetComponentInChildren<Animator>();
            // 1. Создаем наш контроллер камеры (без MonoBehaviour!)
            _cameraController = new PlayerCameraController(config, _cameraTarget, _normalCamera);

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
                _weaponRig

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
            
            // 3. Обновляем контроллер камеры каждый кадр
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
            // Рисуем красную сферу там же, где ее создает наш CheckSphere
            Gizmos.color = Color.red;
            Vector3 spherePosition = transform.position + (Vector3.up * 0.1f);
            Gizmos.DrawWireSphere(spherePosition, 0.2f);
        }
        #endif
    }
}