using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Gameplay.Combat.States
{
    public class LevelStateMachine : IInitializable, IDisposable
    {
        private readonly Dictionary<Type, ILevelState> _states = new Dictionary<Type, ILevelState>();
        private ILevelState _currentState;
        private CancellationTokenSource _cts;

        public LevelStateMachine(List<ILevelState> availableStates)
        {
            foreach (ILevelState state in availableStates)
            {
                _states[state.GetType()] = state;
            }
        }

        public void Initialize()
        {
            _cts = new CancellationTokenSource();
            ChangeStateAsync<LevelInitState>().Forget();
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        public async UniTaskVoid ChangeStateAsync<TState>() where TState : class, ILevelState
        {
            Type stateType = typeof(TState);
            
            if (!_states.TryGetValue(stateType, out ILevelState nextState))
            {
                Debug.LogError($"[LevelStateMachine] Состояние {stateType.Name} не найдено в словаре!");
                return;
            }

            if (_currentState != null)
            {
                await _currentState.ExitAsync(_cts.Token);
            }

            _currentState = nextState;
            
            Debug.Log($"<color=yellow>[LevelStateMachine] Переход в состояние: {stateType.Name}</color>");
            
            await _currentState.EnterAsync(this, _cts.Token);
        }
    }
}