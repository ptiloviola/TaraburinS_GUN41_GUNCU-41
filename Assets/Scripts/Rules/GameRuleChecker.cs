using UnityEngine;
using Zenject;
using VacuumSim.Pathfinding;
using VacuumSim.GameConfigs;

namespace VacuumSim.Rules
{
    public struct GameOverSignal { }

    public class GameRuleChecker : ITickable
    {
        private readonly PathfindingGrid _grid;
        private readonly SignalBus _signalBus;

        private readonly GameConfig _gameConfig;
        
        private float _checkTimer = 0f;
        private const float CHECK_INTERVAL = 1f; 
        private bool _isGameOver = false;

        public GameRuleChecker(PathfindingGrid grid, SignalBus signalBus, GameConfig gameConfig)
        {
            _grid = grid;
            _signalBus = signalBus;
            _gameConfig = gameConfig;
        }

        public void Tick()
        {
            if (_isGameOver) return;

            _checkTimer += Time.deltaTime;
            if (_checkTimer >= CHECK_INTERVAL)
            {
                _checkTimer = 0f;
                float trashPercent = _grid.GetDirtyPercentage();

                // Debug.Log($"[GameRule] Пол завален мусором на: {trashPercent * 100:F1}%");

                if (trashPercent >= _gameConfig.GameOverTrashPercent)
                {
                    _isGameOver = true;
                    Debug.Log("<color=red>[GameRule] ПОРАЖЕНИЕ! Комната слишком грязная!</color>");
                    _signalBus.Fire<GameOverSignal>();
                }
            }
        }
    }
}