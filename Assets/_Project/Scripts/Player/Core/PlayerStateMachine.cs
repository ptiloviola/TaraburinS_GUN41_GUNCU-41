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
            if (_currentState != null)
                _currentState.Exit();

        #if UNITY_EDITOR
            // Выводим в консоль, откуда и куда мы переходим
            UnityEngine.Debug.Log($"[StateMachine] Transition: {_currentState?.GetType().Name} -> {typeof(T).Name}");
        #endif

            _currentState = _states[typeof(T)];
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