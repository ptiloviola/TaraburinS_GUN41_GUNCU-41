using Bowling.Ball;
using UnityEngine;

namespace Bowling.UI
{
    public class GameStateManager : MonoBehaviour
    {
        [SerializeField] private BallController _ballController;
        [SerializeField] private BaseThrowMechanic[] _inputMechanics;

        private BaseThrowMechanic _activeMechanic;

        private void Start()
        {
            SetPhysicsStrategy(0);
            SetInputMechanic(0);  
        }

        public void SetPhysicsStrategy(int index)
        {
            IThrowStrategy newStrategy = ThrowStrategyFactory.CreateStrategy(index);
            _ballController.SetStrategy(newStrategy);
        }

        public void SetInputMechanic(int index)
        {
            if (_activeMechanic != null)
            {
                _activeMechanic.OnThrowExecuted -= HandleThrow;
                _activeMechanic.enabled = false;
            }
            if (index >= 0 && index < _inputMechanics.Length)
            {
                _activeMechanic = _inputMechanics[index];
                _activeMechanic.enabled = true;
                _activeMechanic.OnThrowExecuted += HandleThrow;
                Debug.Log($"Механика ввода изменена на: {_activeMechanic.GetType().Name}");
            }
        }

        private void HandleThrow(Vector3 dir, float force)
        {
            _ballController.ThrowBall(dir, force);
        }

        private void OnDestroy()
        {
            if (_activeMechanic != null)
            {
                _activeMechanic.OnThrowExecuted -= HandleThrow;
            }
        }
    }
}


