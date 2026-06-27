using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VacuumSim.Robotics.Contracts;
using VacuumSim.Robotics.Configs;
using VacuumSim.Pathfinding;
using VacuumSim.Robotics.Signals;
using Zenject;

namespace VacuumSim.Robotics.Brain.States
{
    public class ManualTransitState : IVacuumState
    {
        private readonly IVacuumMotor _motor;
        private readonly VacuumConfig _config;
        private readonly Pathfinder _pathfinder;
        private readonly PathfindingGrid _grid;
        private readonly SignalBus _signalBus;

        public Vector3 TargetPoint { get; set; }

        public ManualTransitState(
            IVacuumMotor motor, VacuumConfig config, 
            Pathfinder pathfinder, PathfindingGrid grid, SignalBus signalBus)
        {
            _motor = motor;
            _config = config;
            _pathfinder = pathfinder;
            _grid = grid;
            _signalBus = signalBus;
        }

        public async UniTask ExecuteAsync(CancellationToken token)
        {
            Debug.Log("[State] Ручной приказ! Строю маршрут к точке игрока...");

            List<Node> path = _pathfinder.FindPath(_motor.Position, TargetPoint);

            if (path == null || path.Count == 0)
            {
                Debug.LogWarning("[State] Невозможно проехать в эту точку!");
                _signalBus.Fire<TransitCompletedSignal>();
                return;
            }

            _grid.CurrentPath = path;

            foreach (Node pathNode in path)
            {
                if (token.IsCancellationRequested) break;
                await MoveToNodeAsync(pathNode.WorldPosition, token);
            }

            _grid.CurrentPath = null;
            _motor.Stop();
            
            Debug.Log("[State] Прибыл в указанную точку. Передаю управление.");
            _signalBus.Fire<TransitCompletedSignal>();
        }

        private async UniTask MoveToNodeAsync(Vector3 targetPos, CancellationToken token)
        {
            Vector3 targetPosFlat = new Vector3(targetPos.x, 0, targetPos.z);
            while (!token.IsCancellationRequested)
            {
                Vector3 currentPosFlat = new Vector3(_motor.Position.x, 0, _motor.Position.z);
                if (Vector3.Distance(currentPosFlat, targetPosFlat) < 0.4f) return;

                Vector3 direction = targetPosFlat - currentPosFlat;
                if (direction.sqrMagnitude > 0.001f)
                {
                    float angle = Vector3.SignedAngle(_motor.Forward, direction.normalized, Vector3.up);
                    if (Mathf.Abs(angle) > 20f)
                    {
                        _motor.Stop();
                        try { await _motor.RotateAsync(angle, token).Timeout(System.TimeSpan.FromSeconds(1f)); } catch { }
                    }
                }
                _motor.MoveForward(_config.MoveSpeed);
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
            }
        }
    }
}