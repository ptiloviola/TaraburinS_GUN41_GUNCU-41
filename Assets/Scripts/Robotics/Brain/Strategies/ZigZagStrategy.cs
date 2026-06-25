using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VacuumSim.Robotics.Contracts;
using VacuumSim.Robotics.Configs;
using VacuumSim.Pathfinding;

namespace VacuumSim.Robotics.Brain.Strategies
{
    public class ZigZagStrategy : ICleaningStrategy
    {
        private readonly IVacuumMotor _motor;
        private readonly VacuumConfig _config;
        private readonly PathfindingGrid _grid;
        private readonly Pathfinder _pathfinder;

        private int _currentX;
        private int _currentY;
        private int _directionX = 1;

        public ZigZagStrategy(
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

        public async UniTask ExecuteAsync(CancellationToken token)
        {
            Debug.Log("<color=cyan>[ZigZag] --- СТАРТ СТРАТЕГИИ ---</color>");

            Node startNode = _grid.NodeFromWorldPoint(_motor.Position);
            _currentX = startNode.GridX;
            _currentY = startNode.GridY;
            Debug.Log($"[ZigZag] Начальная позиция: ячейка [{_currentX}, {_currentY}]");

            while (!token.IsCancellationRequested)
            {
                Node currentNode = _grid.GetNodeFromIndices(_currentX, _currentY);
                if (!currentNode.IsCleaned)
                {
                    currentNode.IsCleaned = true;
                    Debug.Log($"[ZigZag] Закрасили ячейку [{_currentX}, {_currentY}]");
                }

                int nextX = _currentX + _directionX;
                int nextY = _currentY;
                bool moveSuccessful = false;

                // Попытка 1: Шаг вперед по ряду
                if (IsValidWalkableAndDirty(nextX, nextY))
                {
                    Debug.Log($"[ZigZag] Путь свободен. Идем по ряду в [{nextX}, {nextY}]");
                    _currentX = nextX;
                    moveSuccessful = true;
                }
                else
                {
                    Debug.Log($"[ZigZag] Впереди препятствие или край [{nextX}, {nextY}]. Пробуем сдвинуться на ряд ВВЕРХ.");
                    nextX = _currentX;
                    nextY = _currentY + 1;

                    if (IsValidWalkableAndDirty(nextX, nextY))
                    {
                        _currentY = nextY;
                        _directionX = -_directionX;
                        moveSuccessful = true;
                        Debug.Log($"[ZigZag] Успешно сдвинулись ВВЕРХ на [{nextX}, {nextY}]. Новое направление X: {_directionX}");
                    }
                    else
                    {
                        Debug.Log($"[ZigZag] ВВЕРХ нельзя. Пробуем сдвинуться ВНИЗ.");
                        nextY = _currentY - 1;
                        if (IsValidWalkableAndDirty(nextX, nextY))
                        {
                            _currentY = nextY;
                            _directionX = -_directionX;
                            moveSuccessful = true;
                            Debug.Log($"[ZigZag] Успешно сдвинулись ВНИЗ на [{nextX}, {nextY}]. Новое направление X: {_directionX}");
                        }
                    }
                }

                if (!moveSuccessful)
                {
                    Debug.LogWarning($"<color=orange>[ZigZag] ТУПИК вокруг ячейки [{_currentX}, {_currentY}]. Ищу транзит по A*...</color>");
                    
                    Node nextDirtyNode = FindNearestDirtyNode();

                    if (nextDirtyNode == null)
                    {
                        Debug.Log("<color=green>[ZigZag] КОМНАТА ПОЛНОСТЬЮ УБРАНА! Жду новый мусор...</color>");
                        _motor.Stop();
                        
                        // Робот засыпает и каждый кадр проверяет, не намусорил ли WaveManager
                        await UniTask.WaitUntil(() => FindNearestDirtyNode() != null, PlayerLoopTiming.Update, token);
                        
                        Debug.Log("[ZigZag] Обнаружен новый мусор! Возобновляю уборку.");
                        continue; // Начинаем цикл while заново!
                    }

                    List<Node> path = _pathfinder.FindPath(_motor.Position, nextDirtyNode.WorldPosition);

                    if (path != null && path.Count > 0)
                    {
                        Debug.Log($"[ZigZag] Транзит найден! Едем в ячейку [{nextDirtyNode.GridX}, {nextDirtyNode.GridY}] (Длина пути: {path.Count})");
                        _grid.CurrentPath = path; 

                        foreach (Node pathNode in path)
                        {
                            if (token.IsCancellationRequested) break;
                            await MoveToNodeAsync(pathNode.WorldPosition, token, isTransit: true);
                        }

                        _grid.CurrentPath = null;
                        
                        _currentX = nextDirtyNode.GridX;
                        _currentY = nextDirtyNode.GridY;
                        Debug.Log($"[ZigZag] Транзит окончен. Продолжаем змейку из [{_currentX}, {_currentY}]");
                        continue;
                    }
                    else
                    {
                        Debug.LogError($"[ZigZag] A* не смог проложить путь к грязной зоне [{nextDirtyNode.GridX}, {nextDirtyNode.GridY}]! Отмечаем ее как недоступную.");
                        nextDirtyNode.IsCleaned = true;
                        continue;
                    }
                }

                // Едем в выбранную ячейку
                Vector3 targetPos = _grid.GetNodeFromIndices(_currentX, _currentY).WorldPosition;
                Debug.Log($"[ZigZag] Начинаю физическое движение к мировым координатам {targetPos}");
                await MoveToNodeAsync(targetPos, token, isTransit: false);
            }
        }

        private bool IsValidWalkableAndDirty(int x, int y)
        {
            if (x < 0 || x >= _grid.GridSizeX || y < 0 || y >= _grid.GridSizeY) return false;
            Node node = _grid.GetNodeFromIndices(x, y);
            return node.IsWalkable && !node.IsCleaned;
        }

        private Node FindNearestDirtyNode()
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
                        // Считаем реальную физическую дистанцию до грязной ячейки
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

        private async UniTask MoveToNodeAsync(Vector3 targetPos, CancellationToken token, bool isTransit)
        {
            Vector3 targetPosFlat = new Vector3(targetPos.x, 0, targetPos.z);
            
            // Защита от дрифта: цикл подруливания
            while (!token.IsCancellationRequested)
            {
                Vector3 currentPosFlat = new Vector3(_motor.Position.x, 0, _motor.Position.z);
                float distance = Vector3.Distance(currentPosFlat, targetPosFlat);
                
                // Если мы уже в радиусе ячейки - выходим из метода (успех!)
                if (distance < 0.4f)
                {
                    return;
                }

                Vector3 direction = targetPosFlat - currentPosFlat;
                if (direction.sqrMagnitude > 0.001f)
                {
                    float angle = Vector3.SignedAngle(_motor.Forward, direction.normalized, Vector3.up);
                    
                    // Если робот отклонился больше чем на 10 градусов - жестко стопорим и корректируем!
                    if (Mathf.Abs(angle) > 25f)
                    {
                        if (!isTransit) Debug.Log($"[Motor] Отклонение {angle:F1} градусов. Корректирую курс...");
                        _motor.Stop();
                        try { await _motor.RotateAsync(angle, token).Timeout(System.TimeSpan.FromSeconds(1f)); }
                        catch (System.TimeoutException) { Debug.LogWarning("[Motor] Таймаут поворота!"); }
                    }
                }

                _motor.MoveForward(_config.MoveSpeed);

                // Едем маленькими "тиками", чтобы постоянно проверять угол и дистанцию
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
            }
        }
    }
}