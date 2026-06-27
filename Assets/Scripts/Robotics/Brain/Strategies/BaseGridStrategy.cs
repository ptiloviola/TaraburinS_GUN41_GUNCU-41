using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VacuumSim.Robotics.Contracts;
using VacuumSim.Robotics.Configs;
using VacuumSim.Pathfinding;

namespace VacuumSim.Robotics.Brain.Strategies
{
    public abstract class BaseGridStrategy : ICleaningStrategy
    {
        protected readonly IVacuumMotor _motor;
        protected readonly VacuumConfig _config;
        protected readonly PathfindingGrid _grid;
        protected readonly Pathfinder _pathfinder;

        protected BaseGridStrategy(
            IVacuumMotor motor, 
            VacuumConfig config, 
            PathfindingGrid grid, 
            Pathfinder pathfinder)
        {
            _motor = motor;
            _config = config;
            _grid = grid;
            _pathfinder = pathfinder;
        }

        public abstract UniTask ExecuteAsync(CancellationToken token);


        protected bool IsValidWalkableAndDirty(int x, int y)
        {
            if (x < 0 || x >= _grid.GridSizeX || y < 0 || y >= _grid.GridSizeY) return false;
            Node node = _grid.GetNodeFromIndices(x, y);
            return node.IsWalkable && !node.IsCleaned;
        }

        protected Node FindNearestDirtyNode()
        {
            Node bestNode = null;
            float minDistance = float.MaxValue;
            Vector3 currentPosFlat = new Vector3(_motor.Position.x, 0, _motor.Position.z);

            for (int y = 0; y < _grid.GridSizeY; y++)
            {
                for (int x = 0; x < _grid.GridSizeX; x++)
                {
                    Node node = _grid.GetNodeFromIndices(x, y);
                    if (node.IsWalkable && !node.IsCleaned)
                    {
                        Vector3 nodePosFlat = new Vector3(node.WorldPosition.x, 0, node.WorldPosition.z);
                        float dist = Vector3.Distance(currentPosFlat, nodePosFlat);
                        
                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            bestNode = node;
                        }
                    }
                }
            }
            return bestNode;
        }

        protected async UniTask MoveToNodeAsync(Vector3 targetPos, CancellationToken token, bool isTransit)
        {
            Vector3 targetPosFlat = new Vector3(targetPos.x, 0, targetPos.z);
            Vector3 currentPosFlat = new Vector3(_motor.Position.x, 0, _motor.Position.z);
            Vector3 direction = targetPosFlat - currentPosFlat;
            
            if (direction.sqrMagnitude > 0.001f)
            {
                float initialAngle = Vector3.SignedAngle(_motor.Forward, direction.normalized, Vector3.up);
                if (Mathf.Abs(initialAngle) > 2f) 
                {
                    _motor.Stop();
                    try { await _motor.RotateAsync(initialAngle, token).Timeout(System.TimeSpan.FromSeconds(1f)); }
                    catch (System.TimeoutException) { Debug.LogWarning("[Motor] Таймаут начального поворота!"); }
                }
            }

            while (!token.IsCancellationRequested)
            {
                currentPosFlat = new Vector3(_motor.Position.x, 0, _motor.Position.z);
                float distance = Vector3.Distance(currentPosFlat, targetPosFlat);
                
                if (distance < 0.4f) return;

                Vector3 currentDirection = targetPosFlat - currentPosFlat;
                if (currentDirection.sqrMagnitude > 0.001f)
                {
                    float currentAngle = Vector3.SignedAngle(_motor.Forward, currentDirection.normalized, Vector3.up);
                    if (Mathf.Abs(currentAngle) > 45f)
                    {
                        if (!isTransit) Debug.Log($"[Motor] Аварийное отклонение {currentAngle:F1}. Корректирую курс...");
                        _motor.Stop();
                        try { await _motor.RotateAsync(currentAngle, token).Timeout(System.TimeSpan.FromSeconds(1f)); }
                        catch (System.TimeoutException) { Debug.LogWarning("[Motor] Таймаут аварийного поворота!"); }
                    }
                }

                _motor.MoveForward(_config.MoveSpeed);
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
            }
        }
    }
}