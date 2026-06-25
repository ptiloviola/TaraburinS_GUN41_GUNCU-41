using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using VacuumSim.Robotics.Brain.Strategies;
using UnityEngine;

namespace VacuumSim.Robotics.Brain.States
{
    public class CleaningState : IVacuumState
    {
        private readonly List<ICleaningStrategy> _strategies;
        private int _activeIndex = 0; // Индекс текущей стратегии

        // Zenject автоматически соберет все забинденные ICleaningStrategy в этот список!
        public CleaningState(List<ICleaningStrategy> strategies)
        {
            _strategies = strategies;
            if (_strategies.Count == 0)
            {
                Debug.LogError("[CleaningState] Нет доступных стратегий уборки!");
            }
        }

        public void SetStrategy(int index)
        {
            if (index >= 0 && index < _strategies.Count)
            {
                _activeIndex = index;
                Debug.Log($"[CleaningState] Выбрана стратегия: {_strategies[_activeIndex].GetType().Name}");
            }
        }

        public async UniTask ExecuteAsync(CancellationToken token)
        {
            if (_strategies.Count == 0) return;
            
            // Запускаем выбранную стратегию
            await _strategies[_activeIndex].ExecuteAsync(token);
        }
    }
}