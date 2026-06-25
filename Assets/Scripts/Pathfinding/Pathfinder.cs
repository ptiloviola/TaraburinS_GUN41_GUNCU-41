using System.Collections.Generic;
using UnityEngine;

namespace VacuumSim.Pathfinding
{
    public class Pathfinder
    {
        private readonly PathfindingGrid _grid;

        // Конструктор: Искателю пути обязательно нужна карта (сетка) для работы
        public Pathfinder(PathfindingGrid grid)
        {
            _grid = grid;
        }

        // Главный метод: Дай мне точку А и точку Б, и я верну список узлов для проезда
        public List<Node> FindPath(Vector3 startPos, Vector3 targetPos)
        {
            Node startNode = _grid.NodeFromWorldPoint(startPos);
            Node targetNode = _grid.NodeFromWorldPoint(targetPos);

            // OpenSet - ячейки, которые мы планируем проверить
            List<Node> openSet = new List<Node>();
            
            // ClosedSet - ячейки, которые мы уже проверили (чтобы не ходить кругами)
            HashSet<Node> closedSet = new HashSet<Node>();
            
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                // Ищем ячейку с наименьшим F-Cost
                Node currentNode = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].FCost < currentNode.FCost || 
                        openSet[i].FCost == currentNode.FCost && openSet[i].HCost < currentNode.HCost)
                    {
                        currentNode = openSet[i];
                    }
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                // Если мы дошли до цели — восстанавливаем путь и возвращаем его!
                if (currentNode == targetNode)
                {
                    return RetracePath(startNode, targetNode);
                }

                // Перебираем всех соседей текущей ячейки
                foreach (Node neighbor in _grid.GetNeighbors(currentNode))
                {
                    // Если туда нельзя проехать или мы там уже были - пропускаем
                    if (!neighbor.IsWalkable || closedSet.Contains(neighbor))
                        continue;

                    // Считаем стоимость шага в эту соседнюю ячейку
                    int newMovementCostToNeighbor = currentNode.GCost + GetDistance(currentNode, neighbor);
                    
                    if (newMovementCostToNeighbor < neighbor.GCost || !openSet.Contains(neighbor))
                    {
                        // Обновляем данные соседа
                        neighbor.GCost = newMovementCostToNeighbor;
                        neighbor.HCost = GetDistance(neighbor, targetNode);
                        neighbor.Parent = currentNode; // Запоминаем, откуда пришли!

                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }
            
            // Если мы вышли из цикла, значит путь не найден (робот замурован)
            return null;
        }

        // Метод, который разматывает клубок обратно от финиша к старту
        private List<Node> RetracePath(Node startNode, Node endNode)
        {
            List<Node> path = new List<Node>();
            Node currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;
            }
            
            // Переворачиваем список, чтобы он шел от старта к финишу
            path.Reverse();
            return path;
        }

        // Хитрый метод подсчета дистанции на сетке
        // 10 = шаг по прямой (горизонталь/вертикаль)
        // 14 = шаг по диагонали (примерный корень из 2 * 10)
        private int GetDistance(Node nodeA, Node nodeB)
        {
            int dstX = Mathf.Abs(nodeA.GridX - nodeB.GridX);
            int dstY = Mathf.Abs(nodeA.GridY - nodeB.GridY);

            if (dstX > dstY)
                return 14 * dstY + 10 * (dstX - dstY);
                
            return 14 * dstX + 10 * (dstY - dstX);
        }
    }
}