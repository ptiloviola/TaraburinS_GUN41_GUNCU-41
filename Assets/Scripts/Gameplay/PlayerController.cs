using UnityEngine;
using Bowling.Ball;
using Zenject;

namespace Bowling.Gameplay
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private BaseThrowMechanic[] _inputMechanics;
        [SerializeField] private PhysicsConfig _physicsConfig;
        [SerializeField] private BallController _ballController;
        private BowlingInputActions _inputActions;
        private GameStateManager _gameStateManager;
        private BaseThrowMechanic _activeMechanic;
        private bool _canThrow = true;

        [Inject]
        public void Construct(BowlingInputActions inputActions, GameStateManager gameStateManager)
        {
            _inputActions = inputActions;
            _gameStateManager = gameStateManager;
        }

        private void Start()
        {
            foreach (var mechanic in _inputMechanics)
            {
                if (mechanic != null) mechanic.Initialize(_inputActions);
            }
            
            SetPhysicsStrategy(0);
            SetInputMechanic(0);  
        }

        public void SetPhysicsStrategy(int index)
        {
            IThrowStrategy newStrategy = ThrowStrategyFactory.CreateStrategy(index, _physicsConfig);
            
            if (newStrategy != null)
            {
                _ballController.SetStrategy(newStrategy);
            }
            
            _gameStateManager.ResetFullGame();
        }

        public void SetInputMechanic(int index)
        {
            foreach (var mechanic in _inputMechanics)
            {
                if (mechanic != null)
                {
                    mechanic.OnThrowExecuted -= HandleThrowExecuted;
                    mechanic.DisableMechanic();
                }
            }
            
            if (index >= 0 && index < _inputMechanics.Length)
            {
                _activeMechanic = _inputMechanics[index];
                _activeMechanic.EnableMechanic();
                _activeMechanic.OnThrowExecuted += HandleThrowExecuted;
                Debug.Log($"Механика ввода изменена на: {_activeMechanic.GetType().Name}");
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

            if (_ballController != null)
            {
                return _ballController.IsSettled();
            }
            return true;
        }

        private void OnDestroy()
        {
            if (_activeMechanic != null) _activeMechanic.OnThrowExecuted -= HandleThrowExecuted;
        }


    }
}


