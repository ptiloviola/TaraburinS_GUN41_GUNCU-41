using System.Collections.Generic;
using MeatMushrooms.Wolf.Contracts;
using UnityEngine;
using Zenject;

namespace MeatMushrooms.Wolf.Components
{
    public class WolfBrain : IInitializable, ITickable
    {
        private readonly List<IWolfState> _availableStates;
        private readonly WolfStats _stats; // Добавили статы для отладки
        private IWolfState _currentState;
        
        private float _thinkTimer;
        private const float ThinkInterval = 0.5f; 

        // Обрати внимание: Zenject сам добавит сюда WolfStats, нам не нужно менять инсталлятор
        [Inject]
        public WolfBrain(List<IWolfState> availableStates, WolfStats stats)
        {
            _availableStates = availableStates;
            _stats = stats;
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

            // Строка телеметрии, которую мы будем собирать
            string telemetry = $"[Brain] Голод: {_stats.Hunger:F1} | ";

            foreach (var state in _availableStates)
            {
                float score = state.CalculateScore();
                telemetry += $"{state.GetType().Name}: {score:F1} | ";
                
                if (score > highestScore)
                {
                    highestScore = score;
                    bestState = state;
                }
            }

            // РАСКОММЕНТИРУЙ СТРОКУ НИЖЕ, чтобы видеть мысли волка в реальном времени!
            Debug.Log(telemetry);

            if (bestState != null && bestState != _currentState)
            {
                ChangeState(bestState);
            }
        }

        private void ChangeState(IWolfState newState)
        {
            _currentState?.Exit();
            _currentState = newState;
            
            // Если тут произойдет ошибка, мы это увидим, так как лог перехода стоит ПЕРЕД методом Enter
            Debug.Log($"[WolfBrain] ---> Переход в: {_currentState.GetType().Name}");
            
            _currentState?.Enter();
        }
    }
}