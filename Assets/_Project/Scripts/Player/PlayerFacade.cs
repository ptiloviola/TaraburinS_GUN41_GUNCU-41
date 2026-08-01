using UnityEngine;
using Zenject;
using TpsShooter.Services.Input;
using TpsShooter.Player.States;
using TpsShooter.Player.Core;
using TpsShooter.Player.Configs;

namespace TpsShooter.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerFacade : MonoBehaviour
    {
        private PlayerStateMachine _stateMachine;
        private PlayerContext _context;
        private IInputService _inputService;

        [Inject]
        public void Construct(IInputService inputService, PlayerConfig config)
        {
            _inputService = inputService;
            Transform camTransform = Camera.main != null ? Camera.main.transform : null;
            var groundSensor = new GroundSensor(transform, config);

            _context = new PlayerContext(
                GetComponent<CharacterController>(),
                transform,
                camTransform,
                config,
                inputService,
                groundSensor // Передаем сенсор в контекст
            );

            _stateMachine = new PlayerStateMachine();
            
            // Регистрируем все возможные состояния
            _stateMachine.AddState(new PlayerIdleState(_context, _stateMachine));
            _stateMachine.AddState(new PlayerMoveState(_context, _stateMachine));
            _stateMachine.AddState(new PlayerAimState(_context, _stateMachine));
            _stateMachine.AddState(new PlayerAirborneState(_context, _stateMachine));
            
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
            _context.GroundSensor.Tick(); // Сенсор обновил данные
            // Знак вопроса означает: "вызывай Tick только если _stateMachine не равен null"
            _stateMachine?.Tick(Time.deltaTime);
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