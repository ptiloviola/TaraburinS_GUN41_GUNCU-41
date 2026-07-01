using System.Collections.Generic;
using MeatMushrooms.Wolf.Contracts;
using UnityEngine;
using Zenject;

namespace MeatMushrooms.Wolf.Components
{
    public class WolfBrain : IInitializable, ITickable
    {
        private List<IWolfState> _availableStates;
        private IWolfState _currentState;
        
        // Таймер, чтобы не пересчитывать желания каждый кадр (оптимизация)
        private float _thinkTimer;
        private const float ThinkInterval = 0.5f; // Волк "думает" 2 раза в секунду

        [Inject]
        public WolfBrain()
        {
            // Пока создаем пустой список. 
            // Позже Zenject сам передаст сюда все готовые состояния (Idle, Wander, Eat).
            _availableStates = new List<IWolfState>();
        }

        public void Initialize()
        {
            // В будущем здесь мы зададим дефолтное состояние при спавне
            Debug.Log("[WolfBrain] Мозг волка запущен.");
        }

        // Этот метод Zenject вызывает каждый кадр (замена Update)
        public void Tick()
        {
            _thinkTimer += Time.deltaTime;
            
            if (_thinkTimer >= ThinkInterval)
            {
                _thinkTimer = 0f;
                EvaluateStates(); // Пришло время подумать!
            }

            // Если состояние выбрано, выполняем его логику
            _currentState?.Tick();
        }

        private void EvaluateStates()
        {
            if (_availableStates.Count == 0) return;

            IWolfState bestState = null;
            float highestScore = -1f;

            // Опрашиваем все доступные состояния: кто наберет больше баллов?
            foreach (var state in _availableStates)
            {
                float score = state.CalculateScore();
                if (score > highestScore)
                {
                    highestScore = score;
                    bestState = state;
                }
            }

            // Если победившее состояние отличается от текущего - переключаемся
            if (bestState != null && bestState != _currentState)
            {
                ChangeState(bestState);
            }
        }

        private void ChangeState(IWolfState newState)
        {
            _currentState?.Exit();
            
            _currentState = newState;
            
            _currentState?.Enter();
            
            // Временно выводим в консоль, чтобы видеть, как меняются мысли волка
            Debug.Log($"[WolfBrain] Переход в новое состояние: {_currentState.GetType().Name}");
        }
    }
}

