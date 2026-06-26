using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VacuumSim.Robotics.Contracts;
using VacuumSim.Robotics.Configs;
using VacuumSim.Pathfinding;

namespace VacuumSim.Robotics.Brain.Strategies
{
    // Наследуемся от базовой стратегии
    public class SpiralStrategy : BaseGridStrategy
    {
        private int _currentX;
        private int _currentY;
        
        // Векторы направлений: Вверх, Вправо, Вниз, Влево (по часовой стрелке)
        private readonly Vector2Int[] _directions = {
            new Vector2Int(0, 1),
            new Vector2Int(1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0)
        };
        private int _currentDirIndex = 0; // Начинаем движение "Вверх"

        // Конструктор передает зависимости в базовый класс
        public SpiralStrategy(
            IVacuumMotor motor, VacuumConfig config, 
            PathfindingGrid grid, Pathfinder pathfinder) 
            : base(motor, config, grid, pathfinder)
        {
        }

        // Реализуем только логику раскручивания спирали!
        public override async UniTask ExecuteAsync(CancellationToken token)
        {
            Debug.Log("<color=magenta>[Spiral] --- СТАРТ СПИРАЛЬНОЙ СТРАТЕГИИ ---</color>");

            Node startNode = _grid.NodeFromWorldPoint(_motor.Position);
            _currentX = startNode.GridX;
            _currentY = startNode.GridY;

            while (!token.IsCancellationRequested)
            {
                Node currentNode = _grid.GetNodeFromIndices(_currentX, _currentY);
                if (!currentNode.IsCleaned)
                {
                    currentNode.IsCleaned = true;
                }

                bool moveSuccessful = false;
                int turnsAttempts = 0;

                // Пытаемся сделать шаг. Если стена - поворачиваем на 90 градусов.
                while (turnsAttempts < 4)
                {
                    int nextX = _currentX + _directions[_currentDirIndex].x;
                    int nextY = _currentY + _directions[_currentDirIndex].y;

                    // Вызов метода из базового класса!
                    if (IsValidWalkableAndDirty(nextX, nextY)) 
                    {
                        _currentX = nextX;
                        _currentY = nextY;
                        moveSuccessful = true;
                        break; 
                    }
                    else
                    {
                        _currentDirIndex = (_currentDirIndex + 1) % 4;
                        turnsAttempts++;
                    }
                }

                // Тупик (все 4 стороны заблокированы)
                if (!moveSuccessful)
                {
                    Debug.LogWarning($"<color=orange>[Spiral] Локальный тупик. Ищу новую зону через A*...</color>");
                    
                    // Вызов метода из базового класса!
                    Node nextDirtyNode = FindNearestDirtyNode(); 

                    if (nextDirtyNode == null)
                    {
                        Debug.Log("<color=green>[Spiral] КОМНАТА ПОЛНОСТЬЮ УБРАНА! Жду новый мусор...</color>");
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
                            
                            // Вызов метода из базового класса!
                            await MoveToNodeAsync(pathNode.WorldPosition, token, isTransit: true); 
                        }

                        _grid.CurrentPath = null;
                        
                        _currentX = nextDirtyNode.GridX;
                        _currentY = nextDirtyNode.GridY;
                        _currentDirIndex = 0; // Сбрасываем направление для новой спирали
                        continue;
                    }
                    else
                    {
                        nextDirtyNode.IsCleaned = true;
                        continue;
                    }
                }

                // Едем в выбранную ячейку
                Vector3 targetPos = _grid.GetNodeFromIndices(_currentX, _currentY).WorldPosition;
                
                // Вызов метода из базового класса!
                await MoveToNodeAsync(targetPos, token, isTransit: false); 
            }
        }
    }
}