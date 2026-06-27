using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VacuumSim.Robotics.Contracts;
using VacuumSim.Robotics.Configs;
using VacuumSim.Pathfinding;

namespace VacuumSim.Robotics.Brain.Strategies
{
    public class ZigZagStrategy : BaseGridStrategy
    {
        private int _currentX;
        private int _currentY;
        private int _directionX = 1;

        public ZigZagStrategy(
            IVacuumMotor motor, VacuumConfig config, 
            PathfindingGrid grid, Pathfinder pathfinder) 
            : base(motor, config, grid, pathfinder)
        {
            
        }

        public override async UniTask ExecuteAsync(CancellationToken token)
        {
            Debug.Log("<color=cyan>[ZigZag] --- СТАРТ СТРАТЕГИИ ---</color>");

            Node startNode = _grid.NodeFromWorldPoint(_motor.Position);
            _currentX = startNode.GridX;
            _currentY = startNode.GridY;

            while (!token.IsCancellationRequested)
            {
                Node currentNode = _grid.GetNodeFromIndices(_currentX, _currentY);
                if (!currentNode.IsCleaned) currentNode.IsCleaned = true;

                int nextX = _currentX + _directionX;
                int nextY = _currentY;
                bool moveSuccessful = false;

                if (IsValidWalkableAndDirty(nextX, nextY))
                {
                    _currentX = nextX;
                    moveSuccessful = true;
                }
                else
                {
                    nextX = _currentX;
                    nextY = _currentY + 1;

                    if (IsValidWalkableAndDirty(nextX, nextY))
                    {
                        _currentY = nextY;
                        _directionX = -_directionX;
                        moveSuccessful = true;
                    }
                    else
                    {
                        nextY = _currentY - 1;
                        if (IsValidWalkableAndDirty(nextX, nextY))
                        {
                            _currentY = nextY;
                            _directionX = -_directionX;
                            moveSuccessful = true;
                        }
                    }
                }

                if (!moveSuccessful)
                {
                    Node nextDirtyNode = FindNearestDirtyNode();

                    if (nextDirtyNode == null)
                    {
                        _motor.Stop();
                        await UniTask.WaitUntil(() => FindNearestDirtyNode() != null, PlayerLoopTiming.Update, token);
                        continue; 
                    }

                    List<Node> path = _pathfinder.FindPath(_motor.Position, nextDirtyNode.WorldPosition);

                    if (path != null && path.Count > 0)
                    {
                        _grid.CurrentPath = path; 
                        foreach (Node pathNode in path)
                        {
                            if (token.IsCancellationRequested) break;
                            await MoveToNodeAsync(pathNode.WorldPosition, token, isTransit: true);
                        }
                        _grid.CurrentPath = null;
                        
                        _currentX = nextDirtyNode.GridX;
                        _currentY = nextDirtyNode.GridY;
                        continue;
                    }
                    else
                    {
                        nextDirtyNode.IsCleaned = true;
                        continue;
                    }
                }

                Vector3 targetPos = _grid.GetNodeFromIndices(_currentX, _currentY).WorldPosition;
                await MoveToNodeAsync(targetPos, token, isTransit: false);
            }
        }
    }
}