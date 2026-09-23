using System;
using System.Collections.Generic;

namespace Gameplay.Enemies.FSM
{
    public class EnemyStateMachine
    {
        private Dictionary<EnemyStateType, IEnemyState> _states = new Dictionary<EnemyStateType, IEnemyState>();
        private IEnemyState _currentState;

        public EnemyStateType CurrentStateType => _currentState?.StateType ?? EnemyStateType.Spawn;

        public event Action<EnemyStateType> OnStateChanged;

        public void AddState(IEnemyState state)
        {
            _states[state.StateType] = state;
        }

        public void ChangeState(EnemyStateType stateType)
        {
            if (_currentState != null)
            {
                if (_currentState.StateType == stateType) return;
                _currentState.Exit();
            }

            if (_states.TryGetValue(stateType, out IEnemyState nextState))
            {
                _currentState = nextState;
                _currentState.Enter();
                OnStateChanged?.Invoke(stateType);
            }
        }

        public void Tick(float deltaTime)
        {
            _currentState?.Tick(deltaTime);
        }

        public void Cleanup()
        {
            _currentState?.Exit();
            _currentState = null;
        }
    }
}