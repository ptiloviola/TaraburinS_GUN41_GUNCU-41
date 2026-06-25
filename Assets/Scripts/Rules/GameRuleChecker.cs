using UnityEngine;
using Zenject;
using VacuumSim.Pathfinding;
using VacuumSim.Rules;

namespace VacuumSim.Rules
{
    public struct GameOverSignal { }

    public class GameRuleChecker : ITickable
    {
        private readonly PathfindingGrid _grid;
        private readonly SignalBus _signalBus;
        
        private float _checkTimer = 0f;
        private const float CHECK_INTERVAL = 1f; 
        private bool _isGameOver = false;

        public GameRuleChecker(PathfindingGrid grid, SignalBus signalBus)
        {
            _grid = grid;
            _signalBus = signalBus;
        }

        public void Tick()
        {
            if (_isGameOver) return;

            _checkTimer += Time.deltaTime;
            if (_checkTimer >= CHECK_INTERVAL)
            {
                _checkTimer = 0f;
                float trashPercent = _grid.GetDirtyPercentage();

                // Лог теперь будет показывать правду (на старте будет 0%)
                Debug.Log($"[GameRule] Пол завален мусором на: {trashPercent * 100:F1}%");

                if (trashPercent >= 0.2f)
                {
                    _isGameOver = true;
                    Debug.Log("<color=red>[GameRule] ПОРАЖЕНИЕ! Комната слишком грязная!</color>");
                    _signalBus.Fire<GameOverSignal>();
                }
            }
        }
    }
}