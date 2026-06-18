using Bowling.Ball;
using UnityEngine;
using Bowling.BowlingPins;
using System.Collections;

namespace Bowling.UI
{
    public class GameStateManager : MonoBehaviour
    {
        [SerializeField] private BallController _ballController;
        [SerializeField] private BaseThrowMechanic[] _inputMechanics;
        [SerializeField] private PhysicsConfig _physicsConfig;

        private BaseThrowMechanic _activeMechanic;

        [SerializeField] private GameObject _pinDeck;
        private BowlingPin[] _bowlingPins;

        public event System.Action<int> OnPinsKnockedDown;

        private void Start()
        {
            SetPhysicsStrategy(0);
            SetInputMechanic(0);  
            if (_pinDeck != null)
            {
                _bowlingPins = _pinDeck.GetComponentsInChildren<BowlingPin>();
            }
        }

        public void SetPhysicsStrategy(int index)
        {
            IThrowStrategy newStrategy = ThrowStrategyFactory.CreateStrategy(index, _physicsConfig);
            
            if (newStrategy != null)
            {
                _ballController.SetStrategy(newStrategy);
            }
            
            ResetRound();
        }

        public void SetInputMechanic(int index)
        {
            foreach (var mechanic in _inputMechanics)
            {
                if (mechanic != null)
                {
                    mechanic.OnThrowExecuted -= HandleThrow;
                    mechanic.enabled = false;
                }
            }
            
            if (index >= 0 && index < _inputMechanics.Length)
            {
                _activeMechanic = _inputMechanics[index];
                _activeMechanic.enabled = true;
                _activeMechanic.OnThrowExecuted += HandleThrow;
                Debug.Log($"Механика ввода изменена на: {_activeMechanic.GetType().Name}");
            }
            ResetRound();
        }

        private void HandleThrow(Vector3 dir, float force)
        {
            _ballController.ThrowBall(dir, force);
            StartScoringRoutine();
        }

        private void OnDestroy()
        {
            if (_activeMechanic != null)
            {
                _activeMechanic.OnThrowExecuted -= HandleThrow;
            }
        }

        public void ResetRound()
        {
            _ballController.ResetBall();
            
            if (_activeMechanic != null)
            {
                _activeMechanic.ResetMechanic();
            }

            if (_bowlingPins != null)
            {
                foreach(var pin in _bowlingPins)
                {
                    pin.ResetPin();
                }
            }

          
            Debug.Log("Раунд сброшен. Можно бросать снова!");
        }

        public void StartScoringRoutine() 
        {
            StartCoroutine(CalculateScoreAfterDelay());
        }

        private IEnumerator CalculateScoreAfterDelay()
        {
            yield return new WaitForSeconds(5f);
            int fallenCount = 0;
            foreach(var pin in _bowlingPins)
            {
                if(pin.IsFallen)
                {
                    fallenCount += 1;
                }
            }
            Debug.Log($"Бросок завершен! Упало кеглей: {fallenCount}");
            OnPinsKnockedDown?.Invoke(fallenCount);
        }
    }
}


