using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VacuumSim.Robotics.Contracts;
using VacuumSim.Robotics.Configs;
using VacuumSim.Pathfinding;

namespace VacuumSim.Robotics.Brain.Strategies
{
    public class SpiralStrategy : ICleaningStrategy
    {
        private readonly IVacuumMotor _motor;
        private readonly VacuumConfig _config;
        private readonly PathfindingGrid _grid;
        private readonly Pathfinder _pathfinder;

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

        public SpiralStrategy(
            IVacuumMotor motor, VacuumConfig config, 
            PathfindingGrid grid, Pathfinder pathfinder)
        {
            _motor = motor;
            _config = config;
            _grid = grid;
            _pathfinder = pathfinder;
        }

        public async UniTask ExecuteAsync(CancellationToken token)
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

                // Пытаемся сделать шаг. Если впереди стена - поворачиваем на 90 градусов вправо.
                // Если сделали 4 поворота (360 градусов) и все равно тупик - выходим из цикла попыток.
                while (turnsAttempts < 4)
                {
                    int nextX = _currentX + _directions[_currentDirIndex].x;
                    int nextY = _currentY + _directions[_currentDirIndex].y;

                    if (IsValidWalkableAndDirty(nextX, nextY))
                    {
                        _currentX = nextX;
                        _currentY = nextY;
                        moveSuccessful = true;
                        break; // Нашли путь, выходим из цикла поворотов
                    }
                    else
                    {
                        // Поворот на 90 градусов по часовой стрелке
                        _currentDirIndex = (_currentDirIndex + 1) % 4;
                        turnsAttempts++;
                    }
                }

                // Тупик (все 4 стороны заблокированы или убраны)
                if (!moveSuccessful)
                {
                    Debug.LogWarning($"<color=orange>[Spiral] Локальный тупик. Ищу новую зону через A*...</color>");
                    
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
            
            // 1. ИДЕАЛЬНЫЙ ПРИЦЕЛ: Поворачиваемся ДО того, как начнем движение
            Vector3 currentPosFlat = new Vector3(_motor.Position.x, 0, _motor.Position.z);
            Vector3 direction = targetPosFlat - currentPosFlat;
            
            if (direction.sqrMagnitude > 0.001f)
            {
                // Целимся очень точно (с погрешностью всего в 2 градуса)
                float initialAngle = Vector3.SignedAngle(_motor.Forward, direction.normalized, Vector3.up);
                if (Mathf.Abs(initialAngle) > 2f) 
                {
                    _motor.Stop();
                    try { await _motor.RotateAsync(initialAngle, token).Timeout(System.TimeSpan.FromSeconds(1f)); }
                    catch (System.TimeoutException) { Debug.LogWarning("[Motor] Таймаут начального поворота!"); }
                }
            }

            // 2. УВЕРЕННЫЙ РАЗГОН: Едем вперед без микро-подруливаний
            while (!token.IsCancellationRequested)
            {
                currentPosFlat = new Vector3(_motor.Position.x, 0, _motor.Position.z);
                float distance = Vector3.Distance(currentPosFlat, targetPosFlat);
                
                // Если мы уже в радиусе ячейки - выходим из метода (успех!)
                if (distance < 0.4f)
                {
                    return;
                }

                // АВАРИЙНЫЙ ПРЕДОХРАНИТЕЛЬ: Подруливаем на ходу ТОЛЬКО если нас сильно отбросило физикой (> 45 градусов)
                Vector3 currentDirection = targetPosFlat - currentPosFlat;
                if (currentDirection.sqrMagnitude > 0.001f)
                {
                    float currentAngle = Vector3.SignedAngle(_motor.Forward, currentDirection.normalized, Vector3.up);
                    if (Mathf.Abs(currentAngle) > 45f)
                    {
                        if (!isTransit) Debug.Log($"[Motor] Аварийное отклонение {currentAngle:F1} градусов. Корректирую курс...");
                        _motor.Stop();
                        try { await _motor.RotateAsync(currentAngle, token).Timeout(System.TimeSpan.FromSeconds(1f)); }
                        catch (System.TimeoutException) { Debug.LogWarning("[Motor] Таймаут аварийного поворота!"); }
                    }
                }

                // Просто уверенно жмем на газ!
                _motor.MoveForward(_config.MoveSpeed);
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
            }
        }
    }
}