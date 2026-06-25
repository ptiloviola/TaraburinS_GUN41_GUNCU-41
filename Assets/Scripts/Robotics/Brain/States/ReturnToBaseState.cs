using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VacuumSim.Robotics.Contracts;
using VacuumSim.Robotics.Configs;
using VacuumSim.Pathfinding;
using VacuumSim.Robotics.Components;
using Zenject;
using VacuumSim.Robotics.Signals;

namespace VacuumSim.Robotics.Brain.States
{
    public class ReturnToBaseState : IVacuumState
    {
        private readonly IVacuumMotor _motor;
        private readonly VacuumConfig _config;
        private readonly Pathfinder _pathfinder;
        private readonly BaseStation _baseStation;
        private readonly PathfindingGrid _grid;

        private SignalBus _signalBus;



        public ReturnToBaseState(
            IVacuumMotor motor, 
            VacuumConfig config, 
            Pathfinder pathfinder, 
            BaseStation baseStation,
            PathfindingGrid grid,
            SignalBus signalBus)
        {
            _motor = motor;
            _config = config;
            _pathfinder = pathfinder;
            _baseStation = baseStation;
            _grid = grid;
            _signalBus = signalBus;
        }

        public async UniTask ExecuteAsync(CancellationToken token)
        {
            Debug.Log("[State] Активирован умный возврат на базу!");

            while (!token.IsCancellationRequested)
            {
                // 1. Сначала просим навигатор построить путь
                List<Node> path = _pathfinder.FindPath(_motor.Position, _baseStation.Position);

                // 2. ДВОЙНАЯ ПРОВЕРКА ПРИБЫТИЯ
                Vector3 currentPosFlat = new Vector3(_motor.Position.x, 0, _motor.Position.z);
                Vector3 basePosFlat = new Vector3(_baseStation.Position.x, 0, _baseStation.Position.z);
                
                // УСЛОВИЕ: Если физически близко (< 1.5f) 
                // ИЛИ если навигатор А* вернул путь из 1 точки (значит мы УЖЕ в ячейке базы!)
                if (Vector3.Distance(currentPosFlat, basePosFlat) < 1.5f || (path != null && path.Count == 1))
                {
                    _motor.Stop();
                    _grid.CurrentPath = null;
                    Debug.Log("[State] Робот успешно прибыл на базу!");
                    _signalBus.Fire<ArrivedAtBaseSignal>(); // Даем команду на зарядку!
                    return; // Выходим из цикла раз и навсегда
                }

                // 3. ЗАЩИТА ОТ ТУПИКОВ (Wiggle)
                if (path == null || path.Count == 0)
                {
                    Debug.LogWarning("[State] Путь заблокирован! Пытаюсь вырваться (Wiggle)...");
                    _motor.MoveForward(-_config.MoveSpeed * 0.5f);
                    await UniTask.Delay(600, cancellationToken: token);
                    await _motor.RotateAsync(45f, token);
                    continue;
                }

                _grid.CurrentPath = path;

                // 4. ЕДЕМ! 
                // Так как выше мы отсекли path.Count == 1, мы уверенно берем path[1] (следующую ячейку)
                Node targetNode = path[1]; 
                Vector3 targetPosFlat = new Vector3(targetNode.WorldPosition.x, 0, targetNode.WorldPosition.z);

                Vector3 directionToNode = targetNode.WorldPosition - _motor.Position;
                directionToNode.y = 0;

                if (directionToNode.sqrMagnitude > 0.001f)
                {
                    float angleToTarget = Vector3.SignedAngle(_motor.Forward, directionToNode.normalized, Vector3.up);
                    if (Mathf.Abs(angleToTarget) > 5f) 
                    {
                        _motor.Stop();
                        try { await _motor.RotateAsync(angleToTarget, token).Timeout(System.TimeSpan.FromSeconds(1f)); }
                        catch (System.TimeoutException) { /* Игнорируем заминки поворота */ }
                    }
                }

                _motor.MoveForward(_config.MoveSpeed);

                try
                {
                    await UniTask.WaitUntil(() => 
                    {
                        Vector3 pos = new Vector3(_motor.Position.x, 0, _motor.Position.z);
                        // Оставляем радиус 0.5f для уверенного "проглатывания" точек маршрута
                        return Vector3.Distance(pos, targetPosFlat) < 0.5f; 
                    }, PlayerLoopTiming.FixedUpdate, token).Timeout(System.TimeSpan.FromSeconds(1.5f));
                }
                catch (System.TimeoutException)
                {
                    Debug.LogWarning("[State] Застрял на перегоне! Пересчитываю маршрут.");
                    _motor.Stop();
                }
            }
        }
    }
}