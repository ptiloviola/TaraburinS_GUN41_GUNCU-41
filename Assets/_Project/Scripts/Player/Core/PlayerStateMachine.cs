using System.Collections.Generic;
using System;

namespace TpsShooter.Player.Core
{
    public class PlayerStateMachine
    {
        private IPlayerState _currentState;
        private readonly Dictionary<Type, IPlayerState> _states = new();

        public void AddState(IPlayerState state)
        {
            _states.Add(state.GetType(), state);
        }

        public void SwitchState<T>() where T : IPlayerState
        {
            Type type = typeof(T);
            

            if (!_states.TryGetValue(type, out IPlayerState nextState))
            {
                DevLogger.LogError($"[StateMachine] Критическая ошибка! Стейт {type.Name} не зарегистрирован в машине состояний.");
                return;
            }

            if (_currentState != null)
                _currentState.Exit();

            DevLogger.Log($"[StateMachine] Transition: {_currentState?.GetType().Name} -> {type.Name}");

            _currentState = nextState;
            _currentState.Enter();
        }

        public void Tick(float deltaTime)
        {
            _currentState?.Tick(deltaTime);
        }

        public void HandleJump()
        {
            _currentState?.HandleJump();
        }
    }
}