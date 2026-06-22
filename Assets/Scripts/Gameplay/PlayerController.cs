using UnityEngine;
using Bowling.Ball;
using Zenject;
using System.Collections.Generic;

namespace Bowling.Gameplay
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Ссылки на сцену")]
        [SerializeField] private BallController _ballController;

        private List<BaseThrowMechanic> _mechanics;
        private DiContainer _container;
        private GameStateManager _gameStateManager;
        private BowlingInputActions _inputActions;

        private BaseThrowMechanic _activeMechanic;
        private bool _canThrow = true;

        [Inject]
        public void Construct(
            GameStateManager gameStateManager,
            List<BaseThrowMechanic> mechanics, 
            DiContainer container,
            BowlingInputActions inputActions)
        {
            _gameStateManager = gameStateManager;
            _mechanics = mechanics;
            _container = container;
            _inputActions = inputActions;

            foreach (var mechanic in _mechanics)
            {
                if (mechanic != null)
                {
                    mechanic.Initialize(_inputActions); 
                    mechanic.OnThrowExecuted += HandleThrowExecuted;
                }
            }
        }

        private void Start()
        {
            _inputActions.Enable();
            SetPhysicsStrategy(0);
            SetInputMechanic(0);  
        }

        public void SetPhysicsStrategy(int index)
        {
            IThrowStrategy newStrategy = index switch
            {
                0 => _container.Instantiate<AddForceStrategy>(),
                1 => _container.Instantiate<LinearVelocityStrategy>(),
                2 => _container.Instantiate<MovePositionStrategy>(),
                _ => null
            };
            
            if (newStrategy != null) _ballController.SetStrategy(newStrategy);
            _gameStateManager.ResetFullGame(); 
        }

        public void SetInputMechanic(int index)
        {
            foreach (var mechanic in _mechanics)
            {
                if (mechanic != null) mechanic.DisableMechanic();
            }
            
            if (index >= 0 && index < _mechanics.Count)
            {
                _activeMechanic = _mechanics[index];
                _activeMechanic.EnableMechanic();
                Debug.Log($"[PlayerController] Включена механика: {_activeMechanic.GetType().Name}");
            }
        }

        private void HandleThrowExecuted(Vector3 dir, float force)
        {
            if (!_canThrow) return;
            
            _ballController.ThrowBall(dir, force);
            LockInput();
            _gameStateManager.StartScoringRoutine();
        }

        public void LockInput() => _canThrow = false;
        public void UnlockInput() => _canThrow = true;
        
        public void ResetPlayerState()
        {
            if (_ballController != null) _ballController.ResetBall();
            if (_activeMechanic != null) _activeMechanic.ResetMechanic();
            UnlockInput();
        }

        public bool IsBallSettled()
        {
            if (_ballController != null) return _ballController.IsSettled();
            return true;
        }

        private void OnDestroy()
        {
            if (_mechanics != null)
            {
                foreach (var mechanic in _mechanics)
                {
                    if (mechanic != null) mechanic.OnThrowExecuted -= HandleThrowExecuted;
                }
            }
        }
    }
}