using System.Collections.Generic;
using MeatMushrooms.Wolf.Contracts;
using UnityEngine;
using Zenject;

namespace MeatMushrooms.Wolf.Components
{
    public class WolfBrain : IInitializable, ITickable
    {
        private readonly List<IWolfState> _availableStates;
        private IWolfState _currentState;
        
        private float _thinkTimer;
        private const float ThinkInterval = 0.5f; 

        // ПУБЛИЧНЫЕ СВОЙСТВА ДЛЯ ДЕБАГГЕРА
        public string CurrentStateName => _currentState != null ? _currentState.GetType().Name : "Initializing...";
        public Dictionary<string, float> StateScores { get; private set; } = new Dictionary<string, float>();

        // Убрали WolfStats, так как мозгу они не нужны
        [Inject]
        public WolfBrain(List<IWolfState> availableStates)
        {
            _availableStates = availableStates;
        }

        public void Initialize()
        {
            Debug.Log("[WolfBrain] Мозг волка запущен.");
        }

        public void Tick()
        {
            _thinkTimer += Time.deltaTime;
            
            if (_thinkTimer >= ThinkInterval)
            {
                _thinkTimer = 0f;
                EvaluateStates();
            }

            _currentState?.Tick();
        }

        private void EvaluateStates()
        {
            if (_availableStates.Count == 0) return;

            IWolfState bestState = null;
            float highestScore = -1f;

            foreach (var state in _availableStates)
            {
                float score = state.CalculateScore();
                
                // Сохраняем очки в словарь (без создания мусорных строк)
                StateScores[state.GetType().Name] = score;
                
                if (score > highestScore)
                {
                    highestScore = score;
                    bestState = state;
                }
            }

            if (bestState != null && bestState != _currentState)
            {
                ChangeState(bestState);
            }
        }

        private void ChangeState(IWolfState newState)
        {
            _currentState?.Exit();
            _currentState = newState;
            
            // Оставляем только этот лог, чтобы видеть факт перехода в консоли
            Debug.Log($"<color=magenta>[WolfBrain]</color> Волк перешел в: {_currentState.GetType().Name}");
            
            _currentState?.Enter();
        }
    }
}